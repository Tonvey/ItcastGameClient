using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInformationUIController : MonoBehaviour
{
    //该类用于控制UI的显示
    public GameObject textNameGameObject;
    public Text textName;
    public GameObject hpBarGameObject;
    public Slider hpBar;
    private GameObject sceneCanvas;//场景的画布
    private Camera mainCamera;
    //姓名的text ui 的偏移量
    public float textNameOffsetX = 0f;
    public float textNameOffsetY = 3.0f;
    public float hpBarOffsetX = 0f;
    public float hpBarOffsetY = 2.5f;
    public float standardDistance = 5.4f;

    private void OnDestroy()
    {
        //如果当前对象被干掉了,那么就将对应的文字以及血条干掉
        Destroy(this.textNameGameObject);        
        Destroy(this.hpBarGameObject);        
    }

    // Start is called before the first frame update
    void Start()
    {
        //当脚本启动的时候就加载预制体 
        textNameGameObject = Instantiate(Resources.Load<GameObject>("Name"));
        hpBarGameObject = Instantiate(Resources.Load<GameObject>("Slider"));
        sceneCanvas = GameObject.Find("Canvas");//在场景中查找叫做Canvas的对象
        if (sceneCanvas == null)
        {
            Debug.LogError("Cannot find canvas");
            return;
        }

        if(textNameGameObject!=null)
        {
            //加载成功,还要从游戏对象中提取Text UI
            textName = textNameGameObject.GetComponent<Text>();
            textName.text = "itcast";
            //当前text ui对象要跟画布建立父子关系
            textNameGameObject.transform.SetParent(sceneCanvas.transform);
        }
        else
        {
            Debug.LogError("Fail to load Name prefab");
        }
        if(hpBarGameObject!=null)
        {
            //加载成功,还要从游戏对象中提取Text UI
            hpBar = hpBarGameObject.GetComponent<Slider>();
            hpBar.value = 0.5f;
            //当前text ui对象要跟画布建立父子关系
            hpBarGameObject.transform.SetParent(sceneCanvas.transform);
        }
        else
        {
            Debug.LogError("Fail to load Slider prefab");
        }

        //获取主摄像机
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        UpdateUI(); 
    }

    private void UpdateUI()
    {
        //完成UI 文本的位置的控制,显示到角色的坐标
        //坐标转换,将当前模型世界坐标转化为屏幕的坐标
        Vector3 nameTextScreePos = mainCamera.WorldToScreenPoint(
            new Vector3(transform.position.x + textNameOffsetX, transform.position.y + textNameOffsetY, transform.position.z)
            );
        Vector3 hpBarScreePos = mainCamera.WorldToScreenPoint(
            new Vector3(transform.position.x + hpBarOffsetX, transform.position.y + hpBarOffsetY, transform.position.z)
            );
        float scale = 1f; ;
        if (nameTextScreePos.z > 0f)
        {
            //表示在摄像机前方
            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            //做保护,distance可能会等于0
            if (distance == 0f)
            {
                scale = 1f; 
            }
            else
            {
                scale = standardDistance / distance;
            }
            //控制一下大小不能太大,也不能太小
            if (scale > 1.0f)
            {
                scale = 1.0f;
            }
            if (scale < 0.2f)
            {
                scale = 0.2f;
            }
        }
        else
        {
            //表示在摄像机后方
            //不用显示
            scale = 0f;
        }
        this.textNameGameObject.transform.localScale = new Vector3(scale, scale, scale);
        this.textNameGameObject.transform.position = nameTextScreePos;
        this.hpBarGameObject.transform.position = hpBarScreePos;
        this.hpBarGameObject.transform.localScale = new Vector3(scale, scale, scale);
    }
}
