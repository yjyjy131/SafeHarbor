using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationFlag : MonoBehaviour
{
    public Vector3 pos { get { return transform.position; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            GameManager.Instance.OnArrived();
        }
    }

    public void active()
    {
        gameObject.SetActive(true);
    }

    public void deActive()
    {
        gameObject.SetActive(false);
    }
}
