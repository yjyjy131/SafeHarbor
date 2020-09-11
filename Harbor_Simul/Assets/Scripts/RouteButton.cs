using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RouteButton : MyButton
{
    [SerializeField]
    GameObject info;
    public override void OnPointerEnter(PointerEventData eventData)
    {
        parent.OnMouseEnter(btn);
    }

    public override void highlight()
    {
        info.SetActive(true);
        base.highlight();
    }

    public override void deHighlight()
    {
        info.SetActive(false);
        base.deHighlight();
    }
}
