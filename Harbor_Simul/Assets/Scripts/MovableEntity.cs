using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableEntity : MonoBehaviour
{
    int id;
    protected virtual void Awake()
    {
        id = GlobalData.entityNum++;
    }

    public virtual void destroy()
    {
        GameObject.Destroy(this.gameObject);
    }
}
