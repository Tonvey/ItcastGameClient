using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;

public class WorldSceneController : SceneController
{
    //存储当前场景里边所有玩家的list
    public List<int> PlayerList = new List<int>();   //ArrayList存储的都是object,什么类型的东西都可以存储
    public List<Pb.BroadCast> NewPlayerList = new List<Pb.BroadCast>();   //在下一帧要创建的玩家的列表
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        NetManager.OnNewPlayer += OnNewPlayer;
        NetManager.OnNewPlayers += OnNewPlayers;
        NetManager.OnLogon += OnLogon;
    }

    private void OnNewPlayers(List<Player> l)
    {
        //要在主线程取创建一个玩家对象
        lock (NewPlayerList)
        {
            foreach(var player in l)
            {
                Pb.BroadCast bc = new Pb.BroadCast();
                bc.Username = player.Username;
                bc.Pid = player.Pid;
                bc.P = player.P;
                NewPlayerList.Add(bc);
            }
        }
    }

    private void OnLogon(int pid, string name)
    {
        Debug.Log(string.Format("User logon , pid : {0}, name : {1}", pid, name));
        this.AddPlayerToList(pid);
    }

    public void AddPlayerToList(int pid)
    {
        lock (this.PlayerList)
        {
            if(!PlayerList.Contains(pid))
            {
                this.PlayerList.Add(pid);
            }
        }
    }
    public void RemovePlayerFromList(int pid)
    {
        lock(this.PlayerList)
        {
            this.PlayerList.Remove(pid);
        }
    }

    private void OnNewPlayer(BroadCast bc)
    {
        //要在主线程取创建一个玩家对象
        lock (NewPlayerList)
        {
            NewPlayerList.Add(bc);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (NewPlayerList.Count > 0)
        {
            lock (NewPlayerList)
            {
                foreach (var o in NewPlayerList)
                {
                    Debug.Log(string.Format("New Player id : {0} , name : {1}", o.Pid, o.Username));
                    if (PlayerList.Contains(o.Pid))
                        continue;
                    var bc = o;
                    this.AddPlayerToList(bc.Pid);
                    GameObject newPlayerGameObject = Instantiate(Resources.Load<GameObject>("16_2"));
                    var aiController = newPlayerGameObject.GetComponent<AIController>();
                    aiController.InitPlayer(bc.Pid, bc.Username, bc.P.X, bc.P.Y, bc.P.Z, bc.P.V);
                    aiController.OnUserDestroy += OnUserDestroy;
                }
                NewPlayerList.Clear();
            }
        }
    }

    private void OnUserDestroy(int playerId)
    {
        //将当前用户的pid从列表中清除
        lock (this.PlayerList)
        {
            this.PlayerList.Remove(playerId);
        }
    }
}
