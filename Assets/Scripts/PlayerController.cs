﻿using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;
using System.IO;
public class PlayerController : Role
{
    //子弹，ID 递增
    private static int BoltId = 1;
    //角色动画控制器
    public Animator playerAnimator;
    //子弹对象
    public GameObject bolt;
    //子弹速度
    public float boltSpeed = 1f;
    //角色移动速度
    public float speed; //运动速度
    //角色控制器
    public CharacterController playerCharacterConctl; 

    //距离上次同步位置的时间差
    private float _lastSyncPositionTimeSpec=0f;
    //上次同步的位置
    private Vector4 _lastSyncPosition = Vector4.zero;

    //是否已经获取出生点位置,0表示未获取,1表示已经获取但是未同步到模型上,2表示已经同步到模型上
    private int _initPosState = 0 ;

    //四元组存储 玩家的xyz坐标以及面向的角度
    private Vector4 InitPos;

    //能否攻击以及移动的开关
    private bool canAttack = true;
    private bool canMove = true;


    void MoveAction()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if(Mathf.Abs( moveHorizontal)>0.0f|| Mathf.Abs(moveVertical)>0.0f)
        {
            this.transform.eulerAngles = new Vector3(transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, transform.localEulerAngles.z);
            Vector3 velocity = (transform.forward * moveVertical + transform.right * moveHorizontal) * speed;
            playerCharacterConctl.SimpleMove(velocity);
        }
    }
  
    IEnumerator WaitAndRun(float seconds,Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
    private void AttackAction()
    {
        if (!canAttack)
            return;
        canAttack = false;
        StartCoroutine(WaitAndRun(1,
            new Action(() =>
        {
            canAttack = true;
        })));
        this.transform.eulerAngles = new Vector3(transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, transform.localEulerAngles.z);
        this.playerAnimator.SetTrigger("Attack");
        Vector3 pos = this.transform.position + this.transform.up + this.transform.forward*3.5f;
        var bulletObj = Instantiate(bolt, pos, this.bolt.transform.rotation);
        bulletObj.transform.rotation = Quaternion.AngleAxis(90, this.transform.right) * this.transform.rotation;
        var bc = bulletObj.GetComponent<BoltContoller>();
        bc.BornPos = pos;
        bc.PlayerId = this.Pid;
        bc.BulletId = BoltId++;
        bc.SkillId = 1;
        var rgbody = bulletObj.GetComponent<Rigidbody>();
        rgbody.velocity = this.transform.forward * this.boltSpeed;

        SkillTrigger fire = new SkillTrigger();
        fire.Pid = this.Pid;
        fire.SkillId = 1;
        Position p = new Position();
        fire.BulletId = bc.BulletId;
        p.X = bulletObj.transform.position.x;
        p.Y = bulletObj.transform.position.y;
        p.Z = bulletObj.transform.position.z;
        p.V = bulletObj.transform.localEulerAngles.y;

        Velocity v = new Velocity();

        v.X = rgbody.velocity.x;
        v.Y = rgbody.velocity.y;
        v.Z = rgbody.velocity.z;
        fire.P = p;
        fire.V = v;
        NetworkController.Instance.SendMessage(NetworkController.Protocol.GAME_MSG_SKILL_TRIGGER,fire);
    }
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        //关注NetMgr的一些网络事件
        GameEventManager.OnLogon += OnLogon;
        GameEventManager.OnMove += OnMove;
        GameEventManager.OnSkillContact += OnSkillContact;
        GameEventManager.OnChatting += OnChatting;
    }

    private void OnChatting(bool isChatting)
    {
        this.canMove = !isChatting;
    }

    private void OnSkillContact(SkillContact contact)
    {
        if(contact.TargetPid==this.Pid)
        {
            this.HP = contact.ContactPos.BloodValue;
        }
    }

    private void OnLogon(int pid, string userName)
    {
        this.PlayerName = userName;
        this.Pid = pid;
    }

    private void OnMove(BroadCast obj)
    {
        //完成当前玩家的移动
        //因为当前玩家是这个世界的主角
        //只接受第一次的报文,将玩家放到出生点
        if(_initPosState==0)
        {
            this.InitPos = new Vector4(obj.P.X, obj.P.Y, obj.P.Z, obj.P.V);
            _initPosState = 1;
            this.HP = obj.P.BloodValue;
            Debug.Log("Get HP:" + obj.P.BloodValue);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(_initPosState==1)//表示已经获取出生点,但是未同步到模型上
        {
            //已经获取了初始化位置
            //将当前玩家移动到出生点
            this.transform.position = new Vector3(InitPos.x, InitPos.y, InitPos.z);
            //还有旋转的角度,单纯的做Y轴的旋转
            this.transform.rotation = Quaternion.Euler(0, InitPos.w, 0);
            _initPosState = 2;
        }
        if(canMove)
        {
            MoveAction();
            MouseEvent();
        }
    }

    private void MouseEvent()
    {
        if(SceneController.CurrentSceneController!=null&&SceneController.CurrentSceneController.PlayerCanAttack)
        {
            if (Input.GetButton("Fire1"))//1代表右键
            {
                //InputGameData.Instance.AttackTrigger = true;
                AttackAction();
            }

        }
    }


    private void FixedUpdate()
    {
        //两次发送位置同步的事件间隔必须大于0.1秒
        if(_lastSyncPositionTimeSpec>0.2f) 
        {
            _lastSyncPositionTimeSpec = 0f;
            Vector4 pos = new Vector4(
                transform.position.x,
                transform.position.y,
                transform.position.z,
                transform.localEulerAngles.y
                );
            if(!pos.Equals(_lastSyncPosition))
            {
                SendPosition();
                _lastSyncPosition = pos;
            }
        }
        else
        {
            this._lastSyncPositionTimeSpec += Time.deltaTime;
        }
    }

    private void SendPosition()
    {
        Pb.Position pos = new Pb.Position();
        pos.X = this.transform.position.x;
        pos.Y = this.transform.position.y;
        pos.Z = this.transform.position.z;
        pos.V = this.transform.localEulerAngles.y; //当前玩家的面朝方向应该获取欧拉角度的y轴旋转角度
        pos.BloodValue = this.HP;
        NetworkController.Instance.SendMessage(3,pos);
    }
    private void OnDestroy()
    {
        GameEventManager.OnLogon -= OnLogon;
        GameEventManager.OnMove -= OnMove;
        GameEventManager.OnSkillContact -= OnSkillContact;
        GameEventManager.OnChatting -= OnChatting;
    }
}
