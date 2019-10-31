using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pb;

public class GameEventManager : MonoBehaviour
{
    public static Action TestEvent;
    //玩家获取pid的事件
    public static Action<int, string> OnLogon;

    //当有新的玩家创建的时候的事件
    public static Action<Pb.BroadCast> OnNewPlayer;
    public static Action<List<Pb.Player>> OnNewPlayers;

    //有新的聊天信息
    public static Action<int, string, string> OnNewChatMessage;

    public static Action<bool> OnChatting;

    //move ,玩家移动的事件
    public static Action<Pb.BroadCast> OnMove;
    //leave ,玩家离开的事件
    public static Action<int> OnOver;
    //name ,玩家更新名字的事件
    public static Action<string> OnName;
    //bloodvalue
    public static Action<int> OnBlood;
    public static Action<ChangeWorldResponse> OnChangeWorldResponse;
    public static Action<SkillTrigger> OnSkillTrigger;
    public static Action<SkillContact> OnSkillContact;
    public static event Action<Vector4> OnPlayerMove;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
