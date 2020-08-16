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
    void LateUpdate()
    {
        transform.position = CamManager.Instance.mainCam.position;
        transform.rotation = CamManager.Instance.mainCam.rotation;
    }

    public void setTransform(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}
