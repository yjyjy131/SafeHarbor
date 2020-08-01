using System;
using UnityEngine;

namespace DWP2
{
    [RequireComponent(typeof(Rigidbody))]
    [ExecuteInEditMode]
    public class RigidbodyMassFromChildren : MonoBehaviour {

        private WaterObject[] waterObjects;
        private Rigidbody rb;
        
        public void Calculate()
        {
            waterObjects = GetComponentsInChildren<WaterObject>();

            if (waterObjects.Length > 0)
            {
                rb = GetComponent<Rigidbody>();
                float massSum = 0;

                foreach (WaterObject wo in waterObjects)
                {
                    massSum += wo.Mass;
                }

                if(massSum > 0.001f)
                {
                    rb.mass = massSum;
                }
            }
        }

    }
}
