using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private float time = 0;
    private Ship player;
    private List<MovableEntity> entities = new List<MovableEntity>();
    private ArrivePanel arrivePanel;
    private List<MyRoute> routes = new List<MyRoute>();
    private MyRoute currentRoute;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        setInstance(this);
        arrivePanel = GameObject.Find("Canvas").transform.Find("ArrivePanel").GetComponent<ArrivePanel>();
        foreach(MyRoute child in GameObject.Find("Routes").transform.GetComponentsInChildren<MyRoute>())
        {
            routes.Add(child);
        }
        foreach (MovableEntity child in GameObject.Find("Entities").transform.GetComponentsInChildren<MovableEntity>())
        {
            entities.Add(child);
        }
    }

    private void Start()
    {
        
    }

    public void StartGame()
    {
        currentRoute = routes[(int)GlobalData.route];
        currentRoute.destination.active();
        player = Instantiate(Resources.Load<GameObject>("Prefabs/Ship"), currentRoute.startPoint.pos, Quaternion.identity).GetComponent<Ship>();
        StartCoroutine("logCoroutine");
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
