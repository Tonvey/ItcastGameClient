using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pb;

public class NetManager : MonoBehaviour
{
    public enum Protocol
    {
        GAME_MSG_LOGON_SYNCPID = 1,
        GAME_MSG_TALK_CONTENT = 2,
        GAME_MSG_NEW_POSTION = 3,

        GAME_MSG_SKILL_FIRE = 4,
        GAME_MSG_SEE_HIT = 5,
        GAME_MSG_CHANGE_WORLD = 6,

        GAME_MSG_BROADCAST = 200,
        GAME_MSG_LOGOFF_SYNCPID = 201,
        GAME_MSG_SUR_PLAYER = 202,
        GAME_MSG_SKILL_BROAD = 204,
        GAME_MSG_FIRE_HIT = 205,
        GAME_MSG_CHANGE_WORLD_R = 206,
    }

    //玩家获取pid的事件
    public static Action<int,string> OnLogon;

    //当有新的玩家创建的时候的事件
    public static Action<Pb.BroadCast> OnNewPlayer;
    public static Action<List<Pb.Player>> OnNewPlayers;

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
    public static NetManager Instance
    {
        private set;
        get;
    }
    private SocketClient _client;
    public SocketClient Client
    {
        get
        {
            return _client;
        }
    }
    public void ConnectToServer(string ip , int port)
    {
        Client.ConnectToServer(ip, port);
    }
    public void SendMessage(int id ,Google.Protobuf.IMessage message)
    {
        Client.SendMessage(id, message);
    }

    internal void SendMessage(Protocol messageId, Google.Protobuf.IMessage req)
    {
        SendMessage((int)messageId, req);
    }

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        _client = new SocketClient();
        _client.OnNewProtocolMessage += this.NetLogic;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Disconnect()
    {
        Client.Disconnect();
    }

    private void NetLogic(int messageId, byte[] packetData)
    {
        Protocol protocolId = (Protocol)messageId;
        Debug.Log("New Message : " + messageId);
        switch (protocolId)
        {
            case Protocol.GAME_MSG_LOGON_SYNCPID://syncpid 玩家出生同步pid和姓名
                {
                    SyncPid sync = SyncPid.Parser.ParseFrom(packetData);
                    NetManager.OnLogon(sync.Pid,sync.Username);
                    //将自己存到List里边
                    break;
                }
            case Protocol.GAME_MSG_BROADCAST://广播消息
                {
                    BroadCast bc = BroadCast.Parser.ParseFrom(packetData);
                    if (bc.Tp == 1)
                    {
                        //聊天消息
                        if (bc.Content != null)
                        {
                            TalkCenter.Content = bc.Content;
                            TalkCenter.PlayerName = bc.Username;
                            TalkCenter.TalkFlag = true;
                        }

                    }
                    else if (bc.Tp == 2)
                    {
                        //bool playerExists = false;
                        //判断当前传进来的玩家的移动信息是一个新的玩家,还是一个已经存在于当前场景的一个玩家
                        //lock (GameController.PlayerList)
                        //{
                        //    playerExists = GameController.PlayerList.Contains(bc.Pid);
                        //    if (!playerExists)
                        //    {
                        //        //考虑将玩家创建到场景中
                        //        NetManager.OnNewPlayer(bc);
                        //        //将当前玩家添加到list里边
                        //        GameController.PlayerList.Add(bc.Pid);
                        //    }
                        //}
                        //新的位置消息
                        NetManager.OnNewPlayer(bc);
                        NetManager.OnMove(bc);
                    }
                    if (bc.Tp == 4)//tp 为4,表明玩家在移动
                    {
                        NetManager.OnMove(bc);
                    }
                    break;
                }
            case Protocol.GAME_MSG_LOGOFF_SYNCPID:
                {
                    SyncPid sync = SyncPid.Parser.ParseFrom(packetData);
                    NetManager.OnOver(sync.Pid);
                    break;
                }
            case Protocol.GAME_MSG_SUR_PLAYER:
                {
                    //刚开始上线的时候获取周围玩家的列表
                    SyncPlayers players = SyncPlayers.Parser.ParseFrom(packetData);
                    List<Pb.Player> lPlayers = new List<Pb.Player>();
                    foreach(var player in players.Ps)
                    {
                        lPlayers.Add(player);
                    }
                    OnNewPlayers(lPlayers);
                    //lock (GameController.PlayerList)
                    //{
                    //    //将玩家都全部添加玩家列表进来
                    //    foreach (var player in players.Ps)
                    //    {
                    //        if (!GameController.PlayerList.Contains(player.Pid))
                    //        {
                    //            GameController.PlayerList.Add(player.Pid);
                    //            BroadCast bc = new BroadCast();
                    //            bc.Pid = player.Pid;
                    //            bc.P = player.P;
                    //            bc.Username = player.Username;
                    //            NetManager.OnNewPlayer(bc);
                    //        }
                    //    }
                    //}
                    break;
                }
            case Protocol.GAME_MSG_CHANGE_WORLD_R:
                {
                    var res = ChangeWorldResponse.Parser.ParseFrom(packetData);
                    Debug.Log("ChangWorld response : " + res.ChangeRes);
                    if(OnChangeWorldResponse!=null)
                    {
                        OnChangeWorldResponse(res);
                    }
                    break;
                }
            case Protocol.GAME_MSG_SKILL_BROAD:
                {
                    var res = SkillTrigger.Parser.ParseFrom(packetData);
                    if(OnChangeWorldResponse!=null)
                    {
                        OnSkillTrigger(res);
                    }
                    break;
                }
            default:
                break;
        }
    }
}
