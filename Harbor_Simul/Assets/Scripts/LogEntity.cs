using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogEntity : MyButton
{
    [SerializeField]
    private TextMeshProUGUI startedAt;
    [SerializeField]
    private TextMeshProUGUI ship;
    [SerializeField]
    private TextMeshProUGUI route;
    private LogData log;

    public void setText(LogData _log, SelectPanel _parent)
    {
        log = _log;
        startedAt.text = log.startedAt.ToString();
        ship.text = log.shipType.ToString();
        route.text = log.route.ToString();
        parent = _parent;
    }

    public void OnSelected()
    {
        Logger.setCurLogData(log);
        SceneManage.Instance.loadScene("LogScene");
    }

}
