using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipType{
    Container
}
public enum Route
{
    RouteA
}

public enum SelectPhase
{
    Start,
    Ship,
    Route,
    End
}

public class MainMenu : Singleton<MainMenu>
{
    [SerializeField]
    private List<SelectPanel> selectPanels;
    private int currentPanel = 0;
    private Vector3 currentButtonPos = Vector3.zero;
    private SelectPhase currentPhase = SelectPhase.Start;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        currentPhase = SelectPhase.Ship;
    }

    public void onShipSelected(ShipType shipType)
    {
        GlobalData.shipType = shipType;
        selectPanels[currentPanel++].deActive();
        selectPanels[currentPanel].active();
        currentPhase = SelectPhase.Route;
    }

    public void onRouteSelected(Route route)
    {
        GlobalData.route = route;
        selectPanels[currentPanel].deActive();
        currentPanel = 0;
        currentPhase = SelectPhase.End;
        GameManager.Instance.StartGame();
        deActive();
    }

    public void moveLeft()
    {
        selectPanels[currentPanel].moveLeft();
        currentButtonPos = selectPanels[currentPanel].getCurrentButtonPos();
    }
    public void moveRight()
    {
        selectPanels[currentPanel].moveRight();
        currentButtonPos = selectPanels[currentPanel].getCurrentButtonPos();
    }

    public void reset()
    {
        currentPanel = 0;
        currentButtonPos = Vector3.zero;
        currentPhase = SelectPhase.Ship;
        active();
    }

    public void active()
    {
        gameObject.SetActive(true);
    }

    public void deActive()
    {
        gameObject.SetActive(false);
    }
}
