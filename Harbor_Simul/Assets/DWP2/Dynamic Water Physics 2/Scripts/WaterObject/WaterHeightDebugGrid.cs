using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2
{
    /// <summary>
    /// Helper scripts to debug water heights in wavy water assets.
    /// Attach to a transform and press play.
    /// </summary>
    public class WaterHeightDebugGrid : MonoBehaviour
    {
        public const int GRID_WIDTH = 30;
        public const int GRID_SIZE = GRID_WIDTH * GRID_WIDTH;
        public string waterObjectTag = "Water";
        public WaterDataProvider waterDataProvider;

        private Vector3[] p0s = new Vector3[GRID_SIZE / 3];
        private Vector3[] p1s = new Vector3[GRID_SIZE / 3];
        private Vector3[] p2s = new Vector3[GRID_SIZE / 3];
        private float[] waterHeights0 = new float[GRID_SIZE / 3];
        private float[] waterHeights1 = new float[GRID_SIZE / 3];
        private float[] waterHeights2 = new float[GRID_SIZE / 3];
        private Matrix4x4[] localToWorldMatrices = new Matrix4x4[GRID_SIZE];
        
        void Start()
        {
            WaterObjectManager.InitializeWaterDataProvider(ref waterDataProvider, waterObjectTag);

            for (int i = 0; i < GRID_SIZE; i++)
            {
                localToWorldMatrices[i] = transform.localToWorldMatrix;
            }

            Vector3[] positions = new Vector3[GRID_SIZE];
            for (int i = 0; i < GRID_WIDTH; i++)
            {
                for (int j = 0; j < GRID_WIDTH; j++)
                {
                    int index = i * GRID_WIDTH + j;
                    positions[index] = new Vector3(j, 0, i);
                }
            }
            
            int n = GRID_SIZE / 3;
            Array.Copy(positions, 0, p0s, 0, n);
            Array.Copy(positions, n, p1s, 0, n);
            Array.Copy(positions, 2 * n, p2s, 0, n);
        }
        
        void Update()
        {
            waterDataProvider.GetWaterHeights(ref p0s, ref p1s, ref p2s, ref waterHeights0, ref waterHeights1, ref waterHeights2, ref localToWorldMatrices);
        }

        private void OnDrawGizmos()
        {
            int n = GRID_SIZE / 3;
            
            for (int i = 0; i < n; i++)
            {
                Gizmos.color = Color.white;
                Vector3 p1 = transform.TransformPoint(p1s[i]);
                p1.y = waterHeights1[i];
                Gizmos.DrawSphere(p1, 0.1f);
            }
        }
    }
}

