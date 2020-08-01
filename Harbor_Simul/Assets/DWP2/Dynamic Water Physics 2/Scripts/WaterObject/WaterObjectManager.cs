using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

namespace DWP2
{
    /// <summary>
    /// Main class for data processing and triangle job management. Handles all the physics calculations.
    /// </summary>
    [Serializable]
    public class WaterObjectManager : MonoBehaviour
    {
        public static bool GenerateGizmos;

        [HideInInspector] public UnityEvent OnSynchronize = new UnityEvent();
        
        public static WaterObjectManager Instance;
        public WaterDataProvider waterDataProvider;

        /// <summary>
        /// Density of the fluid the object is in. Affects only buoyancy.
        /// </summary>
        public float fluidDensity = 1030f;

        /// <summary>
        /// Coefficient by which all the non-buoyancy forces will be multiplied.
        /// </summary>
        [Range(0.5f, 1.5f)]
        public float dynamicForceCoefficient = 1f;
        
        /// <summary>
        /// Set to 1 for linear force/velocity ratio or >1 for exponential ratio.
        /// If higher than one forces will increase exponentially with speed.
        /// </summary>
        [Range(0.5f, 2f)]
        public float dynamicForcePower = 1f;
        
        /// <summary>
        /// Resistant force exerted on an object moving in a fluid. Skin friction drag is caused by the viscosity of fluids and is developed as a fluid moves on the surface of an object.
        /// </summary>
        [Range(0f, 0.2f)]
        public float skinFrictionDrag = 0.01f;
        
        /// <summary>
        /// Set to 1 for linear dot/force ratio or other than 1 for exponential ratio.
        /// When force is calculated it is multiplied by the dot value between normal of the surface and the velocity.
        /// High power values will result in shallow angles between the two having less of an effect on the final force, and vice versa.
        /// </summary>
        [Range(0.5f, 2f)]
        public float velocityDotPower = 1f;

        /// <summary>
        /// If true water heights will be queried at each vertex instead of triangle centers.
        /// </summary>
        public bool highResolutionWaterQueries = false;
        
        /// <summary>
        /// Should water heights be queried?
        /// </summary>
        public bool queryWaterHeights = true;
        
        /// <summary>
        /// Should water velocities be queried?
        /// </summary>
        public bool queryWaterVelocities = false;
        public string waterObjectTag = "Water";
        [SerializeField] private bool generateGizmos;

        private List<WaterObject> _waterObjects = new List<WaterObject>();
        public List<WaterObject> WaterObjects => _waterObjects;
        
        private List<TriangleData> _triangleData = new List<TriangleData>();
        public List<TriangleData> TriangleData => _triangleData;

        private WaterTriangleJob _waterTriJob;
        private JobHandle _waterJobHandle;
        private Transform _water;
        private bool _scheduled;
        private bool _initialized;
        private bool _finished;
        
        // Managed data containers
        
        // Input
        private float[] waterHeights0;
        private float[] waterHeights1;
        private float[] waterHeights2;
        private Vector3[] waterVelocities0;
        private Vector3[] waterVelocities1;
        private Vector3[] waterVelocities2;
        private Matrix4x4[] localToWorldMatrices;
        private Vector3[] rbCentersOfMass;
        private Vector3[] rbLinearVelocities;
        private Vector3[] rbAngularVelocities;
        private Vector3[] p0s;
        private Vector3[] p1s;
        private Vector3[] p2s;
        
        // Output
        private bool[] _hasBeenDestroyed;
        private byte[] _states;
        private Vector3[] _forces;
        private Vector3[] _points;
        private Vector3[] _normals;
        private float[] _areas;
        private Vector3[] _velocities;
        private float[] _distances;
        private Vector3[] _P00S;
        private Vector3[] _P01S;
        private Vector3[] _P02S;
        private Vector3[] _P10S;
        private Vector3[] _P11S;
        private Vector3[] _P12S;
        
