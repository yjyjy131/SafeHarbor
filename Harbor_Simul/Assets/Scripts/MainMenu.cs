using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType
{
    Container
}
public enum Route
{
    RouteA
}


public class MainMenu : Singleton<MainMenu>
{
    [SerializeField]
    private List<SelectPanel> selectPanels;
    [SerializeField]
    private LogListPanel logPanel;
    private int currentPanel = 0;
    private Vector3 currentButtonPos = Vector3.zero;
    private bool buttonActive = true;
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
    }

    public void onGameStartSelected()
    {
        selectPanels[currentPanel++].deActive();
        selectPanels[currentPanel].active();
    }

    public void onReplaySelected()
    {
        deActive();
        logPanel.active();
    }


    public void onShipSelected(int shipType)
    {
        GlobalData.shipType = (ShipType) shipType;
        nextPanel();
    }

    public void onRouteSelected(int route)
    {
        if (!buttonActive) return;
        GlobalData.route =(Route) route;
        SceneManage.Instance.loadScene("MyScene");
        buttonActive = false;
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
        selectPanels[currentPanel].deActive();
        currentPanel--;
        if (currentPanel < 0) currentPanel = 0;
        selectPanels[currentPanel].active();
    }
    public void nextPanel()
    {
        selectPanels[currentPanel].deActive();
        currentPanel = (currentPanel + 1) % selectPanels.Count;
        selectPanels[currentPanel].active();
    }

    public void reset()
    {
        selectPanels[currentPanel].deActive();
        currentPanel = 0;
        currentButtonPos = Vector3.zero;
        selectPanels[currentPanel].active();
        active();
    }

    public void active()
    {
        selectPanels[currentPanel].active();

    }

    public void deActive()
    {
        selectPanels[currentPanel].deActive();
    }
}
