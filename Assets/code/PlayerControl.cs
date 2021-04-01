using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private float ws, ad;               //移动方向
    public float moveSpeed = 2;         //移动速度

    public float turnArroundSpeed = 10; //转身速度
    private Quaternion dir; //移动方向向量 最终移动计算结果
    private Vector3 move; //按键检测
    private Transform modelT;
    private Transform playerT;
    private Transform dirObjT;
    private PlayerCast playerCast;

    // Start is called before the first frame update
    void Start()
    {
        playerT = GameObject.Find("player").transform;
        dirObjT = GameObject.Find("centerPoint").transform;
        modelT = GameObject.Find("playerModel").transform;
        playerCast = GameObject.Find("playerCast").GetComponent<PlayerCast>();
    }

    // Update is called once per frame
    void Update()
    {
        KeyInput();
    }

    void KeyInput()
    {
        ws = 0;
        ad = 0;
        //移动输入事件
        if (Input.GetKey(KeyCode.W))
        {
            ws = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            ws = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            ad = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            ad = 1;
        }

        //让移动方向参考系 朝向视角方向为正面
        move = new Vector3(ad, 0, ws).normalized;
        dir = Quaternion.Euler(new Vector3(0, dirObjT.eulerAngles.y, dirObjT.eulerAngles.z) - playerT.eulerAngles).normalized;

        //当按下方向键
        if (move != Vector3.zero)
        {
            //模型面朝向
            if(move.z < 0)
            {
                modelT.rotation = Quaternion.Lerp(modelT.rotation, Quaternion.AngleAxis(180 + dirObjT.localEulerAngles.y - 90 * move.x, Vector3.up), Time.deltaTime * turnArroundSpeed);
            }
            else
            {
                modelT.rotation = Quaternion.Lerp(modelT.rotation, Quaternion.AngleAxis(90 * move.x + dirObjT.localEulerAngles.y, Vector3.up), Time.deltaTime * turnArroundSpeed);
            }
        }

        //奔跑
        if(Input.GetKey(KeyCode.LeftShift) && (ws != 0 || ad != 0))
        {
            move *= 2;
        }

        //只允许y轴旋转
        modelT.eulerAngles = new Vector3(0,modelT.eulerAngles.y,0);

        Vector3 from = modelT.position;
        Vector3 v3Dis = (dir * move) * moveSpeed * Time.deltaTime;
        Vector3 to = from + v3Dis * 100;
        // RaycastHit rh;

        // Debug.DrawLine(from, to, Color.black);
        // if (Physics.Linecast(from, to, out rh, LayerMask.GetMask("Default")))
        // {
        //     v3Dis = (dir * move) * 0 * Time.deltaTime;
        // }

        //移动方向即将发生碰撞
        if (playerCast.isCast)
        {
            v3Dis = (dir * move) * 0 * Time.deltaTime;
        }
        //移动
        playerT.Translate(v3Dis);
    }
}