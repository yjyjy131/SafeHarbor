using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2.DemoContent
{
    /// <summary>
    /// An example on how to add WaterObject to an existing object at runtime.
    /// </summary>
    public class WaterObjectFromScript : MonoBehaviour
    {
        void Start()
        {
            WaterObject waterObject = gameObject.AddComponent<WaterObject>();
            waterObject.SetMaterialDensity(400);
            waterObject.convexifyMesh = true;
            waterObject.simplifyMesh = true;
            waterObject.targetTris = 64;
            waterObject.GenerateSimMesh();
            waterObject.Init();

            // Important. Without running Synchronize() WaterObject will not be registered by the WaterObjectManager and 
            // the physics will not work. Just note that running Synchronize() can be 
            WaterObjectManager.Instance.Synchronize();
        }
    }
}

