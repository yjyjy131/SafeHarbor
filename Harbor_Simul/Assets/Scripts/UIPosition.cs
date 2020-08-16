using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPosition : MonoBehaviour
{
    RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        rect.position = CamManager.Instance.uiPos.position;
        rect.rotation = CamManager.Instance.uiPos.rotation;
    }

    public void setTransform(Vector3 pos, Quaternion rot)
    {
        rect.position = pos;
        rect.rotation = rot;
    }
}
