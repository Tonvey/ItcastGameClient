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
    public bool ParseCMD = false;
    private CmdHelper mCmdHelper = new CmdHelper();
    private Action NextFrameAction = null;
    private Position InitP;
    private static GameController _instance = null;
    private bool hasInit = false;
    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameController();
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        if (!hasInit)
        {
            Debug.Log("GameController start");
            GameEventManager.OnLogon += OnLogon;
            GameEventManager.OnChangeWorldResponse += OnChangeWorldResponse;
            if (ParseCMD)
            {
                mCmdHelper.ParseCMDArgs();
                this.ServerIP = mCmdHelper.ServerIP;
                this.ServerPort = ushort.Parse(mCmdHelper.ServerPort);
            }
            NetworkController.Instance.ConnectToServer(this.ServerIP, this.ServerPort);
            hasInit = true;
        }
    }


    private void OnLogon(int id, string name)
    {
        this.PlayerID = id;
        this.PlayerName = name;
    }

    internal void RequestChangeScene(int targetSceneID)
    {
        Debug.Log("Request Scene change");
        Pb.ChangeWorldRequest req = new ChangeWorldRequest();
        req.Pid = this.PlayerID;
        req.SrcId = SceneController.CurrentSceneController.SceneID;
        req.TargetId = targetSceneID;
        NetworkController.Instance.SendMessage(NetworkController.Protocol.GAME_MSG_CHANGE_WORLD, req);
    }
    public void Update()
    {
        try
        {
            NetworkController.Instance.ProcessMessages();
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }

        if (NextFrameAction != null)
        {
            Debug.Log("Run NextFrameAction");
            NextFrameAction();
            NextFrameAction = null;
            BroadCast bc = new BroadCast();
            bc.P = this.InitP;
            bc.Pid = this.PlayerID;
            bc.Username = this.PlayerName;
            bc.Tp = 2;
            GameEventManager.OnMove(bc);
        }
    }

    private void OnChangeWorldResponse(ChangeWorldResponse res)
    {
        if (res.ChangeRes == 1)
        {
            NetworkController.Instance.PauseProcMessage = true;
            this.InitP = res.P;

            string sceneName = "WorldScene";
            if (res.TargetId == 1)
            {
                sceneName = "WorldScene";
            }
            if (res.TargetId == 2)
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
                    var sc = GameObject.Find("SceneController");
                    var bsc = sc.GetComponent<BattleSceneController>();
                    bsc.AddPlayerToList(this.PlayerID);
                    NetworkController.Instance.PauseProcMessage = false;
                };
            };
        }
    }
    public void OnDestroy()
    {
        NetworkController.Instance.Disconnect();
    }
    ~GameController()
    { 
        NetworkController.Instance.Disconnect();
    }
}
