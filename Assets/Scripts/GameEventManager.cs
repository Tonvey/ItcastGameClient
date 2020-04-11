using System.Collections.Generic;
using UnityEngine;
using System;
using Pb;

public class GameEventManager
{
    //玩家获取pid的事件
    public static Action<int, string> OnLogon;
    //当有新的玩家创建的时候的事件
    public static Action<Pb.BroadCast> OnNewPlayer;
    public static Action<List<Pb.Player>> OnNewPlayers;
    //move ,玩家移动的事件
    public static Action<Pb.BroadCast> OnMove;
    //leave ,玩家离开的事件 ，pid
    public static Action<int> OnOver;
    //name ,玩家更新名字的事件
    public static Action<string> OnName;
    //有新的聊天信息，pid + 名字 + 内容 
    public static Action<int, string, string> OnNewChatMessage;
    //玩家正在输入播聊天内容的事件 ，true表示正在聊天，聊天框是焦点，false表示恢复默认状态
    public static Action<bool> OnChatting;
    //bloodvalue
    public static Action<int> OnBlood;
    public static Action<ChangeWorldResponse> OnChangeWorldResponse;
    public static Action<SkillTrigger> OnSkillTrigger;
    public static Action<SkillContact> OnSkillContact;
    public static event Action<Vector4> OnPlayerMove;
}
