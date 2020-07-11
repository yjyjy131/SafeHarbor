using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    [SerializeField]
    private List<Button> buttons;
    private int currentButton;

    public void active()
    {
        gameObject.SetActive(true);
    }

    public void deActive()
    {
        gameObject.SetActive(false);
    }

    public void moveLeft()
    {
        currentButton--;
        if (currentButton < 0)
            currentButton = buttons.Count + 1;
    }
    public void moveRight()
    {
        currentButton = (currentButton + 1) % buttons.Count;
    }

    public Vector3 getCurrentButtonPos()
    {
        return buttons[currentButton].transform.position;
    }
}
