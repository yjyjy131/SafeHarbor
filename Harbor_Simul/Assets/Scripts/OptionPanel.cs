using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionPanel : SelectPanel
{
    public void onResumeSelected()
    {
        deActive();
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
