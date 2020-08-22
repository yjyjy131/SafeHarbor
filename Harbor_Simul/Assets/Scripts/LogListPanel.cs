using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class LogListPanel : SelectPanel
{
    [SerializeField]
    private GameObject content;
    private List<LogData> logDatas;

    public override void active()
    {
        base.active();
        loadLogs();
    }

    public override void deActive()
    {
        base.deActive();
        Transform[] childList = content.GetComponentsInChildren<Transform>(true);
        if (childList != null)
        {
            for (int i = 0; i < childList.Length; i++)
            {
                if (childList[i] != content)
                    Destroy(childList[i].gameObject);
            }
        }
    }

    public void loadLogs()
    {
        logDatas = Logger.getLogDatas();
        if (logDatas == null)
            return;
        foreach(LogData log in logDatas)
        {
            makeLogEntity(log);
        }
        if (buttons.Count >= 2)
        {
            nextButton();
        }
    }

    public void makeLogEntity(LogData log)
    {
        GameObject entity = Instantiate(Resources.Load<GameObject>("Prefabs/LogEntity"), content.transform).gameObject;
        entity.GetComponent<LogEntity>().setText(log);
        buttons.Add(entity.GetComponent<UnityEngine.UI.Button>());
    }

    public void back()
    {
        deActive();
        MainMenu.Instance.active();
    }
}
