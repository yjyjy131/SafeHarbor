using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class SerialManager : Singleton<SerialManager>
{
    public enum PortNumber
    {
        COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10,
        COM11, COM12, COM13, COM14, COM15, COM16
    }
    private SerialPort serial;
    [SerializeField]
    private PortNumber portNumber = PortNumber.COM1;
    [SerializeField]
    private int baudRate = 9600;
    private string output;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        if (SerialManager.instance != null && SerialManager.instance != this)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);
        serial = new SerialPort(portNumber.ToString(), baudRate);
    }

    // Update is called once per frame
    void Update()
    {
        if (serial.IsOpen)
        {
            try
            {
                output = serial.ReadLine();
                if (output.Length > 0)
                {
                    Debug.Log(output);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }
    }
}
