using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    protected SelectPanel parent;
    protected MyButton btn;
    public Color highlightColor;
    protected Color baseColor;
    protected bool isColorSetted = false;
    public Image image;

    public void Awake()
    {
        parent = transform.parent.GetComponent<SelectPanel>();
        btn = transform.GetComponent<MyButton>();
        if (!isColorSetted)
        {
            baseColor = image.color;
            isColorSetted = true;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        parent.OnMouseEnter(btn);
    }

    public virtual void highlight()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, highlightColor.a);
    }

    public void setBaseColor()
    {
        baseColor = image.color;
        isColorSetted = true;
    }

    public virtual void deHighlight()
    {
        image.color = baseColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        deHighlight();
    }
}
