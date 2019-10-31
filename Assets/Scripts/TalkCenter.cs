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
    private bool mHasNewMessage = false;
    //显示内容的最大条数
    public int MaxDisplayMessageCount = 10;

    //队列容器
    private Queue mContents = new Queue();


    public void MessageEnqueue(int pid , string playerName, string message)
    {
        Debug.Log("llalalalal");
        mHasNewMessage = true;
        string content = playerName + ": ";
        content += message;
        content += "\n";
        mContents.Enqueue(content);
        while(mContents.Count>MaxDisplayMessageCount)
        {
            mContents.Dequeue();
        }
    }

    // Use this for initialization
    void Start()
    {
        GameEventManager.OnNewChatMessage += OnNewChatMessage;
    }
    private void OnDestroy()
    {
        GameEventManager.OnNewChatMessage -= OnNewChatMessage;
    }

    private void OnNewChatMessage(int pid, string userName, string message)
    {
        MessageEnqueue(pid, userName, message);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO:有bug，如果一帧时间内来了多条消息怎么办
        if (mHasNewMessage)
        {
            mHasNewMessage = false;
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
        ByteBuffer b = new ByteBuffer();
        //创建一个protobuf的消息结构
        Talk talk = new Talk();
        //获取输入框的内容
        talk.Content = mInputField.text;
        NetworkController.Instance.SendMessage(NetworkController.Protocol.GAME_MSG_TALK_CONTENT, talk);
    }
    //进行输入不能进行移动等操作
    public void SetTalking()//属性视图中调用
    {
        GameEventManager.OnChatting(true);
    }
    //解除相应阻截
    public void SetNotTalking()//属性视图中调用
    {
        GameEventManager.OnChatting(false);
    }
}
