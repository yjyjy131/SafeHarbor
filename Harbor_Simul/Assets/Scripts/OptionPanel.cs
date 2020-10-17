using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionPanel : SelectPanel
{
    public void onResumeSelected()
    {
        Invoke("deActive", 0.1f);
        //deActive();
    }

    public void onRestartSelected()
    {
        GameManager.Instance.stopAndDeleteLog();
        SceneManage.Instance.loadScene("MyScene");
    }

    public void onMainMenuSelected()
    {
        GameManager.Instance.stopAndDeleteLog();
        SceneManage.Instance.loadScene("MainScene");
    }
}
