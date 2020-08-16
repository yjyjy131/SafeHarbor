using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class TransFormData
{
    public Vector3 pos;
    public Vector4 rotation;
    public int id;
    public TransFormData(Vector3 _pos, Vector4 _rotation, int _id)
    {
        pos = _pos;
        rotation = _rotation;
        id = _id;
    }

    public string toString()
    {
        return id + " " + pos.x + " " + pos.y + " " + pos.z + " " + rotation.x + " " + rotation.y + " " + rotation.z + " " + rotation.w;
    }
}
