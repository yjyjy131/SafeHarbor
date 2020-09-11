using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType
{
    Container,
    SmallShip
}
public enum Route
{
    RouteA,
    RouteB,
    RouteC,
    RouteD
}


public class MainMenu : Singleton<MainMenu>
{
    [SerializeField]
    private SelectPanel currentPanel;
    [SerializeField]
    private LogListPanel logPanel;
    [SerializeField]
    public SelectPanel shipPanel;
    [SerializeField]
    public SelectPanel mainPanel;
    [SerializeField]
    private List<SelectPanel> routePanels;
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
        changePanel(shipPanel);
    }

    public void onReplaySelected()
    {
        changePanel(logPanel);
    }


    public void onShipSelected(int shipType)
    {
        GlobalData.shipType = (ShipType) shipType;
        changePanel(routePanels[shipType]);
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
        currentPanel.backButton();
        currentButtonPos = currentPanel.getCurrentButtonPos();
    }
    public void nextButton()
    {
        currentPanel.nextButton();
        currentPanel.getCurrentButtonPos();
    }
    public void backPanel()
    {
        currentPanel.deActive();
        currentPanel = currentPanel.prePanel;
        currentPanel.active();
    }

    public void reset()
    {
        currentPanel.deActive();
        currentPanel = mainPanel;
        currentButtonPos = Vector3.zero;
        currentPanel.active();
    }

    public void active()
    {
        currentPanel.active();

    }

    public void deActive()
    {
        currentPanel.deActive();
    }

    public void changePanel(SelectPanel panel)
    {
        panel.prePanel = currentPanel;
        currentPanel.deActive();
        currentPanel = panel;
        currentPanel.active();
    }
}
