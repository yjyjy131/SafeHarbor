using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSystem : Singleton<PanelSystem>
{
    [SerializeField]
    private List<SelectPanel> selectPanels;
    private Vector3 currentButtonPos = Vector3.zero;
    private int currentPanel = 0;
    private bool isActive = false;
    private bool preVitualRight = false;
    private bool preVitualLeft = false;
    protected override void Awake()
    {
        base.Awake();
        if (PanelSystem.instance != null && PanelSystem.instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void active()
    {
        selectPanels[currentPanel].active();
    }
    public void deActive()
    {
        selectPanels[currentPanel].deActive();
    }

    public void Update()
    {
        if (InputSystem.Instance.virtualRight == true && preVitualRight == false)
        {
            preVitualRight = true;
            nextButton();
        }
        if (InputSystem.Instance.virtualRight == false && preVitualRight == true)
        {
            preVitualRight = false;
        }
        if (InputSystem.Instance.virtualLeft == true && preVitualLeft == false)
        {
            preVitualLeft = true;
            backButton();
        }
        if (InputSystem.Instance.virtualLeft == false && preVitualLeft == true)
        {
            preVitualLeft = false;
        }
    }

    public void backButton()
    {
        selectPanels[currentPanel].backButton();
        currentButtonPos = selectPanels[currentPanel].getCurrentButtonPos();
    }
    public void nextButton()
    {
        selectPanels[currentPanel].nextButton();
        currentButtonPos = selectPanels[currentPanel].getCurrentButtonPos();
    }

    public void backPanel()
    {
        selectPanels[currentPanel--].deActive();
        selectPanels[currentPanel].active();
    }
    public void nextPanel()
    {
        selectPanels[currentPanel++].deActive();
        selectPanels[currentPanel].active();
    }
}
