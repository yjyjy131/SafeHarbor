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
        RectTransform[] childList = content.GetComponentsInChildren<RectTransform>();
        if (childList != null)
        {
            for (int i = 0; i < childList.Length; i++)
            {
                if (childList[i].gameObject != content.gameObject)
                    Destroy(childList[i].gameObject);
            }
        }
        buttons.RemoveRange(1, buttons.Count-1);
        currentButton = 0;
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
        entity.GetComponent<LogEntity>().setText(log, this);
        buttons.Add(entity.GetComponent<MyButton>());
    }

    public void back()
    {
        deActive();
        MainMenu.Instance.active();
    }
}
