using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController
{
    public string ServerIP = "";
    public ushort ServerPort = 0;
    public int PlayerID;
    public string PlayerName;
    private int ChangingScene = 0;
    private int ChangToSceneInNextFrame = 0;
    public bool ParseCMD = false;
    private CmdHelper mCmdHelper = new CmdHelper();
    private Action NextFrameAction=null;
    private Position InitP;
    private static GameController _instance = null;
    private bool hasStart = false;
    public static GameController Instance
    {
        get
        {
            if(_instance==null)
            {
                _instance = new GameController();
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        if(!hasStart)
        {
            Debug.Log("GameController start");
            NetManager.OnLogon += OnLogon;
            NetManager.OnChangeWorldResponse += OnChangeWorldResponse;
            if (ParseCMD)
            {
                mCmdHelper.ParseCMDArgs();
                this.ServerIP = mCmdHelper.ServerIP;
                this.ServerPort = ushort.Parse(mCmdHelper.ServerPort);
            }
            NetManager.Instance.ConnectToServer(this.ServerIP, this.ServerPort);
            hasStart = true;
        }
    }


    private void OnLogon(int id, string name)
    {
        this.PlayerID = id;
        this.PlayerName = name;
    }

    internal void RequestChangeScene(int targetSceneID)
    {
        //if(this.ChangeScene==0)
        //{
        //    Debug.Log("Waiting for server response, please wait and try again");
        //    return;
        //}
        Debug.Log("Request Scene change");
        Pb.ChangeWorldRequest req = new ChangeWorldRequest();
        req.Pid = this.PlayerID;
        req.SrcId = SceneController.CurrentSceneController.SceneID;
        req.TargetId = targetSceneID;
        NetManager.Instance.SendMessage(NetManager.Protocol.GAME_MSG_CHANGE_WORLD,req);
    }
    public void Update()
    {
        if(this.ChangToSceneInNextFrame!=0)
        {
            this.ChangingScene = 0;
            string sceneName = "WorldScene";
            if (ChangToSceneInNextFrame == 1)
            {
                sceneName = "WorldScene";
            }
            if (ChangToSceneInNextFrame == 2)
            {
                sceneName = "BattleScene";
            }
            //SceneManager.LoadScene("BattleScene");
            var aop = SceneManager.LoadSceneAsync(sceneName);
            aop.completed += (obj) =>
              {
                  this.NextFrameAction = () =>
                  {
                      Debug.Log("Scene changed");
                      var player = GameObject.Find("16_1");
                      var pc = player.GetComponent<PlayerController>();
                      pc.Pid = this.PlayerID;
                      pc.PlayerName = this.PlayerName;
                      ChangToSceneInNextFrame = 0;
                      var sc = GameObject.Find("SceneController");
                      var bsc = sc.GetComponent<BattleSceneController>();
                      bsc.AddPlayerToList(this.PlayerID);
                      NetManager.Instance.Client.Pause = false;
                  };
              };
        }
        if(NextFrameAction!=null)
        {
            Debug.Log("Run NextFrameAction");
            NextFrameAction();
            NextFrameAction = null;
            BroadCast bc = new BroadCast();
            bc.P = this.InitP;
            bc.Pid = this.PlayerID;
            bc.Username = this.PlayerName;
            bc.Tp = 2;
            NetManager.OnMove(bc);
        }
    }

    private void OnChangeWorldResponse(ChangeWorldResponse res)
    {
        if(res.ChangeRes==1)
        {
            this.ChangToSceneInNextFrame = res.TargetId;
            NetManager.Instance.Client.Pause = true;
            this.InitP = res.P;
        }
    }
    public void OnDestroy()
    {
        NetManager.Instance.Disconnect();
    }
}
