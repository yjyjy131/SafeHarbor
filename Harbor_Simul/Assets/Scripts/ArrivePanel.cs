using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrivePanel : MonoBehaviour
{
    public Text time;

    public void active(float etime)
    {
        TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(etime)));
        time.text = string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
        gameObject.SetActive(true);
    }

    public void deActive()
    {
        gameObject.SetActive(false);
    }

    public void onRestartSelected()
    {
        SceneManage.Instance.loadScene("MyScene");
    }

    public void onMainMenuSelected()
    {
        SceneManage.Instance.loadScene("MainScene");
    }
}
