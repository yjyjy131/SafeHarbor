using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : Singleton<InputSystem>
{
    public float angle { get; private set; }
    public float speed { get; private set; }
    public bool select { get { return Input.GetKeyDown(KeyCode.Mouse0); } }
    public bool back { get { return Input.GetKeyDown(KeyCode.Mouse1); } }
    public bool virtualRight { get; private set; }
    public bool virtualLeft { get; private set; }

    private const float threshold = 100f;

    private void Update()
    {
        speed = getControllerSpeedValue();
        angle = getControllerAngleValue();

        if (Input.GetKey(KeyCode.W))
        {
            speed += 1f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            speed -= 1f * Time.deltaTime;
        }
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            speed = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            angle -= 1f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            angle += 1f * Time.deltaTime;
        }
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            angle = 0;
        }

        if (Math.Abs(getControllerAngleValue()) > threshold)
        {
            if (getControllerAngleValue() > 0)
                virtualRight = true;
            else
                virtualLeft = true;
        }
        else
        {
            virtualLeft = false;
            virtualRight = false;
        }
    }

    private float getControllerSpeedValue()
    {
        return speed;
    }

    private float getControllerAngleValue()
    {
        return angle;
    }
}
