using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[Serializable]
public class LogData
{
    public List<TransformDataSet> transformDataSet;
    public System.DateTime startedAt;
    public ShipType shipType = ShipType.Container;
    public Route route = Route.RouteA;
    public int entityNum = 0;

    public LogData(System.DateTime _startedAt, ShipType _ship, Route _route)
    {
        transformDataSet = new List<TransformDataSet>();
        startedAt = _startedAt;
        shipType = _ship;
        route = _route;
    }
}

public static class Logger
{
    [SerializeField]
    private static List<LogData> logDatas;
    private static LogData curLogData;
    private static string path = Application.dataPath;
    private static string filename = "log.ini";
    public const float interval = 0.05f;

    public static List<LogData> getLogDatas()
    {
        if(logDatas == null)
        {
            loadAndApply();
        }
        return logDatas;
    }

    public static void setCurLogData(LogData log)
    {
        curLogData = log;
    }

    public static void addData(TransformDataSet data)
    {
        curLogData.transformDataSet.Add(data);
    }

    public static void initLogData(System.DateTime startedAt, ShipType ship, Route route)
    {
        if (logDatas == null)
        {
            loadAndApply();
            if (logDatas == null)
                logDatas = new List<LogData>();
        }
        LogData log = new LogData(startedAt, ship, route);
        logDatas.Add(log);
        curLogData = log;
    }

    public static LogData getCurLogData()
    {
        if(curLogData == null)
        {
            getLogDatas();
            curLogData = logDatas[0];
        }
        return curLogData;
    }

    public static string toString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(curLogData.route.ToString() + " " + curLogData.shipType.ToString() + " " + curLogData.startedAt.ToString());
        foreach (TransformDataSet data in curLogData.transformDataSet)
        {
            builder.AppendLine(data.toString());
        }
        return builder.ToString();
    }

    public static void saveToFile()
    {
        using (FileStream fs = new FileStream(path + "/" + filename, FileMode.OpenOrCreate, FileAccess.Write))
        using (BufferedStream bs = new BufferedStream(fs))
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logDatas));
            bs.Write(data, 0, data.Length);
        }
    }

    public static List<LogData> loadFromFile()
    {
        try
        {
            FileStream fs = new FileStream(path + "/" + filename, FileMode.Open, FileAccess.Read);
            BufferedStream bs = new BufferedStream(fs);
            byte[] data = new byte[fs.Length];
            bs.Read(data, 0, data.Length);
            bs.Close();
            if (data.Length == 0) return null;
            string jsonData = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<List<LogData>>(jsonData); ;
        }catch(FileNotFoundException e)
        {
            Debug.Log("로그파일 없음");
        }
        return null;
    }

    public static void loadAndApply()
    {
        logDatas = loadFromFile();
        Debug.Log("로그파일 불러옴");
    }

}
