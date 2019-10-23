using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;

public class SceneController : MonoBehaviour
{
    public List<int> PlayerList = new List<int>();   //ArrayList存储的都是object,什么类型的东西都可以存储
    public List<Pb.BroadCast> NewPlayerList = new List<Pb.BroadCast>();   //在下一帧要创建的玩家的列表

    public static SceneController CurrentSceneController = null;
    public bool PlayerCanAttack = false;
    public int SceneID;
    private int frameCount = 10;
    virtual protected void Awake()
    {
        Debug.Log("SceneController awake");
    }
    // Start is called before the first frame update
    virtual protected void Start()
    {
        Debug.Log("SceneController start");
        CurrentSceneController = this;
        NetManager.OnNewPlayers += OnNewPlayers;
        NetManager.OnNewPlayer += OnNewPlayer;
        NetManager.OnLogon += OnLogon;
    }
    private void OnLogon(int pid, string name)
    {
        Debug.Log(string.Format("User logon , pid : {0}, name : {1}", pid, name));
        this.AddPlayerToList(pid);
    }


    // Update is called once per frame
    virtual protected void Update()
    {
        try
        {
            //if (frameCount > 0)
            //{
            //    Debug.Log("SceneController Skip frame");
            //    frameCount--;
            //    return;
            //}
            if (NewPlayerList.Count > 0)
            {
                lock (NewPlayerList)
                {
                    Debug.Log("Before create new player , player count : " + NewPlayerList.Count);
                    foreach (var o in NewPlayerList)
                    {
                        Debug.Log(string.Format("New Player id : {0} , name : {1} , {2}", o.Pid, o.Username, o.P));
                        if (PlayerList.Contains(o.Pid))
                        {
                            Debug.Log(string.Format("New Player id : {0} , name : {1} , {2}  exists", o.Pid, o.Username, o.P));
                            continue;
                        }
                        var bc = o;
                        this.AddPlayerToList(bc.Pid);
                        GameObject newPlayerGameObject = Instantiate(Resources.Load<GameObject>("16_2"));
                        var aiController = newPlayerGameObject.GetComponent<AIController>();
                        aiController.InitPlayer(bc.Pid, bc.Username, bc.P.X, bc.P.Y, bc.P.Z, bc.P.V, bc.P.BloodValue);
                        aiController.OnUserDestroy += OnUserDestroy;
                    }
                    NewPlayerList.Clear();
                }
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private void OnNewPlayers(List<Player> l)
    {
        Debug.LogFormat("class : {0}  , OnNewPlayers", this.GetType().ToString());
        //要在主线程取创建一个玩家对象
        lock (NewPlayerList)
        {
            foreach (var player in l)
            {
                Pb.BroadCast bc = new Pb.BroadCast();
                bc.Username = player.Username;
                bc.Pid = player.Pid;
                bc.P = player.P;
                NewPlayerList.Add(bc);
            }
        }
    }
    public void AddPlayerToList(int pid)
    {
        lock (this.PlayerList)
        {
            if (!PlayerList.Contains(pid))
            {
                this.PlayerList.Add(pid);
            }
        }
    }
    public void RemovePlayerFromList(int pid)
    {
        lock (this.PlayerList)
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
    private void OnUserDestroy(int playerId)
    {
        //将当前用户的pid从列表中清除
        lock (this.PlayerList)
        {
            this.PlayerList.Remove(playerId);
        }
    }
}
