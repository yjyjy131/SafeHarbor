using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using UnityEngine.UI;

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
    [SerializeField]
    private int readTimeout = 2;
    [SerializeField]
    private string _output;
    [SerializeField]
    private float _angle;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private bool _select;
    [SerializeField]
    private bool _back;
    public string output { get; private set; }
    public float angle { get; set; }
    public float speed { get; private set; }
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
        serial.ReadTimeout = readTimeout;
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

    public void Reset()
    {
        StopCoroutine("read");
        serial.Close();
        serial = new SerialPort(portNumber.ToString()
                              , baudRate
                              , Parity.None
                              , 8
                              , StopBits.One);
        serial.Open();
        serial.ReadTimeout = readTimeout;
        output = "";
        StartCoroutine("read");
        select = false;
        back = false;
        speed = 0;
        angle = 0;
        Debug.Log("시리얼 리셋됨");
    }
    public Text text;
    IEnumerator read()
    {
        float parseOutput;
        while (true)
        {
            if (serial.IsOpen)
            {
                try
                {
                    string line = serial.ReadLine();
                    //string line = text.text;
                    Debug.Log(line);
                    if (line != null && line.Length > 0)
                    {
                        output = line;
                        string[] token = output.Split(' ');
                        if (token.Length >= 4)
                        {
                            if (float.TryParse(token[0], out parseOutput)) angle = parseOutput;
                            else
                                throw new Exception("err");
                            if (float.TryParse(token[0], out parseOutput)) speed = parseOutput;
                            else
                                throw new Exception("err");
                            select = token[2].Equals("0") ? false : true;
                            back = token[3].Equals("0") ? false : true;
                        }
                        else
                            throw new Exception("err");
                    }
                    else
                        throw new Exception("err");
                }
                catch (TimeoutException e)
                {
                    //Debug.LogError(e.ToString());
                }
                catch(Exception e)
                {
                    if (e.Message.Equals("err")) Debug.Log("시리얼 형식오류, 입력을 무시합니다");
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
