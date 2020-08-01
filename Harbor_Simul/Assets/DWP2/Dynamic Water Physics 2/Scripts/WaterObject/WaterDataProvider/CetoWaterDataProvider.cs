using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if DWP_CETO
using Ceto;
#endif

namespace DWP2
{
    public class CetoWaterDataProvider : WaterDataProvider
    {
#if DWP_CETO
        private static Ocean ocean;

        public override void Initialize()
        {
            base.Initialize();

            ocean = waterObject.GetComponent<Ocean>();
            if (ocean == null)
            {
                Debug.LogError(
                    "A gameobject tagged 'Water' has been found but it does not contain Ocean component. " +
                    "You have defined DWP_CETO and therefore that component is required. ");
            }
        }


        public override float GetWaterHeight(Vector3 position)
        {
            return ocean.QueryWaves(position.x, position.z);
        }
#endif
    }
}
