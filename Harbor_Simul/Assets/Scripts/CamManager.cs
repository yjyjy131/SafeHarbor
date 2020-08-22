using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : Singleton<CamManager>
{
    public Transform mainCam;
    public RectTransform uiPos;
    public Transform playerPos;
    public Quaternion monitorCamRot;


}
