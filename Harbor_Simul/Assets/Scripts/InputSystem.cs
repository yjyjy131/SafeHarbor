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

    protected override void Awake()
    {
        base.Awake();
        if (InputSystem.instance != null && InputSystem.instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {

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
            getControllerSpeedValue();
        }
        speed = Mathf.Clamp(speed, -1, 1);

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
            getControllerAngleValue();
        }
        angle = Mathf.Clamp(angle, -1, 1);

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
        return 0;
    }

    private float getControllerAngleValue()
    {
        return 0;
    }
}
