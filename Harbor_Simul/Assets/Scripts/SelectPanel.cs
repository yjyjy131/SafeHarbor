using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    [SerializeField]
    protected List<MyButton> buttons;
    protected int currentButton = 0;
    protected bool isActive = false;
    protected bool preVitualRight = false;
    protected bool preVitualLeft = false;
    protected SelectPanel _prePanel;
    public SelectPanel prePanel{ get; set;}

    public virtual void OnEnable()
    {
        if (buttons.Count > 0)
        {
            buttons[currentButton].setBaseColor();
            buttons[currentButton].highlight();
        }
    }

    public virtual void OnDisable()
    {
        if(buttons.Count > 0)
            buttons[currentButton].deHighlight();
    }

    public virtual void active()
    {
        gameObject.SetActive(true);
    }

    public virtual void deActive()
    {
        gameObject.SetActive(false);
    }

    public virtual void Update()
    {
        if (!gameObject.activeSelf) return;

        if (InputSystem.Instance.virtualRight == true && preVitualRight == false)
        {
            preVitualRight = true;
            nextButton();
        }
        if (InputSystem.Instance.virtualRight == false)
        {
            preVitualRight = false;
        }
        if (InputSystem.Instance.virtualLeft == true && preVitualLeft == false)
        {
            preVitualLeft = true;
            backButton();
        }
        if (InputSystem.Instance.virtualLeft == false)
        {
            preVitualLeft = false;
        }
        if (InputSystem.Instance.select == true)
        {
            OnClick();
        }
    }

    public void backButton()
    {
        buttons[currentButton].deHighlight();
        currentButton--;
        if (currentButton < 0)
            currentButton = buttons.Count - 1;
        buttons[currentButton].highlight();
    }
    public void nextButton()
    {
        buttons[currentButton].deHighlight();
        currentButton = (currentButton + 1) % buttons.Count;
        buttons[currentButton].highlight();
    }

    public void OnClick()
    {
        ExecuteEvents.Execute(buttons[currentButton].gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    public Vector3 getCurrentButtonPos()
    {
        return buttons[currentButton].transform.position;
    }

    public void OnMouseEnter(MyButton btn)
    {
        buttons[currentButton].deHighlight();
        if (!buttons.Contains(btn)) return;
        currentButton = buttons.IndexOf(btn);
        buttons[currentButton].highlight();
    }
}