        // Indices
        private int[] _dataStarts;
        private int[] _dataLengths;
        
        public byte[] States
        {
            get => _states;
            private set => _states = value;
        }

        public bool[] IsDestroyed
        {
            get => _hasBeenDestroyed;
            set => _hasBeenDestroyed = value;
        }

        public Vector3[] Forces
        {
            get => _forces;
            private set => _forces = value;
        }

        public Vector3[] Points
        {
            get => _points;
            private set => _points = value;
        }

        public Vector3[] Normals
        {
            get => _normals;
            private set => _normals = value;
        }

        public float[] Areas
        {
            get => _areas;
            private set => _areas = value;
        }

        public Vector3[] Velocities
        {
            get => _velocities;
            private set => _velocities = value;
        }

        public float[] Distances
        {
            get => _distances;
            private set => _distances = value;
        }

        public Vector3[] P00S
        {
            get => _P00S;
            private set => _P00S = value;
        }

        public Vector3[] P01S
        {
            get => _P01S;
            private set => _P01S = value;
        }

        public Vector3[] P02S
        {
            get => _P02S;
            private set => _P02S = value;
        }

        public Vector3[] P10S
        {
            get => _P10S;
            private set => _P10S = value;
        }

        public Vector3[] P11S
        {
            get => _P11S;
            private set => _P11S = value;
        }

        public Vector3[] P12S
        {
            get => _P12S;
            private set => _P12S = value;
        }

        // Tri Stats
        public int ActiveTriCount => States == null ? 0 : States.Count(s => s <= 2);
        public int ActiveUnderwaterTriCount => States == null ? 0 : States.Count(s => s <= 1);
        public int ActiveAboveWaterTriCount => States == null ? 0 : States.Count(s => s == 2);
        public int DisabledTriCount => States == null ? 0 : States.Count(s => s == 3);
        public int DestroyedTriCount => States == null ? 0 : States.Count(s => s == 4);
        public int InactiveTriCount => States == null ? 0 : States.Count(s => s == 3 || s == 4);

        
        /// <summary>
        /// Slow. Avoid if at all possible.
        /// Instead of spawning new objects it is preferable to spawn them at start and then disable them - this way
        /// job arrays will get initialized to correct size and yet inactive objects will not be simulated.
        /// </summary>
        public void Synchronize()
        {
            Initialize();
            OnSynchronize.Invoke();
        }


        private void OnValidate()
        {
            Instance = this;
        }

        private void Awake()
        {
            // Check if there is only one WaterObjectManager
            if (FindObjectsOfType<WaterObjectManager>().Length > 1)
            {
                UnityEngine.Debug.LogError("There can be only one WaterObjectManager in the scene.");
            }
            Instance = this;

            _finished = true;
            _scheduled = false;
            _initialized = false;

            GenerateGizmos = generateGizmos;
            generateGizmos = false; // Workaround for not assigned warning
        }

        private void Start()
        {
            InitializeWaterDataProvider(ref waterDataProvider, waterObjectTag);

            // Initialize water object manager
            Initialize();
        }

        private void FixedUpdate()
        {
            waterDataProvider.groupWaterQueries = highResolutionWaterQueries;
            waterDataProvider.queryWaterHeights = queryWaterHeights;
            waterDataProvider.queryWaterVelocities = queryWaterVelocities;

            // Force finishing on the same frame (not ideal) due to Unity bug that detects fake memory leak 
            // if a job is not completed in 4 Update() frames. This means that Unity will complain about
            // data being more than 4 frames old when framerate is 4x higher than FixedUpdate() rate.
            Schedule();
            Finish();
            Process();
        }

        private void OnDestroy()
        {
            Deallocate();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Deallocate();
        }

