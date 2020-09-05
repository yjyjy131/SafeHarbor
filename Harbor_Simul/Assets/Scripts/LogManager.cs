using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogManager : Singleton<LogManager>
{
    private Ship player;
    private List<MovableEntity> entities = new List<MovableEntity>();
    private List<MyRoute> routes = new List<MyRoute>();
    private MyRoute currentRoute;
    private bool isPlay = false;
    public float PlayedTime;
    public float playedTime { get { return PlayedTime; } set { PlayedTime = value; } }
    public float endTime { get; private set; }
    private GameObject map;
    public LogData logData;
    public CamPosition cam;
    public UIPosition ui;
    public LogCtrPanel logCtrPanel;
    private float timeScale = 1f;
    public bool isFoward = true;

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
        playedTime = 0;
        logData = Logger.getCurLogData();
        endTime = logData.transformDataSet[logData.transformDataSet.Count - 1].elapsedTime;

    }

    public void Start()
    {
        startReplay();
    }

    public void startReplay()
    {
        currentRoute = routes[(int)logData.route];
        currentRoute.startPoint.active();
        currentRoute.destination.active();
        player = Instantiate(Resources.Load<GameObject>("Prefabs/Ship" + (int)logData.shipType), currentRoute.startPoint.pos, Quaternion.identity).GetComponent<Ship>();
        player.transform.SetParent(map.transform.Find("Entities"));
        entities.Add(player);
        player.stopControl();
        CamManager.Instance.mainCam = player.camPos;
        CamManager.Instance.uiPos = player.uiPos;
        CamManager.Instance.monitorPos = player.MonitorCamPos;
        CamManager.Instance.monitorCamRot = player.MonitorCamPos.rotation;
        playedTime = 0;
        isPlay = true;
        StartCoroutine("replayCoroutine");
    }

    public void setPlayer(Ship ship)
    {
        player = ship;
    }

    public void changePlayedTime(float time)
    {
        stopReplay();
        playedTime = time;
        resumeReplay();
    }

    public int loadByTime(float time)
    {

        //Debug.Log("Loading Time : " + time);
        foreach (TransformDataSet data in logData.transformDataSet)
        {
            if(data.elapsedTime >= time && data.elapsedTime - time < Logger.interval)
            {
                applyFromTransformDataSet(data);
                //Debug.Log("Time Loaded : " + data.elapsedTime);
                return 1;
            }
        }
        return 0;
    }

    public void applyFromTransformDataSet(TransformDataSet dataSet)
    {
        TransFormData data =  dataSet.transFormDatas[0];
        //Debug.Log("pop : " + data.toString());
        player.loadTransform(data);
        //cam.setTransform(player.camPos.position, player.camPos.rotation);
        //ui.setTransform(player.uiPos.position, player.uiPos.rotation);
    }

    public void stopReplay()
    {
        isPlay = false;
        StopCoroutine("replayCoroutine");
    }

    public void resumeReplay()
    {
        isPlay = true;
        StopCoroutine("replayCoroutine");
        StartCoroutine("replayCoroutine");
    }
    public void setTimeScale(float multi)
    {
        if (multi == 0f)
        {
            timeScale = 1;
            return;
        }
        if (timeScale >= 32f) return;
        timeScale *= multi;
    }

    public float getTimeScale()
    {
        return timeScale;
    }

    public IEnumerator replayCoroutine()
    {
        int cooldown = Mathf.RoundToInt(Logger.interval / 0.02f);
        int frame = 0;
        loadByTime(playedTime);
        while (true)
        {
            frame++;
            if (frame >= cooldown)
            {
                frame = 0;
                if (isFoward)
                    playedTime += Logger.interval * timeScale;
                else
                    playedTime -= Logger.interval * timeScale;

                loadByTime(playedTime);

                if (playedTime >= endTime)
                {
                    Debug.Log("time ended");
                    playedTime = endTime;
                    stopReplay();
                }
                else if (playedTime <= 0)
                {
                    playedTime = 0;
                    stopReplay();
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
}
