using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogCtrPanel : SelectPanel
{
    public GameObject resume;
    public GameObject stop;
    public TextMeshProUGUI time;
    public TextMeshProUGUI timeScale;
    public Slider playBar;

    public void Update()
    {
        if(LogManager.Instance.isFoward)
            timeScale.text = LogManager.Instance.getTimeScale().ToString();
        else
            timeScale.text = "-" + LogManager.Instance.getTimeScale().ToString();
        time.text = (LogManager.Instance.playedTime - (LogManager.Instance.playedTime % Logger.interval)).ToString();
        playBar.value = LogManager.Instance.playedTime / LogManager.Instance.endTime;
    }

    public void OnBack()
    {
        SceneManage.Instance.loadScene("MainScene");
    }

    public void OnFast()
    {
        if (!LogManager.Instance.isFoward)
        {
            LogManager.Instance.setTimeScale(0);
            LogManager.Instance.isFoward = true;
            LogManager.Instance.stopReplay();
            LogManager.Instance.resumeReplay();
            return;
        }
        LogManager.Instance.setTimeScale(2f);
        LogManager.Instance.stopReplay();
        LogManager.Instance.resumeReplay();
    }

    public void OnSlow()
    {
        if (LogManager.Instance.isFoward)
        {
            LogManager.Instance.setTimeScale(0f);
            LogManager.Instance.isFoward = false;
            LogManager.Instance.stopReplay();
            LogManager.Instance.resumeReplay();
            return;
        }
        LogManager.Instance.setTimeScale(2f);
        LogManager.Instance.stopReplay();
        LogManager.Instance.resumeReplay();
    }

    public void OnStop()
    {
        LogManager.Instance.stopReplay();
    }

    public void OnResume()
    {
        LogManager.Instance.resumeReplay();
    }

    public void OnPlayBarChanged()
    {/*
        float playedtime =  LogManager.Instance.endTime * playBar.value;
        LogManager.Instance.changePlayedTime(playedtime);
        */
    }

    public void setTime(string str)
    {
        time.text = str;
    }
}
