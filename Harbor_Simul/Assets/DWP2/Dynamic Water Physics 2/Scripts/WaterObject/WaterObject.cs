using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DWP2
{
    /// <summary>
    /// Data holder class for water objects.
    /// All physics calculations are done inside WaterObjectManager.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class WaterObject : MonoBehaviour
    {
        /// <summary>
        /// Should the simulation mesh be simplified / decimated?
        /// </summary>
        public bool simplifyMesh = true;
        
        /// <summary>
        /// Should the simulation mesh be made convex?
        /// </summary>
        public bool convexifyMesh = true;
        
        /// <summary>
        /// Target triangle count for the simulation mesh.
        /// Original mesh will be decimated to this number of triangles is "SimplifyMesh" is enabled.
        /// Otherwise does nothing.
        /// </summary>
        [Range(8, 256)] public int targetTris = 64;
        
        [SerializeField] private float _density = 400f;
        public float Density => _density;
        
        [SerializeField] private float _mass = 0;
        public float Mass => _mass;
        
        [SerializeField] private float _volume = 0;
        public float Volume => _volume;

        [SerializeField] private int _waterObjectMaterialIndex = 0;
        /// <summary>
        /// Index of the currently active water object density preset / material.
        /// </summary>
        public int WaterObjectMaterialIndex
        {
            get => _waterObjectMaterialIndex;
            set
            {
                _waterObjectMaterialIndex = Mathf.Clamp(value, 0, WaterObjectMaterials.Materials.Length);
                if (Material.density > 0) SetMaterialDensity(Material.density);
            }
        }

        /// <summary>
        /// Currently active water object density preset / material.
        /// </summary>
        public WaterObjectMaterials.WaterObjectMaterial Material => WaterObjectMaterials.Materials[WaterObjectMaterialIndex];

        private MeshFilter _meshFilter;
        
        /// <summary>
        /// Rigidbody that the forces will be applied to.
        /// </summary>
        public Rigidbody TargetRigidbody { get; set; }

        [SerializeField] private Mesh _originalMesh;
        /// <summary>
        /// Original mesh of the object, non-simplified and non-convexified.
        /// </summary>
        public Mesh OriginalMesh => _originalMesh;
        
        [SerializeField] private Mesh _simulationMesh;
        /// <summary>
        /// Mesh used to simulate water physics.
        /// </summary>
        public Mesh SimulationMesh => _simulationMesh;

        /// <summary>
        /// Is the object initialized?
        /// </summary>
        private bool _initialized = false;
        public bool Initialized => _initialized;

        /// <summary>
        /// Is the simulation mesh preview enabled?
        /// </summary>
        public bool PreviewEnabled => _meshFilter == null ? false : _meshFilter.sharedMesh.name == "DWP_SIM_MESH";
        
        private float _simplificationRatio;
        /// <summary>
        /// Percentage of triangles of the original mesh [0...1] that the simulation mesh will try to achieve.
        /// </summary>
        public float SimplificationRatio => _simplificationRatio;

        /// <summary>
        /// Number of triangles in simulation mesh.
        /// </summary>
        public int TriangleCount => _simulationMesh == null ? 0 : _simulationMesh.triangles.Length / 3;

        [SerializeField] private SerializedMesh _serializedSimulationMesh;
        public SerializedMesh SerializedSimulationMesh => _serializedSimulationMesh;
        
        // Editor
        public static bool showEditorWarnings = true;
        [SerializeField] private bool editorHasErrors = false;

        // Data Readback
        private int _dataStart = -1;
        /// <summary>
        /// Start index of this object's data in WaterObjectManager's arrays.
        /// </summary>
        public int DataStart
        {
            get => _dataStart;
            set => _dataStart = value;
        }
        
        private int _dataLength = -1;
        /// <summary>
        /// Length of this object's data.
        /// </summary>
        public int DataLength
        {
            get => _dataLength;
            set => _dataLength = value;
        }

        public int DataEnd => _dataStart + _dataLength;

        private int _woArrayIndex;
        public int WoArrayIndex
        {
            get => _woArrayIndex;
            set => _woArrayIndex = value;
        }

        public bool DataInitialized => _dataLength >= 0 && _dataStart >= 0;
        
        /// <summary>
        /// Returns the states of triangles.
        /// 0 - Triangle is under water
        /// 1 - Triangle is partially under water
        /// 2 - Triangle is above water
        /// 3 - Triangle's object is disabled
        /// 4 - Triangle's object is deleted
        /// </summary>
        /// <param name="states"></param>
        public void GetStates(ref byte[] states)
        {
            if (!DataInitialized) return;
            Debug.Assert(states.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, states, 0, DataLength);
        }
        
        /// <summary>
        /// Force application points. 
        /// </summary>
        public void GetPoints(ref Vector3[] points)
        {
            if (!DataInitialized) return;
            Debug.Assert(points.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, points, 0, DataLength);
        }
        
        /// <summary>
        /// Triangle normals
        /// </summary>
        public void GetNormals(ref Vector3[] normals)
        {
            if (!DataInitialized) return;
            Debug.Assert(normals.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, normals, 0, DataLength);
        }
        
        /// <summary>
        /// Triangle areas
        /// </summary>
        public void GetAreas(ref Vector3[] areas)
        {
            if (!DataInitialized) return;
            Debug.Assert(areas.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, areas, 0, DataLength);
        }
        
        /// <summary>
        /// Velocities at centers of triangles
        /// </summary>
        public void GetVelocities(ref Vector3[] velocities)
        {
            if (!DataInitialized) return;
            Debug.Assert(velocities.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, velocities, 0, DataLength);
        }
        
        /// <summary>
        /// Distance from triangle to water surface
        /// </summary>
        public void GetDistances(ref Vector3[] distances)
        {
            if (!DataInitialized) return;
            Debug.Assert(distances.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, distances, 0, DataLength);
        }
        
        public void GetP00S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        public void GetP01S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        public void GetP02S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        public void GetP10S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        public void GetP11S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        public void GetP12S(ref Vector3[] ps)
        {
            if (!DataInitialized) return;
            Debug.Assert(ps.Length == DataLength, "Size mismatch. Array must have length of 'WaterObject.DataLength'");
            Array.Copy(WaterObjectManager.Instance.States, DataStart, ps, 0, DataLength);
        }
        
        /// <summary>
        /// Sets mass of the object and adjusts density of the material to be correct for the volume of the mesh.
        /// </summary>
        public void SetMaterialMass(float mass)
        {
            _mass = Mathf.Clamp(mass, 0.001f, Mathf.Infinity);
            _density = _mass / _volume;
            TargetRigidbody?.GetComponent<RigidbodyMassFromChildren>()?.Calculate();
        }

        /// <summary>
        /// Sets density of the material and adjusts mass to be correct for the volume of the mesh.
        /// </summary>
        public void SetMaterialDensity(float density)
        {
            _density = density;
            _mass = _density * _volume;
            if(TargetRigidbody != null) TargetRigidbody.GetComponent<RigidbodyMassFromChildren>()?.Calculate();
        }

        /// <summary>
        /// Updates volume of the simulation mesh. Also changes mass to be correct for the given density.
        /// </summary>
        public void UpdateVolume()
        {
            _volume = CalculateSimulationMeshVolume();
            _mass = _density * _volume;
        }

        private void OnDisable()
        {
            if (Application.isEditor)
            {
                StopSimMeshPreview();
            }
        }

        private void OnDestroy()
        {
            if (WaterObjectManager.Instance != null)
            {
                WaterObjectManager.Instance.IsDestroyed[WoArrayIndex] = true;
            }
            
            if (Application.isEditor)
            {
                StopSimMeshPreview();
            }
        }

        public void Init()
        {
            _initialized = false;

            if (editorHasErrors)
            {
                Debug.LogError($"WaterObject {name} has setup errors. It will not be simulated. If you have fixed the error but this message still shows, select the WaterObject in question so that editor can refresh.");
                return;
            }

            TargetRigidbody = transform.FindRootRigidbody();
            if (TargetRigidbody == null)
            {
                Debug.LogError($"TargetRigidbody on object {name} is null.");
                return;
            }

            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                Debug.LogError($"MeshFilter on object {name} is null.");
                return;
            }

            if (PreviewEnabled)
            {
                StopSimMeshPreview();
            }

            int colliderCount = TargetRigidbody.transform.GetComponentsInChildren<Collider>(true).Length;
            if (colliderCount == 0)
            {
                Debug.LogError($"{TargetRigidbody.name} has 0 colliders.");
                return;
            }

            if (!PreviewEnabled)
            {
                _originalMesh = _meshFilter.sharedMesh;

                if (_originalMesh == null)
                {
                    Debug.LogError($"MeshFilter on object {name} does not have a valid mesh assigned.");
                    ShowErrorMessage();
                    return;
                }

                if (_simulationMesh == null)
                {
                    _simulationMesh = _serializedSimulationMesh.Deserialize();
                    if (_simulationMesh == null)
                    {
                        _simulationMesh = MeshUtility.GenerateMesh(_originalMesh.vertices, _originalMesh.triangles);
                    }
                }

                // Serialize only if mesh has been modified
                _simulationMesh.name = "DWP_SIM_MESH";
                _serializedSimulationMesh.Serialize(_simulationMesh);
                UpdateVolume();
            }
            
            Debug.Assert(SimulationMesh != null, $"Simulation mesh is null on object {name}.");

            if (editorHasErrors)
            {
                Debug.LogError($"WaterObject {name} has setup errors. Will not simulate.");
                return;
            }

            Debug.Assert(SimulationMesh.GetInstanceID() != OriginalMesh.GetInstanceID(), 
                $"Simulation mesh and original mesh have same Instance ID on object {name}. !BUG!.");
            
            _initialized = true;
        }
        
        /// <summary>
        /// Generates simulation mesh according to the settings
        /// </summary>
        public void GenerateSimMesh()
        {
            bool previewWasEnabled = false;
            
            if (PreviewEnabled)
            {
                StopSimMeshPreview();
                previewWasEnabled = true;
            }

            if (!PreviewEnabled)
            {
                if (!Initialized)
                {
                    Init();
                    if (!Initialized)
                    {
                        Debug.LogError($"Could not generate dummy mesh for object {name}. WaterObject could not be initialized" +
                                       " due to setup errors above. Fix these errors before trying to generate dummy mesh.");
                        return;
                    }
                }

                if (_simulationMesh == null)
                {
                    _simulationMesh = new Mesh
                    {
                        name = "DWP_SIM_MESH"
                    };
                }

                if (simplifyMesh)
                {
                    _simplificationRatio = (targetTris * 3f + 16) / (float)_originalMesh.triangles.Length;
                    if (_simplificationRatio >= 1f && !convexifyMesh)
                    {
                        Debug.Log("Target tri count larger than the original tri count. Nothing to simplify.");
                        return;
                    }
                    _simplificationRatio = Mathf.Clamp(_simplificationRatio, 0f, 1f);
                }
                else
                {
                    // Return if both simplify and convexify are disabled
                    if (!convexifyMesh)
                    {
                        _simulationMesh = MeshUtility.GenerateMesh(_originalMesh.vertices, _originalMesh.triangles);
                        return;
                    }
                }

                MeshUtility.GenerateDummyMesh(ref _originalMesh, ref _simulationMesh, simplifyMesh, convexifyMesh, _simplificationRatio);
                _serializedSimulationMesh.Serialize(_simulationMesh);
            }
            else
            {
                Debug.LogError("Cannot generate simulation mesh while preview is enabled.");
            }

            if (previewWasEnabled)
            {
                StartSimMeshPreview();
            }
        }
        
        /// <summary>
        /// Swaps original mesh with simulation mesh on MeshFilter for in-scene preview.
        /// </summary>
        public void StartSimMeshPreview()
        {
            if (PreviewEnabled) return;

            if (_simulationMesh == null)
            {
                Debug.LogError("Could not start simulation mesh preview. Simulation mesh is null.");
                return;
            }
            
            if (_originalMesh == null)
            {
                Debug.LogError("Could not start simulation mesh preview. Original mesh is null.");
                return;
            }
            
            if (!Initialized)
            {
                Debug.LogError("Can not show dummy mesh preview for object {name}. WaterObject could not be initialized" +
                               " due to setup errors above. Fix these errors before trying to generate dummy mesh.");
                return;
            }

            if (_simulationMesh != null)
            {
                _meshFilter.sharedMesh = _simulationMesh;
            }
        }
        
        /// <summary>
        /// Swaps simulation mesh on MeshFilter with original mesh.
        /// </summary>
        public void StopSimMeshPreview()
        {
            if (!PreviewEnabled) return;
            
            if (_meshFilter == null || _originalMesh == null)
            {
                Debug.LogError($"Cannot stop sim mesh preview on object {name}. MeshFilter or original mesh is null.");
                return;
            }

            if (_originalMesh != null)
            {
                _meshFilter.sharedMesh = _originalMesh;
            }
            else
            {
                Debug.LogError($"Original mesh on object {name} could not be found. !BUG!");
            }
        }
        
        /// <summary>
        /// Shows setup error message.
        /// </summary>
        private void ShowErrorMessage()
        {
            Debug.LogWarning($"One or more setup errors have been found. WaterObject {name} will not be " +
                             $"simulated until these are fixed. Check manual for more details on WaterObject setup.");
        }
        
        /// <summary>
        /// Gets volume of the simulation mesh. Scale-sensitive.
        /// </summary>
        private float CalculateSimulationMeshVolume()
        {
            return SimulationMesh == null ? 0.00000001f : Mathf.Clamp(MeshUtility.VolumeOfMesh(SimulationMesh, transform), 0f, Mathf.Infinity);
        }

        private void OnDrawGizmosSelected()
        {
            if (!DataInitialized || !WaterObjectManager.GenerateGizmos) return;
            
            Gizmos.color = Color.yellow;
            WaterObjectManager wom = WaterObjectManager.Instance;
            for (int i = _dataStart; i < _dataLength; i++)
            {
                Gizmos.DrawLine(wom.P02S[i], wom.P01S[i]);
            }
        }
    }
}

