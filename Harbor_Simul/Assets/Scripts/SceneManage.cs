using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class SceneManage : Singleton<SceneManage>
{
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        if (SceneManage.instance != null && SceneManage.instance != this)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    public void loadScene(string s)
    {
        StartCoroutine("load", s);
    }

    IEnumerator load(string s)
    {
        SteamVR_Fade.Start(Color.clear, 0f);
        SteamVR_Fade.Start(Color.black, 1f);
        yield return new WaitForSeconds(1.2f);
        AsyncOperation progress =  SceneManager.LoadSceneAsync(s);
        while(progress.progress < 1f)
        {
            yield return null;
        }
        SteamVR_Fade.Start(Color.black, 0f);
        SteamVR_Fade.Start(Color.clear, 1f);
    }
}