        private void Initialize()
        {
            if(_initialized) Deallocate();

            // Get all WaterObjects
            FindSceneWaterObjects(ref _waterObjects);

            // Initialize WaterObjects
            try
            {
                foreach (WaterObject wo in _waterObjects)
                {
                    wo.Init();
                }
            }
            catch
            {
                Debug.LogWarning("Some WaterObjects have errors and will not be simulated.");
            }

            _waterObjects = _waterObjects.Where(w => w.Initialized).ToList();

            // Don't run if nothing to simulate
            int waterObjectCount = _waterObjects.Count;
            if (waterObjectCount == 0)
            {
                return;
            }

            // Initialize arrays
            _hasBeenDestroyed = new bool[waterObjectCount];
            for (int i = 0; i < waterObjectCount; i++)
            {
                _waterObjects[i].WoArrayIndex = i;
                _hasBeenDestroyed[i] = false;
            }

            // Indices
            _dataStarts = new int[waterObjectCount];
            _dataLengths = new int[waterObjectCount];
            
            // Get triangle data     
            _triangleData.Clear();
            int index = 0;
            foreach (WaterObject waterObject in _waterObjects)
            {
                waterObject.Init();
                int triCount = waterObject.SerializedSimulationMesh.triangles.Length;
                waterObject.DataStart = _triangleData.Count;
                waterObject.DataLength = triCount / 3;
                _dataStarts[index] = waterObject.DataStart;
                _dataLengths[index] = waterObject.DataLength;
                for(int i = 0; i < triCount; i += 3)
                {
                    _triangleData.Add(new TriangleData(
                        index, 
                        waterObject.SerializedSimulationMesh.vertices[waterObject.SerializedSimulationMesh.triangles[i + 0]],
                        waterObject.SerializedSimulationMesh.vertices[waterObject.SerializedSimulationMesh.triangles[i + 1]],
                        waterObject.SerializedSimulationMesh.vertices[waterObject.SerializedSimulationMesh.triangles[i + 2]],
                        waterObject));
                }

                index++;
            }

            // Allocate native arrays
            int n = _triangleData.Count;
            _waterTriJob.States = new NativeArray<byte>(n, Allocator.Persistent);
            _waterTriJob.ResultForces = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.ResultPoints = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P0S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P1S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P2S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.LocalToWorldMatrices = new NativeArray<Matrix4x4>(n, Allocator.Persistent);
            _waterTriJob.WaterHeights0 = new NativeArray<float>(n, Allocator.Persistent);
            _waterTriJob.WaterHeights1 = new NativeArray<float>(n, Allocator.Persistent);
            _waterTriJob.WaterHeights2 = new NativeArray<float>(n, Allocator.Persistent);
            _waterTriJob.WaterVelocities0 = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.WaterVelocities1 = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.WaterVelocities2 = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.Velocities = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.Normals = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.Areas = new NativeArray<float>(n, Allocator.Persistent);
            _waterTriJob.RigidbodyCOMs = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.RigidbodyAngularVels = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.RigidbodyLinearVels = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P00S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P01S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P02S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P10S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P11S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.P12S = new NativeArray<Vector3>(n, Allocator.Persistent);
            _waterTriJob.Distances = new NativeArray<float>(n, Allocator.Persistent);
            
            // Allocate managed arrays
            // Input
            p0s = new Vector3[n];
            p1s = new Vector3[n];
            p2s = new Vector3[n];
            waterHeights0 = new float[n];
            waterHeights1 = new float[n];
            waterHeights2 = new float[n];
            waterVelocities0 = new Vector3[n];
            waterVelocities1 = new Vector3[n];
            waterVelocities2 = new Vector3[n];
            localToWorldMatrices = new Matrix4x4[n];
            rbCentersOfMass = new Vector3[n];
            rbLinearVelocities = new Vector3[n];
            rbAngularVelocities = new Vector3[n];
            
            // Output
            _states = new byte[n];
            _velocities = new Vector3[n];
            _forces = new Vector3[n];
            _areas = new float[n];
            _points = new Vector3[n];
            _normals = new Vector3[n];
            _distances = new float[n];
            _P00S = new Vector3[n];
            _P01S = new Vector3[n];
            _P02S = new Vector3[n];
            _P10S = new Vector3[n];
            _P11S = new Vector3[n];
            _P12S = new Vector3[n];

            // Fill in static data
            for (int i = 0; i < n; i++)
            {
                TriangleData td = _triangleData[i];
                p0s[i] = td.p0;
                p1s[i] = td.p1;
                p2s[i] = td.p2;
                localToWorldMatrices[i] = td.Transform.localToWorldMatrix;
            }
            
            // Copy data to native arrays
            _waterTriJob.P0S.CopyFrom(p0s);
            _waterTriJob.P1S.CopyFrom(p1s);
            _waterTriJob.P2S.CopyFrom(p2s);
            _waterTriJob.LocalToWorldMatrices.CopyFrom(localToWorldMatrices);

            _initialized = true;
        }

