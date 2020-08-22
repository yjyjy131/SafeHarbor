using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWP2.ShipController;
public class Ship : MovableEntity
{
    private  AdvancedShipController shipControl;
    private Rigidbody rigid;
    [SerializeField]
    public Transform camPos;
    [SerializeField]
    public RectTransform uiPos;
    [SerializeField]
    public Transform MonitorCamPos;
    public float speed { get; private set; }
    public float angle { get; private set; }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        shipControl = gameObject.GetComponent<AdvancedShipController>();
        rigid = gameObject.GetComponent<Rigidbody>();
        speed = rigid.velocity.magnitude;
        angle = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        speed = rigid.velocity.magnitude;
        angle = transform.rotation.eulerAngles.y;
    }

    public void stopControl()
    {
        rigid.isKinematic = true;
        shipControl.Deactivate();
    }

    public void startControl()
    {
        rigid.isKinematic = false;
        shipControl.Activate();
    }

}
