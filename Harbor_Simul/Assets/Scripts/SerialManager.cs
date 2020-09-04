﻿using System.Collections;
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
    [SerializeField]
    private float readRate = 0.5f;
    public string output { get; private set; }
    public int angle { get; private set; }
    public int speed { get; private set; }
    public bool select { get; private set; }
    public bool back { get; private set; }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        if (SerialManager.instance != null && SerialManager.instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        serial = new SerialPort(portNumber.ToString()
                              , baudRate
                              , Parity.None
                              , 8
                              , StopBits.One);
        serial.Open();
        serial.ReadTimeout = 2;
        output = "";
        StartCoroutine("read");
        select = false;
        back = false;
        speed = 0;
        angle = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {/*
        //output = "";
        if (serial.IsOpen)
        {
            try
            {
                string line = serial.ReadLine();
                Debug.Log(line);
                if (line != null && line.Length > 0)
                {
                    output = line;
                }
            }
            catch(TimeoutException e)
            {
                //Debug.LogError(e.ToString());
            }

        }*/
    }

    IEnumerator read()
    {
        while (true)
        {
            if (serial.IsOpen)
            {
                try
                {
                    string line = serial.ReadLine();
                    Debug.Log(line);
                    if (line != null && line.Length > 0)
                    {
                        output = line;
                        string[] token = output.Split(' ');
                        if (token.Length >= 4)
                        {
                            angle = int.Parse(token[0]);
                            speed = int.Parse(token[1]);
                            select = bool.Parse(token[2]);
                            back = bool.Parse(token[3]);
                        }
                    }
                }
                catch (TimeoutException e)
                {
                    //Debug.LogError(e.ToString());
                }

            }
            yield return new WaitForSeconds(readRate);
        }
    }

    void OnApplicationQuit()
    {
        StopCoroutine("read");
        serial.Close();
    }
}