        private void FindSceneWaterObjects(ref List<WaterObject> waterObjects)
        {
            waterObjects = new List<WaterObject>();

            foreach (WaterObject wo in Resources.FindObjectsOfTypeAll(typeof(WaterObject)))
            {
                if (wo.gameObject.scene == SceneManager.GetActiveScene())
                {
                    waterObjects.Add(wo);
                }
            }
        }

        public static void InitializeWaterDataProvider(ref WaterDataProvider waterDataProvider, string waterObjectTag)
        {
            // Decide which water data provider to use based on scripting define symbols
#if DWP_CREST
            waterDataProvider = new CrestWaterDataProvider();
            Debug.Log("DWP2: Using Crest");
#elif DWP_OCEAN_NEXT_GEN
            waterDataProvider = new OceanNextGenWaterDataProvider();
            Debug.Log("DWP2: Using Ocean Community Next Gen");
#elif DWP_LUX
            waterDataProvider = new LuxWaterDataProvider();
            Debug.Log("DWP2: Using Lux Water");
#elif DWP_SUIMONO
            waterDataProvider = new SuimonoWaterDataProvider();
            Debug.Log("DWP2: Using SUIMONO");
#elif DWP_CETO
            waterDataProvider = new CetoWaterDataProvider();
            Debug.Log("DWP2: Using Ceto Ocean");
#else
            waterDataProvider = new FlatWaterDataProvider();
            Debug.Log("DWP2: Using Flat Water (Default)");
#endif
            waterDataProvider.waterObjectTag = waterObjectTag;
            waterDataProvider.Initialize();
        }
                
