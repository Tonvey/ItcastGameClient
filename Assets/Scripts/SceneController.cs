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
    virtual protected void Awake()
    {
        Debug.Log("SceneController awake");
    }
    // Start is called before the first frame update
    virtual protected void Start()
    {
        Debug.Log("SceneController start");
        CurrentSceneController = this;
        GameEventManager.OnNewPlayers += OnNewPlayers;
        GameEventManager.OnNewPlayer += OnNewPlayer;
        GameEventManager.OnLogon += OnLogon;
    }
    private void OnLogon(int pid, string name)
    {
        Debug.Log(string.Format("User logon , pid : {0}, name : {1}", pid, name));
        this.AddPlayerToList(pid);
    }


    // Update is called once per frame
    virtual protected void Update()
    {
    }
    private void AddOnePlayerToScene(Player player)
    {
        if(PlayerList.Contains(player.Pid))
        {
            Debug.LogFormat("Player exists {0} {1}", player.Pid, player.Username);
            return;
        }
        AddPlayerToList(player.Pid);
        GameObject newPlayerGameObject = Instantiate(Resources.Load<GameObject>("16_2"));
        var aiController = newPlayerGameObject.GetComponent<AIController>();
        aiController.InitPlayer(player.Pid, player.Username, player.P.X, player.P.Y, player.P.Z, player.P.V, player.P.BloodValue);
        aiController.OnUserDestroy += OnUserDestroy;
    }
    private void OnNewPlayers(List<Player> l)
    {
        foreach (var player in l)
        {
            AddOnePlayerToScene(player);
        }
    }
    public void AddPlayerToList(int pid)
    {
        if (!PlayerList.Contains(pid))
        {
            this.PlayerList.Add(pid);
        }
    }
    public void RemovePlayerFromList(int pid)
    {
        this.PlayerList.Remove(pid);
    }

    private void OnNewPlayer(BroadCast bc)
    {
        Player player = new Player();
        player.P = bc.P;
        player.Pid = bc.Pid;
        player.Username = bc.Username;
        this.AddOnePlayerToScene(player);
    }
    private void OnUserDestroy(int playerId)
    {
        //将当前用户的pid从列表中清除
        this.PlayerList.Remove(playerId);
    }
}
