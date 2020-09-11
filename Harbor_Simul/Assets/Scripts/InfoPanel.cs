using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : SelectPanel
{
    public TextMeshProUGUI speed;
    public TextMeshProUGUI angle;
    public TextMeshProUGUI time;

    // Update is called once per frame
    public override void Update()
    {
        float _angle;
        float knot;
        knot = GameManager.Instance.player.speed / 1.9438f;
        speed.text = string.Format("{0:00.0}노트", knot);
        if (GameManager.Instance.player.angle > 180)
            _angle = -(360 - GameManager.Instance.player.angle);
        else
            _angle = GameManager.Instance.player.angle;
        angle.text = string.Format("{0:00.0}도", _angle);
        TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(GameManager.Instance.elapsedTime)));
        time.text = string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
    }
}
