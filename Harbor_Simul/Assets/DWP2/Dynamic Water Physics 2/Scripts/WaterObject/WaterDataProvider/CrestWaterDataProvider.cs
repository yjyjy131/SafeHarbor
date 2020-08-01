using System.Collections.Generic;
using UnityEngine;

#if DWP_CREST
using Crest;
#endif

namespace DWP2
{
    public class CrestWaterDataProvider : WaterDataProvider
    {
        #if DWP_CREST
        private SampleHeightHelper _sampleHeightHelper;

        private OceanRenderer _oceanRenderer;
        private ICollProvider _collProvider;
        private Vector3 _worldPos;
        
        private Vector3[] _worldP0s;
        private Vector3[] _worldP1s;
        private Vector3[] _worldP2s;
        private Vector3[] _groupedPs;
        private int _prevArraySize;
        private Matrix4x4 _trsMatrix;

        private int _hash0;
        private int _hash1;
        private int _hash2;
        
        Vector3[] _transformedPoints0Scratch;
        Vector3[] _transformedPoints1Scratch;
        Vector3[] _transformedPoints2Scratch;

        public override void Initialize()
        {
            base.Initialize();

            if (waterObject == null)
            {
                Debug.LogWarning("No objects tagged 'Water' found.");
                return;
            }

            _oceanRenderer = OceanRenderer.Instance;
            if (_oceanRenderer == null)
            {
                Debug.LogError(
                    "A gameobject tagged 'Water' has been found but it does not contain CREST's OceanRenderer component. " +
                    "You have defined DWP_CREST and therefore CREST's OceanRenderer component is required. ");
                return;
            }

            _collProvider = _oceanRenderer.CollisionProvider;
            _sampleHeightHelper = new SampleHeightHelper();
            
            // Generate hopefully unique hashes for Crest query
            _hash0 = Random.Range(0, 2147483641);
            _hash1 = Random.Range(0, 2147483641);
            _hash2 = Random.Range(0, 2147483641);
        }
        
        public override void GetWaterHeights(ref Vector3[] p0s, ref Vector3[] p1s, ref Vector3[] p2s,
            ref float[] waterHeights0, ref float[] waterHeights1, ref float[] waterHeights2, ref Matrix4x4[] localToWorldMatrices)
        {
            Debug.Assert(waterObject != null, $"There is no 'Lux Water_Water Volume' present in the scene with tag {waterObjectTag}");
            Debug.Assert(p0s.Length == waterHeights0.Length, "Points and WaterHeights arrays must have same length.");
            
            int n = p0s.Length;

            // Resize arrays if data size changed
            if (n != _prevArraySize)
            {
                _worldP0s = new Vector3[n];
                _worldP1s = new Vector3[n];
                _worldP2s = new Vector3[n];
                _groupedPs = new Vector3[n];
                
                _transformedPoints0Scratch = new Vector3[n];
                _transformedPoints1Scratch = new Vector3[n];
                _transformedPoints2Scratch = new Vector3[n];
            }
            
            // Pre-calculate world positions
            for (int i = 0; i < n; i++)
            {
                _trsMatrix = localToWorldMatrices[i];
                _worldP0s[i] = _trsMatrix.MultiplyPoint3x4(p0s[i]);
                _worldP1s[i] = _trsMatrix.MultiplyPoint3x4(p1s[i]);
                _worldP2s[i] = _trsMatrix.MultiplyPoint3x4(p2s[i]);
            }

            if (groupWaterQueries)
            {
                // Pre-calculate query points to avoid multiple calls to Crest
                for (int i = 0; i < n; i++)
                {
                    _groupedPs[i] = (_worldP0s[i] + _worldP1s[i] + _worldP2s[i]) / 3f;
                }
                
                // Query Crest
                _collProvider.Query(_transformedPoints0Scratch.GetHashCode(), 0, _groupedPs, waterHeights0, null, null);
                waterHeights1 = waterHeights2 = waterHeights0;
            }
            else
            {
                // Query Crest
                _collProvider.Query(_transformedPoints0Scratch.GetHashCode(), 0, _worldP0s, waterHeights0, null, null);
                _collProvider.Query(_transformedPoints1Scratch.GetHashCode(), 0, _worldP1s, waterHeights1, null, null);
                _collProvider.Query(_transformedPoints2Scratch.GetHashCode(), 0, _worldP2s, waterHeights2, null, null);
            }

            _prevArraySize = n;
        }

        /// <summary>
        /// Single water height call. Do not use for batch queries.
        /// </summary>
        public override float GetWaterHeight(Vector3 position)
        {
            float height = 0;
            _sampleHeightHelper.Init(position, 0f);
            _sampleHeightHelper.Sample(ref height);
            return height;
        }
#endif
    }
}

