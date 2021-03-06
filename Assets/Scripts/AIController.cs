﻿using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;

public class AIController : Role
{
    public event Action<int> OnUserDestroy;
    //当前角色的移动速度
    public float speed = 5.0f;
    private CharacterController aiCharaterController;
    private Animator animator;
    public GameObject bolt;
    public float boltSpeed = 1f;
    private string _playerName;
    private Coroutine movingCoroutine = null;
    private Vector3 mTargetPos = Vector3.zero;
    private bool _playerNameChanged = false;
    internal void InitPlayer(int pid, string username, float x, float y, float z, float v,int hp)
    {
        Debug.Log("AiController init");
        this.Pid = pid;
        this.PlayerName = username;
        this.transform.position = new Vector3(x, y, z);
        this.transform.rotation = Quaternion.Euler(0,v,0);
        this.HP = hp;
    }
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Debug.Log("AiController start");
        GameEventManager.OnMove += OnMove;//监听玩家移动的事件 
        GameEventManager.OnOver += OnOver;//监听玩家离线的事件
        GameEventManager.OnSkillTrigger += OnSkillTrigger;
        GameEventManager.OnSkillContact += OnSkillContact;
        aiCharaterController = this.GetComponent<CharacterController>();
        animator = this.GetComponent<Animator>();
    }

    private void OnSkillContact(SkillContact contact)
    {
        if(contact.TargetPid == this.Pid)
        {
            Debug.LogFormat("AI:{0} be hit , hp : {1}", this.Pid, contact.ContactPos.BloodValue);
            this.HP = contact.ContactPos.BloodValue;
        }
    }
    private void OnOver(int _pid)
    {
        Debug.Log("AiController OnOver");
        if(_pid==this.Pid)
        {
            actionsNextFrame.Enqueue(() => Destroy(this.gameObject));
        }
    }
    private void OnSkillTrigger(SkillTrigger trigger)
    {
        if(trigger.Pid!=this.Pid)
        {
            return;
		}
        this.animator.SetTrigger("Attack");
        Vector3 pos = this.transform.position + this.transform.up + this.transform.forward * 3.5f;
        var bulletObj = Instantiate(bolt, pos, this.bolt.transform.rotation);
        bulletObj.transform.rotation = Quaternion.AngleAxis(90, this.transform.right) * this.transform.rotation;
        var bc = bulletObj.GetComponent<BoltContoller>();
        bc.BornPos = pos;
        bc.PlayerId = this.Pid;
        bc.BulletId = trigger.BulletId;
        bc.SkillId = trigger.SkillId;
        var rgbody = bulletObj.GetComponent<Rigidbody>();
        rgbody.velocity = new Vector3(trigger.V.X, trigger.V.Y, trigger.V.Z);
    }

    private void OnMove(BroadCast bc)
    {
        if(bc.Pid == this.Pid)
        {
            this.HP = bc.P.BloodValue;
            //坐标改变了
            this.transform.rotation = Quaternion.Euler(0, bc.P.V, 0);
            //先判断一下目的地离当前位置的距离,如果很大,那么就使用协程每一帧都自动生成一个平滑的移动
            mTargetPos = new Vector3(bc.P.X, bc.P.Y, bc.P.Z);
            //每次启动协程之前先停止一下上一次的协程
            if(this.movingCoroutine!=null)
            { 
                StopCoroutine(movingCoroutine);
            }
            this.movingCoroutine = StartCoroutine(MoveLogic());
        }
    }

    IEnumerator MoveLogic()
    {
        //只要协程启动,每一帧都会迭代一下
        while (Vector3.Distance(this.transform.position, mTargetPos)>0.1f)
        {
            //处理移动一点点距离
            //计算速度向量
            Vector3 dirVector = mTargetPos - this.transform.position;
            //获取方向向量的单位向量,然后乘以速度,就可以得到一个速度的向量
            Vector3 speedVect = dirVector.normalized * this.speed;
            aiCharaterController.SimpleMove(speedVect);
            yield return 0; 
        }
        this.transform.position = mTargetPos;
        this.movingCoroutine = null;
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    private void OnDestroy()
    {
        Debug.Log("AiController OnDestroy");
        if(OnUserDestroy!=null)
            OnUserDestroy(Pid);
        GameEventManager.OnMove -= OnMove;//监听玩家移动的事件 
        GameEventManager.OnOver -= OnOver;//监听玩家离线的事件
        GameEventManager.OnSkillTrigger -= OnSkillTrigger;
        GameEventManager.OnSkillContact -= OnSkillContact;
    }
}
