using System;
using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private Queue<Action> actionsNextFrame = new Queue<Action>();
    public event Action<int> OnUserDestroy;
    //当前角色的移动速度
    public float speed = 5.0f;
    public int Pid;
    private CharacterController aiCharaterController;
    private Animator animator;
    public GameObject bolt;
    public float boltSpeed = 1f;
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
            this.actionsNextFrame.Enqueue(() =>
            {
                //_playerNameChanged = true;
                var uiController = GetComponent<PlayerInformationUIController>();
                if (uiController != null)
                {
                    uiController.textName.text = _playerName;
                }
                else
                {
                    Debug.Log("UiController is null");
                }
            });
        }
    }
    private int _hp = 0;
    private bool _hpChanged = false;
    public int HP
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
            //_hpChanged = true;
            this.actionsNextFrame.Enqueue(() =>
            {
                var uiController = GetComponent<PlayerInformationUIController>();
                if (uiController != null)
                {
                    Debug.Log("Hp :" + this._hp);
                    uiController.hpBar.value = this._hp / 1000f;
                }
                else
                {
                    Debug.Log("UiController is null");
                }
            });
        }
    }
    internal void InitPlayer(int pid, string username, float x, float y, float z, float v,int hp)
    {
        Debug.Log("AiController init");
        this.Pid = pid;
        this.PlayerName = username;
        this.transform.position = new Vector3(x, y, z);
        this.transform.rotation = Quaternion.Euler(0,v,0);
        this.HP = hp;
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AiController start");
        GameEventManager.OnMove += OnMove;//监听玩家移动的事件 
        GameEventManager.OnOver += OnOver;//监听玩家离线的事件
        GameEventManager.OnSkillTrigger += OnSkillTrigger;
        GameEventManager.OnSkillContact += OnSkillContact;
        aiCharaterController = this.GetComponent<CharacterController>();
        animator = this.GetComponent<Animator>();
    }

    private void OnSkillContact(SkillContact contact)
    {
        if(contact.TargetPid == this.Pid)
        {
            Debug.LogFormat("AI:{0} be hit , hp : {1}", this.Pid, contact.ContactPos.BloodValue);
            this.HP = contact.ContactPos.BloodValue;
        }
    }
    private void OnOver(int _pid)
    {
        Debug.Log("AiController OnOver");
        //干掉当前玩家
        Destroy(this.gameObject);
    }
    private void OnSkillTrigger(SkillTrigger trigger)
    {
        if(trigger.Pid!=this.Pid)
        {
            return;
		}
        this.animator.SetTrigger("Attack");
        Vector3 pos = this.transform.position + this.transform.up + this.transform.forward * 3.5f;
        var bulletObj = Instantiate(bolt, pos, this.bolt.transform.rotation);
        bulletObj.transform.rotation = Quaternion.AngleAxis(90, this.transform.right) * this.transform.rotation;
        var bc = bulletObj.GetComponent<BoltContoller>();
        bc.BornPos = pos;
        bc.PlayerId = this.Pid;
        bc.BulletId = trigger.BulletId;
        bc.SkillId = trigger.SkillId;
        var rgbody = bulletObj.GetComponent<Rigidbody>();
        rgbody.velocity = new Vector3(trigger.V.X, trigger.V.Y, trigger.V.Z);
    }

    private void OnMove(BroadCast bc)
    {
        if(bc.Pid == this.Pid)
        {
            this.HP = bc.P.BloodValue;
            //坐标改变了
            this.transform.rotation = Quaternion.Euler(0, bc.P.V, 0);
            //先判断一下目的地离当前位置的距离,如果很大,那么就使用协程每一帧都自动生成一个平滑的移动
            Vector3 destPos = new Vector3(bc.P.X, bc.P.Y, bc.P.Z);
            if (Vector3.Distance(this.transform.position, destPos) > 0.3f)
            {
                //每次启动协程之前先停止一下上一次的协程
                StopCoroutine(MoveLogic(destPos));
                StartCoroutine(MoveLogic(destPos));
            }
            else
            {
                //如果距离太短,那就直接闪现过去
                this.transform.position = destPos;
            }
        }
    }

    IEnumerator MoveLogic(Vector3 _newPos)
    {
        //只要协程启动,每一帧都会迭代一下
        Vector3 destPos = new Vector3(_newPos.x, _newPos.y, _newPos.z);
        while (Vector3.Distance(this.transform.position, destPos)>0.3f)
        {
            //处理移动一点点距离
            //计算速度向量
            Vector3 dirVector = destPos - this.transform.position;
            //获取方向向量的单位向量,然后乘以速度,就可以得到一个速度的向量
            Vector3 speedVect = dirVector.normalized * this.speed;
            aiCharaterController.SimpleMove(speedVect);
            yield return 0; 
        }
    }
    // Update is called once per frame
    void Update()
    {
        while(actionsNextFrame.Count>0)
        {
            var act = actionsNextFrame.Dequeue();
            act();
        }
    }
    private void OnDestroy()
    {
        Debug.Log("AiController OnDestroy");
        if(OnUserDestroy!=null)
            OnUserDestroy(Pid);
    }
}
