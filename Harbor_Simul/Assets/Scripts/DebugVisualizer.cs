using System.Collections;
using TMPro;
using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    string myLog;
    public TextMeshProUGUI text;
    Queue myLogQueue = new Queue();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
       // if (type != LogType.Error) return;
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        while (myLogQueue.Count > 15)
        {
            myLogQueue.Dequeue();
        }
        myLog = string.Empty;
        foreach (string mylog in myLogQueue)
        {
            myLog += mylog;
        }
    }

    void OnGUI()
    {
        text.text = myLog;
        //GUILayout.Label(myLog);
    }
}