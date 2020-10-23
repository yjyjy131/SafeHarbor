using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationFlag : MonoBehaviour
{
    public Vector3 pos { get { return transform.position; } }

    private void Awake()
    {
        //gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("arrived");
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnArrived();
            gameObject.SetActive(false);
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
