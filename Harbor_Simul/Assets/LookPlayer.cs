using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookPlayer : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        try
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            { transform.LookAt(GameManager.Instance.player.transform); }
            else if (LogManager.Instance != null && LogManager.Instance.player != null)
            { transform.LookAt(LogManager.Instance.player.transform); }

        } catch(NullReferenceException e)
        {

        }
    }
}
