using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCast : MonoBehaviour
{
    private GameObject model;
    private BoxCollider playerCast;
    public bool isCast = false;
    public float Thickness = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        //初始化移动方向碰撞器
        model = GameObject.Find("playerModel");
        playerCast = GameObject.Find("playerCast").GetComponents<BoxCollider>()[0];
        playerCast.center = new Vector3(0, 0, model.transform.localScale.z/2 + Mathf.Epsilon + Thickness/2f);
        playerCast.size = new Vector3(model.transform.localScale.x * 0.9f, model.transform.localScale.y - 0.1f, Thickness);
    }

    // Update is called once per frame
    void Update()
    {

    }
    //检测移动方向是否碰撞
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 0)
        {
            isCast = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if(col.gameObject.layer == 0)
        {
            isCast = false;
        }
    }
}
