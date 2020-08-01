using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrivePanel : MonoBehaviour
{

    public void active()
    {
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
