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
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        shipControl = gameObject.GetComponent<AdvancedShipController>();
        rigid = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
