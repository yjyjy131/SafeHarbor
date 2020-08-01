using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPosition : Singleton<CamPosition>
{
    protected override void Awake()
    {
        base.Awake();
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = CamManager.Instance.mainCam.position;
        transform.rotation = CamManager.Instance.mainCam.rotation;
    }
}
