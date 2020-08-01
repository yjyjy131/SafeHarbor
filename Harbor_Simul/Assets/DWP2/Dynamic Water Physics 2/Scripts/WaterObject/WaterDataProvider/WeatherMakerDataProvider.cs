using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWP2
{
    public class WeatherMakerDataProvider : WaterDataProvider
    {
#if DWP_WEATHER_MAKER
        public Material WaterMaterial;

        private LuxWaterUtils.GersterWavesDescription Description;

        public override void Initialize()
        {
            base.Initialize();

            WaterMaterial = waterObject.GetComponent<MeshRenderer>()?.material;
            if (WaterMaterial == null)
            {
                Debug.LogError("Lux water object does not contain a mesh renderer or material.");
            }
            LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
        }

        public override void GetWaterHeights(ref Vector3[] p0s, ref Vector3[] p1s, ref Vector3[] p2s,
            ref float[] waterHeights0, ref float[] waterHeights1, ref float[] waterHeights2, ref List<TriangleData> triangleData)
        {
            Debug.Assert(p0s.Length == waterHeights0.Length, "Points and WaterHeights arrays must have same length.");

            int n = p0s.Length;

            // Fill in with 0s if no water object set
            if (waterObject == null)
            {
                for (int i = 0; i < n; i++)
                {
                    waterHeights0[i] = 0;
                    waterHeights1[i] = 0;
                    waterHeights2[i] = 0;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    Vector3 position = p0s[i];
                    position.y = 0;
                    waterHeights0[i] = LuxWaterUtils.GetGestnerDisplacement(position, Description, 0).y;

                    position = p1s[i];
                    position.y = 0;
                    waterHeights1[i] = LuxWaterUtils.GetGestnerDisplacement(position, Description, 0).y;

                    position = p2s[i];
                    position.y = 0;
                    waterHeights2[i] = LuxWaterUtils.GetGestnerDisplacement(position, Description, 0).y;
                }
            }
        }
#endif
    }
}

