using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DWP_SUIMONO
using Suimono.Core;
#endif

namespace DWP2
{
    public class SuimonoWaterDataProvider : WaterDataProvider
    {
#if DWP_SUIMONO
        private SuimonoModule suimono;

        public override void Initialize()
        {
            base.Initialize();

            suimono = waterObject.GetComponent<SuimonoModule>();
            if(suimono == null)
            {
                Debug.LogError(
                    "A gameobject tagged 'Water' has been found but it does not contain SuimonoMOdule component. " +
                    "You have defined DWP_SUIMONO and therefore that component is required. ");
            }
        }

        public override float GetWaterHeight(Vector3 position)
        {
            return suimono.SuimonoGetHeight(position, "height");
        }
#endif
    }
}