        private void Schedule()
        {
            if (!_initialized || !_finished) return;
            
            // Dont run if no floating objects found
            int n = _triangleData.Count;
            if (n == 0) return;

            _finished = false;
            _scheduled = true;
            
            //  Fill in new data into managed containers
            _waterTriJob.States.CopyTo(_states);
            int waterObjectCount = _waterObjects.Count;
            for (int i = 0; i < waterObjectCount; i++)
            {
                WaterObject wo = _waterObjects[i];
                int dataStart = _dataStarts[i];
                int dataEnd = dataStart + _dataLengths[i];

                if (_hasBeenDestroyed[i])
                {
                    for (int j = dataStart; j < dataEnd; j++) _states[j] = 4;
                    continue;
                }

                // Fill in data
                byte state = wo.isActiveAndEnabled ? (byte) 2 : (byte) 3;
                Matrix4x4 cachedLocalToWorldMatrix = wo.transform.localToWorldMatrix;
                Vector3 cachedCenterOfMass = wo.TargetRigidbody.worldCenterOfMass;
                Vector3 cachedLinearVelocity = wo.TargetRigidbody.velocity;
                Vector3 cachedAngularVelocity = wo.TargetRigidbody.angularVelocity;
                
                for (int j = dataStart; j < dataEnd; j++)
                {
                    _states[j] = state;
                    localToWorldMatrices[j] = cachedLocalToWorldMatrix;
                    rbCentersOfMass[j] = cachedCenterOfMass;
                    rbLinearVelocities[j] = cachedLinearVelocity;
                    rbAngularVelocities[j] = cachedAngularVelocity;
                }
            }
            
            // Get water height
            waterDataProvider.GetWaterHeights(ref p0s, ref p1s, ref p2s, 
                ref waterHeights0, ref waterHeights1, ref waterHeights2, ref localToWorldMatrices);
            
            _waterTriJob.WaterHeights0.CopyFrom(waterHeights0);
            _waterTriJob.WaterHeights1.CopyFrom(waterHeights1);
            _waterTriJob.WaterHeights2.CopyFrom(waterHeights2);

            // Get water velocity
            waterDataProvider.GetWaterVelocities(ref p0s, ref p1s, ref p2s,
                ref waterVelocities0, ref waterVelocities1, ref waterVelocities2, ref localToWorldMatrices);
            _waterTriJob.WaterVelocities0.CopyFrom(waterVelocities0);
            _waterTriJob.WaterVelocities1.CopyFrom(waterVelocities1);
            _waterTriJob.WaterVelocities2.CopyFrom(waterVelocities2);

            // Copy new data to native containers
            _waterTriJob.States.CopyFrom(_states);
            _waterTriJob.Velocities.CopyFrom(_velocities);
            _waterTriJob.LocalToWorldMatrices.CopyFrom(localToWorldMatrices);
            _waterTriJob.RigidbodyCOMs.CopyFrom(rbCentersOfMass);
            _waterTriJob.RigidbodyLinearVels.CopyFrom(rbLinearVelocities);
            _waterTriJob.RigidbodyAngularVels.CopyFrom(rbAngularVelocities);

            // Set simulation settings
            _waterTriJob.Gravity = Physics.gravity;
            _waterTriJob.FluidDensity = fluidDensity;
            _waterTriJob.DynamicForceFactor = dynamicForceCoefficient;
            _waterTriJob.DynamicForcePower = dynamicForcePower;
            _waterTriJob.VelocityDotPower = velocityDotPower;
            _waterTriJob.SurfaceDrag = skinFrictionDrag;

            _waterJobHandle = _waterTriJob.Schedule(n, 32);
        }

        /// <summary>
        /// Manipulate data retrieved from job before job is started again.
        /// Accessing job data other than here will result in error due to job being unfinished.
        /// </summary>
        private void Process()
        {
            if (!_initialized || !_finished) return;

            int n = _triangleData.Count;
            if (n == 0) return;
            
            // Copy native arrays to managed arrays for faster access
            _waterTriJob.States.CopyTo(_states);
            _waterTriJob.ResultForces.CopyTo(_forces);
            _waterTriJob.ResultPoints.CopyTo(_points);
            _waterTriJob.Normals.CopyTo(_normals);
            _waterTriJob.Areas.CopyTo(_areas);
            _waterTriJob.Velocities.CopyTo(_velocities);
            _waterTriJob.Distances.CopyTo(_distances);
            
            _waterTriJob.P00S.CopyTo(_P00S);
            _waterTriJob.P01S.CopyTo(_P01S);
            _waterTriJob.P02S.CopyTo(_P02S);
            _waterTriJob.P10S.CopyTo(_P10S);
            _waterTriJob.P11S.CopyTo(_P11S);
            _waterTriJob.P12S.CopyTo(_P12S);

            // Apply forces
            bool initAutoSync = Physics.autoSyncTransforms;
            Physics.autoSyncTransforms = false;
            for (int i = 0; i < n; i++)
            {
                if(_states[i] >= 2) continue;
                _triangleData[i].WaterObject.TargetRigidbody.AddForceAtPosition(_forces[i], _points[i]);
            }
            Physics.autoSyncTransforms = initAutoSync;
        }

        private void Finish()
        {
            if (!_initialized || !_scheduled) return;
            
            _scheduled = false;
            _waterJobHandle.Complete();
            _finished = true;
        }

