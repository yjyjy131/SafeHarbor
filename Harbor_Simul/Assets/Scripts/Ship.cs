using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MovableEntity
{
    private ShipControl shipControl;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        shipControl = gameObject.GetComponent<ShipControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void stopControl()
    {
        shipControl.stopControl();
    }

    public void startControl()
    {
        shipControl.startControl();
    }
}
