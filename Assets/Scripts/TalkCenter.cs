using UnityEngine;
using System.Collections;
//一些命名空间
using UnityEngine.UI;
using System;
using Pb;
using System.IO;
using Google.Protobuf;
public class TalkCenter : MonoBehaviour
{
    //装结构视图中的TalkContent里Text组件
    public Text mTalkText;//要把结构视图的游戏对象拖到脚本中进行赋值
    //装结构视图中的InputField
    public InputField mInputField;//要把结构视图的游戏对象拖到脚本中进行赋值
    //是否有服务器发送的聊天信息
    public static bool TalkFlag = false;
    //接收服务器发送过来的聊天内容
    public static string Content;
    //谁发送过来的信息
    public static string PlayerName;
    //是否在输入信息
    public static bool Talking = false;
    //显示内容的最大条数
    public int ContentSize = 10;
    //显示的内容
    private string mContent = "";
    //是否有信息发出的标志位，true表示有
    private bool CanTalk = true;
    private NetworkController netMgr;
    //队列容器
    private Queue mContents = new Queue();
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //TODO:有bug，如果一帧时间内来了多条消息怎么办
        if (mTalkText && TalkFlag)
        {
            TalkFlag = false;
            CanTalk = true;
            //组合要显示的内容
            mContent = PlayerName + ": ";
            mContent += Content;
            mContent += "\n";
            Debug.Log("mContent=" + mContent);
            if (mContents.Count >= ContentSize)
            {
                //出队
                mContents.Dequeue();
            }
            else
            {
                //内容入队列
                mContents.Enqueue(mContent);
            }
            //组合成string
            string str = "";
            foreach (var content in mContents)
            {
                str += content;
            }

            //显示出来
            mTalkText.text = str;
        }
    }
    //发送按键的回调函数
    public void OnClickSendButton()
    {
        //如果没有信息就返回
        if (!CanTalk)
            return;
        ByteBuffer b = new ByteBuffer();
        //创建一个protobuf的消息结构
        Talk talk = new Talk();
        //获取输入框的内容
        talk.Content = mInputField.text;
        netMgr.SendMessage(2, talk);
        CanTalk = false;
    }
    //进行输入不能进行移动等操作
    public void SetTalking()//属性视图中调用
    {
        Talking = true;
    }
    //解除相应阻截
    public void SetNotTalking()//属性视图中调用
    {
        Talking = false;
    }
}
