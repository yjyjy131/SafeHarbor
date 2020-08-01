using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField]
    private float angleBias = 50000f;
    [SerializeField]
    private float speedBias = 20000f;
    private bool isControllable = true;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isControllable)
        {
            Vector3 angleVector = Quaternion.Euler(0, InputSystem.Instance.angle, 0) * transform.forward;
            rigid.AddForce(InputSystem.Instance.speed * transform.forward * Time.deltaTime * speedBias);
            rigid.AddForce(angleVector * Time.deltaTime * angleBias);
        }
    }

    public void stopControl()
    {
        isControllable = false;
    }

    public void startControl()
    {
        isControllable = false;
    }
}
