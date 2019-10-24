using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pb;

public class NetworkController
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
    public class ProtocolMessage
    {
        public int MessageId;
        public byte[] MessageData;
        public ProtocolMessage(int id, byte[] data)
        {
            MessageId = id;
            MessageData = data;
        }
    }

    private static NetworkController _instance;
    public static NetworkController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new NetworkController();
            }
            return _instance;
        }
    }
    private SocketClient _client;
    public SocketClient Client
    {
        get
        {
            if (_client == null)
            {
                _client = new SocketClient();
                _client.OnNewProtocolMessage += this.OnNewProtocolMessage;
            }
            return _client;
        }
    }

    private Queue<ProtocolMessage> messageQueue = new Queue<ProtocolMessage>();

    public bool PauseProcMessage
    {
        set;
        get;
    }


    public void ConnectToServer(string ip, int port)
    {
        Client.ConnectToServer(ip, port);
    }
    public void SendMessage(int id, Google.Protobuf.IMessage message)
    {
        Client.SendMessage(id, message);
    }

    internal void SendMessage(Protocol messageId, Google.Protobuf.IMessage req)
    {
        SendMessage((int)messageId, req);
    }

    internal void Disconnect()
    {
        Debug.Log("NetworkController disconnect");
        Client.Disconnect();
    }

    private void OnNewProtocolMessage(int id, byte[] data)
    {
        lock (messageQueue)
        {
            messageQueue.Enqueue(new ProtocolMessage(id, data));
        }
    }


    private void MessageDispatch(int messageId, byte[] packetData)
    {
        Protocol protocolId = (Protocol)messageId;
        Debug.Log("New Message : " + messageId);
        switch (protocolId)
        {
            case Protocol.GAME_MSG_LOGON_SYNCPID://syncpid 玩家出生同步pid和姓名
                {
                    SyncPid sync = SyncPid.Parser.ParseFrom(packetData);
                    GameEventManager.OnLogon(sync.Pid, sync.Username);
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
                        //新的位置消息
                        GameEventManager.OnNewPlayer(bc);
                        GameEventManager.OnMove(bc);
                    }
                    if (bc.Tp == 4)//tp 为4,表明玩家在移动
                    {
                        GameEventManager.OnMove(bc);
                    }
                    break;
                }
            case Protocol.GAME_MSG_LOGOFF_SYNCPID:
                {
                    SyncPid sync = SyncPid.Parser.ParseFrom(packetData);
                    if (GameEventManager.OnOver != null)
                    {
                        Debug.Log("net manager OnOver trigger : " + sync.Pid);
                        GameEventManager.OnOver(sync.Pid);
                    }
                    break;
                }
            case Protocol.GAME_MSG_SUR_PLAYER:
                {
                    //刚开始上线的时候获取周围玩家的列表
                    SyncPlayers players = SyncPlayers.Parser.ParseFrom(packetData);
                    List<Pb.Player> lPlayers = new List<Pb.Player>();
                    foreach (var player in players.Ps)
                    {
                        lPlayers.Add(player);
                    }
                    GameEventManager.OnNewPlayers(lPlayers);
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
                    if (GameEventManager.OnChangeWorldResponse != null)
                    {
                        GameEventManager.OnChangeWorldResponse(res);
                    }
                    break;
                }
            case Protocol.GAME_MSG_SKILL_BROAD:
                {
                    var res = SkillTrigger.Parser.ParseFrom(packetData);
                    if (GameEventManager.OnSkillTrigger != null)
                    {
                        GameEventManager.OnSkillTrigger(res);
                    }
                    break;
                }
            case Protocol.GAME_MSG_FIRE_HIT:
                {
                    var res = SkillContact.Parser.ParseFrom(packetData);
                    if (GameEventManager.OnSkillContact != null)
                    {
                        GameEventManager.OnSkillContact(res);
                    }
                    break;
                }
            default:
                break;
        }
    }

    //call this function in main thread
    public void ProcessMessages()
    {
        if (PauseProcMessage)
        {
            Debug.Log("ProcessMessages pause in this frame");
            return;
        }
        while (messageQueue.Count != 0 && !PauseProcMessage)
        {
            Debug.Log("Process message , deltaTime : " + Time.deltaTime);
            int messageId;
            byte[] packetData;
            lock (messageQueue)
            {
                var msg = messageQueue.Dequeue();
                messageId = msg.MessageId;
                packetData = msg.MessageData;
            }
            MessageDispatch(messageId, packetData);
        }
    }
}
