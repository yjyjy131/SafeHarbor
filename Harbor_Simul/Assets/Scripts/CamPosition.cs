using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPosition : MonoBehaviour
{
    public Transform mainCam;
    public Transform monitorCam;
    private Vector3 preAngle;

    // Update is called once per frame
    void LateUpdate()
    {
        mainCam.position = CamManager.Instance.mainCam.position;
        mainCam.rotation = CamManager.Instance.mainCam.rotation;
        if(CamManager.Instance.playerPos != null)
        {
            Vector3 lerped = Vector3.Lerp(monitorCam.position, CamManager.Instance.playerPos.position, Time.deltaTime *  3f);
            monitorCam.position = new Vector3(lerped.x, monitorCam.position.y, lerped.z);
            //monitorCam.position = CamManager.Instance.monitorCam.position;
            //preAngle = monitorCam.rotation.eulerAngles;
            //monitorCam.rotation = CamManager.Instance.monitorCamRot;
            //monitorCam.rotation =  Quaternion.Euler(preAngle.x, Mathf.Lerp(preAngle.y, CamManager.Instance.monitorCam.rotation.eulerAngles.y, Time.deltaTime * 2f), preAngle.z);
        }
        else
        {
            monitorCam.position = CamManager.Instance.mainCam.position;
            monitorCam.rotation = CamManager.Instance.mainCam.rotation;
        }
    }

    public void initMonitorCam(Transform trans)
    {
        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }
}
