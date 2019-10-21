using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;
using System.IO;
public class PlayerController : MonoBehaviour
{
    public Animator playerAnimator; // 角色骨骼动画
    public GameObject bolt;
    public float boltSpeed = 1f;
    public float speed; //运动速度
    public CharacterController playerCharacterConctl; //动画控制器
    public float roteSpeed;//旋转速度
    //装载机器人本身的位置角度大小等信息
    public Transform playerTransform;
    //储存机器人移动的距离
    private Vector3 m_CurrentMovement;

    private float _lastSyncPositionTimeSpec=0f;
    private Vector4 _lastSyncPosition = Vector4.zero;
    //是否已经获取出生点位置,0表示未获取,1表示已经获取但是未同步到模型上,2表示已经同步到模型上
    private int _initPosState = 0;
    //四元组存储 玩家的xyz坐标以及面向的角度
    private Vector4 InitPos;

    private bool canAttack = true;


    private float m_LastMouseX;
    private int _pid=0;
    public int Pid
    {
        get
        {
            return _pid;
        }
        set
        {
            _pid = value;
            Debug.Log("Get New Pid :" + _pid);
        }
    }
    private string _playerName;
    private bool _playerNameChanged = false;
    public string PlayerName
    {
        get
        {
            return _playerName;
        }
        set
        {
            this._playerName = value;
            _playerNameChanged = true;
        }
    }
    public event Action<Vector4> OnPlayerMove;

    void MoveAction()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if(Mathf.Abs( moveHorizontal)>0.0f|| Mathf.Abs(moveVertical)>0.0f)
        {
            this.transform.eulerAngles = new Vector3(transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, transform.localEulerAngles.z);
            Vector3 velocity = (transform.forward * moveVertical + transform.right * moveHorizontal) * speed;
            playerCharacterConctl.SimpleMove(velocity);
        }
    }
  
    IEnumerator WaitAndRun(float seconds,Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
    private void AttackAction()
    {
        if (!canAttack)
            return;
        canAttack = false;
        StartCoroutine(WaitAndRun(1,
            new Action(() =>
        {
            canAttack = true;
        })));
        this.playerAnimator.SetTrigger("Attack");
        Vector3 pos = this.transform.position + this.transform.up + this.transform.forward*3.5f;
        var bulletObj = Instantiate(bolt, pos, this.bolt.transform.rotation);
        bulletObj.transform.rotation = Quaternion.AngleAxis(90, this.transform.right) * this.transform.rotation;
        var bc = bulletObj.GetComponent<BoltContoller>();
        bc.BornPos = pos;
        var rgbody = bulletObj.GetComponent<Rigidbody>();
        rgbody.velocity = this.transform.forward * this.boltSpeed;

        SkillTrigger fire = new SkillTrigger();
        fire.Pid = this.Pid;
        fire.SkillId = 1;
        fire.P.X = bulletObj.transform.position.x;
        fire.P.Y = bulletObj.transform.position.y;
        fire.P.Z = bulletObj.transform.position.z;
        fire.P.V = bulletObj.transform.localEulerAngles.y;
        //fire.

    }
    // Start is called before the first frame update
    void Start()
    {
        //关注NetMgr的一些网络事件
        NetManager.OnLogon += OnLogon;
        NetManager.OnMove += OnMove;
    }

    private void OnLogon(int pid, string userName)
    {
        this.PlayerName = userName;
        this.Pid = pid;
    }

    private void OnMove(BroadCast obj)
    {
        //完成当前玩家的移动
        //因为当前玩家是这个世界的主角
        //只接受第一次的报文,将玩家放到出生点
        if(_initPosState==0)
        {
            this.InitPos = new Vector4(obj.P.X, obj.P.Y, obj.P.Z, obj.P.V);
            _initPosState = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_initPosState==1)//表示已经获取出生点,但是未同步到模型上
        {
            //已经获取了初始化位置
            //将当前玩家移动到出生点
            this.transform.position = new Vector3(InitPos.x, InitPos.y, InitPos.z);
            //还有旋转的角度,单纯的做Y轴的旋转
            this.transform.rotation = Quaternion.Euler(0, InitPos.w, 0);
            _initPosState = 2;
        }
        if(_playerNameChanged)
        {
            //如果不是在主线程里边去操作UI,会出一些问题
            //获取脚本PlayerInformationUIController脚本的对象,对Text UI控件进行赋值
            var uiController = GetComponent<PlayerInformationUIController>();
            if(uiController!=null)
            {
                uiController.textName.text = _playerName;
                _playerNameChanged = false;
            }
            else
            {
                Debug.Log("UiController is null");
            }
        }
        MoveAction();
        MouseEvent();
    }

    private void MouseEvent()
    {
        if(SceneController.CurrentSceneController!=null&&SceneController.CurrentSceneController.PlayerCanAttack)
        {
            if (Input.GetButton("Fire1"))//1代表右键
            {
                //InputGameData.Instance.AttackTrigger = true;
                AttackAction();
            }

        }
    }


    private void FixedUpdate()
    {
        //两次发送位置同步的事件间隔必须大于0.1秒
        if(_lastSyncPositionTimeSpec>0.2f) 
        {
            _lastSyncPositionTimeSpec = 0f;
            Vector4 pos = new Vector4(
                transform.position.x,
                transform.position.y,
                transform.position.z,
                transform.localEulerAngles.y
                );
            if(!pos.Equals(_lastSyncPosition))
            {
                SendPosition();
                _lastSyncPosition = pos;
            }
        }
        else
        {
            this._lastSyncPositionTimeSpec += Time.deltaTime;
        }
    }

    private void SendPosition()
    {
        Pb.Position pos = new Pb.Position();
        pos.X = this.transform.position.x;
        pos.Y = this.transform.position.y;
        pos.Z = this.transform.position.z;
        pos.V = this.transform.localEulerAngles.y; //当前玩家的面朝方向应该获取欧拉角度的y轴旋转角度
        NetManager.Instance.SendMessage(3,pos);
    }
    private void OnDestroy()
    {

    }
}