        private static void FastCopy<T>(T[] source, T[] destination) where T : struct
        {
            Array.Copy(source, destination, source.Length);
        }
        
        private void Deallocate()
        {
            if (!_initialized) return;
            _initialized = false;
            
            if (_scheduled) _waterJobHandle.Complete();
            
            _waterTriJob.ResultForces.Dispose();
            _waterTriJob.ResultPoints.Dispose();
            _waterTriJob.P0S.Dispose();
            _waterTriJob.P1S.Dispose();
            _waterTriJob.P2S.Dispose();
            _waterTriJob.LocalToWorldMatrices.Dispose();
            _waterTriJob.WaterHeights0.Dispose();
            _waterTriJob.WaterHeights1.Dispose();
            _waterTriJob.WaterHeights2.Dispose();
            _waterTriJob.WaterVelocities0.Dispose();
            _waterTriJob.WaterVelocities1.Dispose();
            _waterTriJob.WaterVelocities2.Dispose();
            _waterTriJob.Velocities.Dispose();
            _waterTriJob.Areas.Dispose();
            _waterTriJob.Normals.Dispose();
            _waterTriJob.RigidbodyAngularVels.Dispose();
            _waterTriJob.RigidbodyLinearVels.Dispose();
            _waterTriJob.RigidbodyCOMs.Dispose();
            _waterTriJob.States.Dispose();
            _waterTriJob.Distances.Dispose();
            
            _waterTriJob.P00S.Dispose();
            _waterTriJob.P01S.Dispose();
            _waterTriJob.P02S.Dispose();
            _waterTriJob.P10S.Dispose();
            _waterTriJob.P11S.Dispose();
            _waterTriJob.P12S.Dispose();
        }

        /// <summary>
        /// Returns true if point is in water. Works with wavy water too.
        /// </summary>
        public bool PointInWater(Vector3 globalPoint)
        {
            return waterDataProvider.GetWaterHeight(globalPoint) > globalPoint.y;
        }
        
        private void OnDrawGizmos()
        {
            if(!_initialized || !GenerateGizmos) return;

            if (Application.isPlaying)
            {
                Vector3 p00, p01, p02, p10, p11, p12, center;

                for (int i = 0; i < _triangleData.Count; i++)
                {
                    if(_states[i] >= 2) continue;
                    
                    Gizmos.color = Color.Lerp(Color.green, Color.red, (Forces[i].magnitude * 0.0002f) / Areas[i]);
                    
                    p00 = P00S[i];
                    p01 = P01S[i];
                    p02 = P02S[i];
                    center = Points[i];
                    if (p00 != Vector3.zero && p01 != Vector3.zero && p02 != Vector3.zero)
                    {
                        Gizmos.DrawLine(p00, p01);
                        Gizmos.DrawLine(p01, p02);
                        Gizmos.DrawLine(p02, p00);
                        Gizmos.DrawSphere(center, 0.01f);
                        Gizmos.DrawLine(center, center + Normals[i] * 0.1f);
                    }
                    
                    p10 = P10S[i];
                    p11 = P11S[i];
                    p12 = P12S[i];
                    if (p10 != Vector3.zero && p11 != Vector3.zero && p12 != Vector3.zero)
                    {
                        Gizmos.DrawLine(p10, p11);
                        Gizmos.DrawLine(p11, p12);
                        Gizmos.DrawLine(p12, p10);
                        Gizmos.DrawSphere(center, 0.01f);
                        Gizmos.DrawLine(center, center + Normals[i] * 0.1f);
                    }
                    
                    // Visualize distance to water
                    Vector3 p0 = _triangleData[i].Transform.TransformPoint(p0s[i]);
                    Vector3 p1 = _triangleData[i].Transform.TransformPoint(p1s[i]);
                    Vector3 p2 = _triangleData[i].Transform.TransformPoint(p2s[i]);
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(center, center + Vector3.up * Distances[i]);
                }
            }
        }
    }
}