using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableEntity : MonoBehaviour
{
    int id;
    protected virtual void Awake()
    {
        id = ++GlobalData.entityNum;
    }

    public TransFormData saveTransform()
    {
        return new TransFormData(transform.position, new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), id);
    }

    public void loadTransform(TransFormData data)
    {
        transform.position = data.pos;
        Quaternion qt = new Quaternion();
        qt.x = data.rotation.x;
        qt.y = data.rotation.y;
        qt.z = data.rotation.z;
        qt.w = data.rotation.w;
        transform.rotation = qt;
    }

    public virtual void destroy()
    {
        GameObject.Destroy(this.gameObject);
    }
}
