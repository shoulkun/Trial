using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed;                     //移动速度
    public float backSpeed;                 //往后退的速度
    public float runSpeed;                  //跑的速度
    private float runAcceleratTime;          //加速到跑需要的时间
    public float maxStrength;               //最大体力值
    public float recStrSpeed;               //恢复体力的速度
    public float runReduceStr;              //跑步消耗的体力
    public float waitForRecStrToRun;        //体力耗尽，恢复到多少的时候能跑

    private bool runStrCheck = false;       //当前体力是否支持跑步
    private float strength;                 //当前体力
    private float runTime;                  //跑步加速度的时间
    private Vector3 positionBufferBuffer;   //上上一帧的位置
    private Vector3 positionBuffer;         //上一帧的位置
    private bool runBuffer;                 //上一帧是否在跑动

    private float ws, ad;               //移动方向
    private Vector3 dir, move;          //移动方向向量 最终移动计算结果
    private bool backFlag, forwardFlag; //是否在往 后 前
    private Vector3 dirNowBufferReduce; //上一帧是否有移动
    // Start is called before the first frame update
    void Start()
    {
        strength = maxStrength;
    }

    // Update is called once per frame
    void Update()
    {
        KeyInput();
        MoveAndRunAndTiptoe();
        RecStrength();
        //Debug.Log(strength);
    }

    void KeyInput()
    {
        ws = 0;
        ad = 0;
        dirNowBufferReduce = positionBuffer - positionBufferBuffer;
        //移动输入事件
        if (Input.GetKey(KeyCode.W))
        {
            ws = 1;
            forwardFlag = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            ws = -1;
            backFlag = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            ad = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            ad = 1;
        }
        //移动的方向
        dir = new Vector3(ad, 0, ws).normalized;
    }
    void MoveAndRunAndTiptoe()
    {
        //跑步
        if (Input.GetKey(KeyCode.LeftShift) && forwardFlag && (runStrCheck && runBuffer))
        {
            //在移动的情况下
            if(dirNowBufferReduce != Vector3.zero || dir != Vector3.zero)
            {
                //加速过程
                if (runTime < runAcceleratTime)
                {
                    runTime += Time.deltaTime;
                }
                move = dir * runSpeed * runTime * Time.deltaTime + dir * speed * Time.deltaTime;
                strength -= runReduceStr;
                runBuffer = true;
            }
            //在静止的情况下
            else
            {
                runTime = 0;
                move = dir * runSpeed * runTime * Time.deltaTime;
                runBuffer = false;
            }
        }

        //走路
        else
        {
            //跑步后减速过程
            if (runTime > 0)
            {
                runTime -= Time.deltaTime;
            }
            move = dir * speed * Time.deltaTime  + dir * runSpeed * runTime * Time.deltaTime;
            runBuffer = false;
        }

        //判断是否在后退
        if(backFlag)
        {
            move *= backSpeed;
        }

        //计算最终结果
        this.gameObject.transform.Translate(move);

        //上上一帧的位置
        positionBufferBuffer = positionBuffer;
        //上一帧的位置
        positionBuffer = this.transform.position;


        if (strength > 0)
        {
            runStrCheck = true;
        }
        else if (strength == 0)
        {
            runStrCheck = false;
            runBuffer = false;
        }
        if (strength >= waitForRecStrToRun)
        {
            runBuffer = true;
        }
    }
    void RecStrength()
    {
        if(strength > maxStrength)
        {
            strength = maxStrength;
        }
        else if(strength < 0)
        {
            strength = 0;
        }
        strength += recStrSpeed;
    }

}
