using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : MonoBehaviour, IPointerEnterHandler
{
    SelectPanel parent;
    Button btn;

    public void Awake()
    {
        parent = transform.parent.GetComponent<SelectPanel>();
        btn = transform.GetComponent<UnityEngine.UI.Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parent.OnMouseEnter(btn);
    }

}
