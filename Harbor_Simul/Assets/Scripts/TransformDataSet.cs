using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class TransformDataSet
{
    public List<TransFormData> transFormDatas;
    public float elapsedTime;

    public TransformDataSet(float time)
    {
        elapsedTime = time;
        transFormDatas = new List<TransFormData>();
    }

    public string toString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(elapsedTime + " ");
        foreach (TransFormData data in transFormDatas)
        {
            builder.AppendLine(data.toString());
        }
        return builder.ToString();
    }
}
