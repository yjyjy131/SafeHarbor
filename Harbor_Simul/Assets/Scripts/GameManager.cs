using DWP2.ShipController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private float time = 0;
    private Ship player;
    private List<MovableEntity> entities = new List<MovableEntity>();
    [SerializeField]
    private ArrivePanel arrivePanel;
    private List<MyRoute> routes = new List<MyRoute>();
    private MyRoute currentRoute;
    private bool isStart = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        setInstance(this);
        foreach(MyRoute child in GameObject.Find("Routes").transform.GetComponentsInChildren<MyRoute>())
        {
            routes.Add(child);
        }
        foreach (MovableEntity child in GameObject.Find("Entities").transform.GetComponentsInChildren<MovableEntity>())
        {
            entities.Add(child);
        }
    }

    public void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentRoute = routes[(int)GlobalData.route];
        currentRoute.destination.active();
        player = Instantiate(Resources.Load<GameObject>("Prefabs/Ship"+(int)GlobalData.shipType), currentRoute.startPoint.pos, Quaternion.identity).GetComponent<Ship>();
        player.startControl();
        CamManager.Instance.mainCam = player.camPos;
        CamManager.Instance.uiPos = player.uiPos;
        DWP2.WaterObjectManager.Instance.Synchronize();
        isStart = true;
        StartCoroutine("logCoroutine");
    }

    public void Update()
    {
    }

    public void OnArrived()
    {
        player.stopControl();
        stopLogging();
        arrivePanel.active();
    }

    public void SetPlayer(Ship ship)
    {
        player = ship;
    }

    public void doLogging()
    {

    }

    public void stopLogging()
    {
        StopCoroutine("logCoroutine");
    }

    public void Reset()
    {
        foreach(MovableEntity entity in entities)
        {
            entity.destroy();
        }
        currentRoute.destination.deActive();
    }

    public IEnumerator logCoroutine()
    {
        float cooldown = 1f;
        while (true)
        {
            doLogging();
            yield return new WaitForSeconds(cooldown);
        }
    }
}
