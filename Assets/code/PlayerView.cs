using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerView : MonoBehaviour
{
    public float speed;
    //移动镜头速度
    public float XLimit;
    //镜头角度限制
    public float smoothTime;
    //镜头平滑时间
    private float offSetMult;
    //摄像头偏移倍率
    public Vector3 OffsetRayLen;
    //偏移向量（方向）
    private float mouseX;
    //鼠标X轴
    private float mouseY;
    //鼠标Y轴
    private Vector3 currentVelocity = Vector3.zero;
    //忘了干嘛的了，貌似是一个函数充数的
    private GameObject playerCamera;
    //玩家相机
    private GameObject cameraPoint;
    //玩家相机（最终位置有offSetMult偏移）
    private GameObject playerPoint;
    //玩家
    private GameObject centerPoint;
    //玩家中心点
    private GameObject target;
    //测试用目标
    private Vector3[] viewerProfiles;
    //相机预制位置 默认三个
    private Vector3[] hitProfiles;
    //相机碰撞检测预制位置 默认三个
    private GameObject hitPoint;
    //防止碰撞检测点
    private int defaultV;
    // 选择的视角
    // Start is called before the first frame update
    void Start()
    {
        //各种初始化
        offSetMult = 0.5f;
        hitPoint = GameObject.Find("hitPoint");
        playerCamera = GameObject.Find("playerCamera");
        target = GameObject.Find("target");
        cameraPoint = GameObject.Find("cameraPoint");
        playerPoint = GameObject.Find("player");
        centerPoint = GameObject.Find("centerPoint");

        Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
        Cursor.visible = false;//隐藏指针

        OffsetRayLen = (hitPoint.transform.localPosition - centerPoint.transform.localPosition).normalized;

        // 预制四个视角
        viewerProfiles = new Vector3[4] {centerPoint.transform.localPosition, cameraPoint.transform.localPosition, cameraPoint.transform.localPosition + OffsetRayLen, cameraPoint.transform.localPosition + OffsetRayLen * 2 };
        hitProfiles = new Vector3[4] {centerPoint.transform.localPosition, hitPoint.transform.localPosition + OffsetRayLen * offSetMult, hitPoint.transform.localPosition + OffsetRayLen * (1 + offSetMult), hitPoint.transform.localPosition + OffsetRayLen * (2 + offSetMult) };
        defaultV = 1;
        cameraPoint.transform.localPosition = viewerProfiles[defaultV];
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // 锁定视角角度
        CameraLock();
        // 切换视角长度
        ChangeViewer();
    }

    void Update()
    {
        // 记录鼠标移动
        mouseX = Input.GetAxis("Mouse X") * speed;
        mouseY = -(Input.GetAxis("Mouse Y")) * speed;
        //TargetAngle();
        // 旋转摄像头
        RotateCamera();

        // 检测第一人称 摄像头是否到达指定位置
        if(defaultV == 0 && (playerCamera.transform.position == centerPoint.transform.position))
        {
            cameraPoint.transform.localPosition = viewerProfiles[defaultV];
            hitPoint.transform.localPosition = hitProfiles[defaultV];
        }
        // 检测摄像头遮挡 //第一人称若达到指定位置，则不检查
        else if (AvoidCrossWall())
        {
            /// <summary>
            /// 发生碰撞时 不能切换视角长度
            /// </summary>
            cameraPoint.transform.localPosition = viewerProfiles[defaultV];
            hitPoint.transform.localPosition = hitProfiles[defaultV];
            //修正相机及碰撞点位置
        }
    }

    void ChangeViewer()
    {
        /// <summary>
        /// 切换视角长度
        /// </summary>
        if (Input.GetKeyDown("v"))
        {
            //修改defaultV，使viewerProfiles和hitProfiles数组在预制的多个元素进行切换
            defaultV = (defaultV + 1) % 4;
        }
    }

    Boolean AvoidCrossWall()
    {
        /// <summary>
        /// 检测摄像头的hitPoint到人物之间的是否有障碍物
        /// 至于为啥要多加一个hitPoint，是为了稍微提前检测到碰撞，这样比较简便
        /// </summary>

        RaycastHit hit;

        Debug.DrawRay(centerPoint.transform.position, hitPoint.transform.position - centerPoint.transform.position);
        // 人物中心 到 玩家相机hitPoint的 向量 之间是否有物体
        if (Physics.Raycast(centerPoint.transform.position, hitPoint.transform.position - centerPoint.transform.position, out hit, Vector3.Distance(hitPoint.transform.position, centerPoint.transform.position)))
        {
            // 有物体 且该物体 在 摄像头与人物之间
            if (hit.collider.tag == "Untagged" && Vector3.Distance(hit.point, centerPoint.transform.position) <= Vector3.Distance(cameraPoint.transform.position, centerPoint.transform.position))
            {
                // 设置摄像头点位于碰撞点上
                cameraPoint.transform.position = hit.point;
                // 碰撞检测点随之而动
                hitPoint.transform.position = hit.point;
                hitPoint.transform.localPosition += OffsetRayLen * offSetMult;
                CameraFollow(smoothTime/3);
                return false;
            }
            //改成true虽然能实时更新，但是镜头会乱跳
            CameraFollow(smoothTime/3);
            return false;
        }
        // 检测到第一人称时
        if(defaultV == 0)
        {
            // 防止奔跑时摄像头不能到达正确位置
            if((playerCamera.transform.position - centerPoint.transform.position).sqrMagnitude < 1)
            {
                CameraFollow(0);
                return true;
            }
            // 切换为第一人称时为二倍速
            CameraFollow(smoothTime/2);
            return true;
        }
        CameraFollow(smoothTime);
        return true;
    }
    void CameraFollow(float time)
    {
        /// <summary>
        /// 摄像头平滑跟随
        /// </summary>
        /// cameraPoint 减去 (hitPoint.transform.position - centerPoint.transform.position).normalized 是适当给playerCamera一定偏移值，防止遇到遮挡时穿模
        playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, cameraPoint.transform.position - (hitPoint.transform.position - centerPoint.transform.position).normalized * offSetMult, ref currentVelocity, time * Time.deltaTime);
    }

    void CameraLock()
    {
        /// <summary>
        /// 摄像头角度锁定
        /// </summary>
        float currentRotateX = centerPoint.transform.eulerAngles.x;
        //将unity修改后的旋转值恢复回来 ，也就是负数 保持原样
        if (currentRotateX > 180)
        {
            currentRotateX = currentRotateX - 360;//这样-5度还是-5度 而不是355！
        }
        // 冻结z轴
        playerCamera.transform.eulerAngles = new Vector3(playerCamera.transform.eulerAngles.x ,playerCamera.transform.eulerAngles.y,0);
        //限制角度
        if (currentRotateX >= XLimit)
        {
            centerPoint.transform.localEulerAngles = new Vector3(XLimit, centerPoint.transform.localEulerAngles.y, 0);
            playerCamera.transform.eulerAngles = new Vector3(XLimit, centerPoint.transform.eulerAngles.y, 0);
            mouseX = 0;
        }
        else if (currentRotateX <= -XLimit)
        {
            centerPoint.transform.localEulerAngles = new Vector3(-XLimit, centerPoint.transform.localEulerAngles.y, 0);
            playerCamera.transform.eulerAngles = new Vector3(360 - XLimit, centerPoint.transform.eulerAngles.y, 0);
            mouseX = 0;
        }
    }

    void RotateCamera()
    {
        /// <summary>
        /// 鼠标控制摄像头
        /// <summary>
        //镜头转向核心代码
        //人物旋转
        //playerPoint.transform.RotateAround(centerPoint.transform.position, Vector3.up, mouseX);
        //摄像头点位旋转
        centerPoint.transform.RotateAround(centerPoint.transform.position, centerPoint.transform.right, mouseY);
        centerPoint.transform.RotateAround(centerPoint.transform.position, Vector3.up, mouseX);
        //摄像头自转
        playerCamera.transform.RotateAround(centerPoint.transform.position, centerPoint.transform.right, mouseY);
        playerCamera.transform.RotateAround(centerPoint.transform.position, Vector3.up, mouseX);
    }

    void TargetAngle()
    {
        /// <summary>
        /// 目标是否被玩家看见检测，若看见，则不动
        /// </summary>
        Vector3 playerForward = playerCamera.transform.forward;
        Vector3 playerToTarget = target.transform.position - this.transform.position;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        float PTTDot = Vector3.Dot(playerForward, playerToTarget);  //player 与 target 的角度 点积 大于0时在前面

        RaycastHit hit;
        //视野角度内遇到target, 中间有障碍
        if (PTTDot > 0 && !Physics.Raycast(playerCamera.transform.position, playerToTarget, out hit, playerToTarget.magnitude, layerMask))
        {
            Debug.Log(hit.collider);
            Debug.DrawRay(playerCamera.transform.position, playerToTarget, Color.red);
            target.GetComponent<NavMeshAgent>().speed = 0f;
        }
        //视野外
        else
        {
            Debug.DrawRay(playerCamera.transform.position, playerToTarget, Color.white);
            target.GetComponent<NavMeshAgent>().speed = 1f;
        }
    }
}
