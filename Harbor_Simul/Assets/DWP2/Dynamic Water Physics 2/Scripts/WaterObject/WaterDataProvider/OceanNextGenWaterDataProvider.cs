using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWP2
{
    public class OceanNextGenWaterDataProvider : WaterDataProvider
    {
#if DWP_OCEAN_NEXT_GEN
        private static Ocean ocean;

        public override void Initialize()
        {
            base.Initialize();

            ocean = waterObject.GetComponent<Ocean>();
            if (ocean == null)
            {
                Debug.LogError(
                    "A gameobject tagged 'Water' has been found but it does not contain Ocean (Ocean Next Gen) component. " +
                    "You have defined DWP_OCEAN_NEXT_GEN and therefore that component is required. ");
            }
        }


        public override float GetWaterHeight(Vector3 position)
        {
            if (ocean.canCheckBuoyancyNow[0] != 1) return 0;

            float choppyOffset = 0;
            if (ocean.choppy_scale > 0) choppyOffset = ocean.GetChoppyAtLocation2(position.x, position.z);
            return ocean.GetWaterHeightAtLocation2(position.x - choppyOffset, position.z);
        }
#endif
    }
}

