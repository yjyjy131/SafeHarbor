using DWP2.ShipController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Ship player { get; private set; }
    private List<MovableEntity> entities = new List<MovableEntity>();
    [SerializeField]
    private ArrivePanel arrivePanel;
    [SerializeField]
    private InfoPanel infoPanel;
    [SerializeField]
    private OptionPanel optionPanel;
    private List<MyRoute> routes = new List<MyRoute>();
    private MyRoute currentRoute;
    private bool isStart = false;
    public float elapsedTime { get; private set; }
    public static System.DateTime startedAt = System.DateTime.MinValue;
    public GameObject map;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        setInstance(this);
        map = GameObject.Find("Map");
        foreach (MyRoute child in map.transform.Find("Routes").transform.GetComponentsInChildren<MyRoute>())
        {
            routes.Add(child);
        }
        foreach (MovableEntity child in map.transform.GetComponentsInChildren<MovableEntity>())
        {
            entities.Add(child);
        }
        elapsedTime = 0;
    }

    public void Start()
    {
        StartGame();
    }

    public void Update()
    {
        if (InputSystem.instance.select && isStart)
        {
            onOption();
        }
    }

    public void StartGame()
    {
        currentRoute = routes[(int)GlobalData.route];
        currentRoute.startPoint.active();
        currentRoute.destination.active();
        player = Instantiate(Resources.Load<GameObject>("Prefabs/Ship"+(int)GlobalData.shipType), currentRoute.startPoint.pos, currentRoute.startPoint.transform.rotation).GetComponent<Ship>();
        player.transform.SetParent(map.transform.Find("Entities"));
        entities.Add(player);
        player.startControl();
        CamManager.Instance.mainCam = player.camPos;
        CamManager.Instance.uiPos = player.uiPos;
        DWP2.WaterObjectManager.Instance.Synchronize();
        elapsedTime = 0;
        startedAt = System.DateTime.Now;
        isStart = true;
        StartCoroutine("logCoroutine");
    }

    public void FixedUpdate()
    {
        if (isStart)
            elapsedTime += Time.fixedDeltaTime;
    }

    public void OnArrived()
    {
        if (player == null) return;
        player.stopControl();
        stopLogging();
        infoPanel.deActive();
        optionPanel.deActive();
        arrivePanel.active(elapsedTime);
    }

    public void onOption()
    {
        optionPanel.active();
    }

    public void SetPlayer(Ship ship)
    {
        player = ship;
    }

    public void doLogging()
    {
        TransformDataSet dataSet = new TransformDataSet(elapsedTime);
        foreach (MovableEntity entity in entities)
        {
            dataSet.transFormDatas.Add(entity.saveTransform());
        }
        //Debug.Log(dataSet.toString());
        Logger.addData(dataSet);
       // Debug.Log(Logger.toString());
    }

    public void stopLogging()
    {
        StopCoroutine("logCoroutine");
        Logger.saveToFile();
    }

    public void Reset()
    {
        startedAt = System.DateTime.MinValue;
        isStart = false;
        elapsedTime = 0;
        foreach(MovableEntity entity in entities)
        {
            entity.destroy();
        }
        currentRoute.destination.deActive();

    }

    public IEnumerator logCoroutine()
    {
        Logger.initLogData(startedAt, GlobalData.shipType, GlobalData.route);
        int cooldown = Mathf.RoundToInt(Logger.interval/0.02f);
        int frame = 0;
        doLogging();
        while (true)
        {
            frame++;
            if (frame >= cooldown)
            {
                frame = 0;
                doLogging();
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
