using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if DWP_LUX
using LuxWater;
#endif
using UnityEngine;

namespace DWP2
{
    public class LuxWaterDataProvider : WaterDataProvider
    {
#if DWP_LUX
        public Material WaterMaterial;

        private LuxWaterUtils.GersterWavesDescription Description;

        public override void Initialize()
        {
            base.Initialize();

            WaterMaterial = waterObject.GetComponent<MeshRenderer>()?.material;
            if(WaterMaterial == null)
            {
                Debug.LogError("Lux water object does not contain a mesh renderer or material.");
            }
            LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
            waterHeightOffset = waterObject.transform.position.y;
        }
        
        public override void GetWaterHeights(ref Vector3[] p0s, ref Vector3[] p1s, ref Vector3[] p2s,
            ref float[] waterHeights0, ref float[] waterHeights1, ref float[] waterHeights2, ref Matrix4x4[] localToWorldMatrices)
        {
            Debug.Assert(waterObject != null, $"There is no 'Lux Water_Water Volume' present in the scene with tag {waterObjectTag}");
            Debug.Assert(p0s.Length == waterHeights0.Length, "Points and WaterHeights arrays must have same length.");
            
            // Update wave description
            LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
            waterHeightOffset = waterObject.transform.position.y - 0.1f;;

            int n = p0s.Length;
            float timeOffset = 0.06f;

            if (groupWaterQueries)
            {
                for (int i = 0; i < n; i++)
                {
                    waterHeights0[i] = waterHeights1[i] = waterHeights2[i] = LuxWaterUtils.GetGestnerDisplacement(localToWorldMatrices[i].MultiplyPoint3x4((p0s[i] + p1s[i] + p2s[i]) / 3f), Description, timeOffset).y + waterHeightOffset;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    waterHeights0[i] = LuxWaterUtils.GetGestnerDisplacement(localToWorldMatrices[i].MultiplyPoint3x4(p0s[i]), Description, timeOffset).y + waterHeightOffset;
                    waterHeights1[i] = LuxWaterUtils.GetGestnerDisplacement(localToWorldMatrices[i].MultiplyPoint3x4(p1s[i]), Description, timeOffset).y + waterHeightOffset;
                    waterHeights2[i] = LuxWaterUtils.GetGestnerDisplacement(localToWorldMatrices[i].MultiplyPoint3x4(p2s[i]), Description, timeOffset).y + waterHeightOffset;
                }
            }
        }
        
        public override float GetWaterHeight(Vector3 worldPoint)
        {
            return LuxWaterUtils.GetGestnerDisplacement(worldPoint, Description, 0).y + waterHeightOffset;
        }
#endif
    }
}

