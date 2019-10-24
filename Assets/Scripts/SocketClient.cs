using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using Pb;
using Google.Protobuf;
using System.Threading;


//主要处理Socket io的类
public class SocketClient
{
    //TcpClient对象
    private TcpClient client = null;
    //内存流
    private MemoryStream memStream;
    //转换为二进制类的对象
    private BinaryReader reader;
    //数组大小
    private const int MAX_READ = 8192;
    //数组，用来存储传输的信息
    private byte[] readBuff = new byte[MAX_READ];
    public bool LogIO = true;
    //是否连接服务器成功的标志位
    public static bool loggedIn = false;

    private string mIp;
    private int mPort;
    public Action<int, byte[]> OnNewProtocolMessage;

    public void ConnectToServer(string ip , int port)
    {
        Debug.Log("Scoket Client SendConnect : " + ip + ":" + port);
        mIp = ip;
        mPort = port;

        //在这里完成服务器的连接 
        //创建一个客户端实例
        client = null;
        client = new TcpClient();
        /*
         * SendTimeout 属性确定 Send 方法在成功返回之前阻塞的时间量。这一时间的度量单位为毫秒。
            调用 Write 方法以后，基础 Socket 返回实际发送到主机的字节数。SendTimeout 属性确定 
         * TcpClient 在接收返回的字节数之前所等待的时间量。如果超时在 Send 方法成功完成之
         * 前过期，TcpClient 将引发 SocketException。默认为无超时
         */
        client.SendTimeout = 1000;
        /*
         * ReceiveTimeout 属性确定 Read 方法在能够接收数据之前保持阻塞状态的时间量。
         * 这一时间的度量单位为毫秒。如果超时在 Read 成功完成之前到期，
         * TcpClient 将引发 SocketException。默认为无超时
         */
        client.ReceiveTimeout = 1000;
        /*
         * 当NoDelay是false、TcpClient不通过网络发送数据包，直到它收集大量的传出数据。
         */
        client.NoDelay = true;

        try
        {
            client.BeginConnect(ip, port, new AsyncCallback(OnConnect), null);
        }
        catch(Exception e)
        {
            Debug.LogError("Exception : " + e);
            Close();
        }

    }

    internal void Disconnect()
    {
        Debug.Log("SockClient disconnect");
        if(this.client.Connected)
        {
            this.Close();
        }
    }

    internal void SendMessage(int id, Google.Protobuf.IMessage message)
    {
        //将发送过来的消息封装为buffer
        MemoryStream ms = new MemoryStream();
        message.WriteTo(ms);
        byte[] messageBytes = ms.ToArray();

        ByteBuffer bb = new ByteBuffer();
        //主体消息的长度
        bb.WriteInt32(messageBytes.Length);
        bb.WriteInt32(id);
        bb.WriteBytes(messageBytes);
        Byte[] sendData = bb.ToArray();

        //数据的发送
        if (client.Connected)
        {
            if (LogIO)
            {
                string strBuff = "";
                foreach (var c in sendData)
                {
                    strBuff += c.ToString("X2") + " ";
                }
                Debug.Log("Send data:" + strBuff);
            }
            this.client.GetStream().BeginWrite(sendData, 0, sendData.Length, new AsyncCallback(OnWrite), sendData);
        }
        else
        {
            Debug.Log("socket client not been connected");
        }
    }

    private void OnWrite(IAsyncResult ar)
    {
        try
        {
            //写完数据之后
            this.client.GetStream().EndWrite(ar);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void Close()
    {
        if(client!=null)
        {
            if (client.Connected)
                client.Close();
            client = null;
        }
        //是否已经登录的标志,设置为false
        loggedIn = false;
    }

    private void OnConnect(IAsyncResult ar)
    {
        if(client.Connected)
        {
            Debug.Log("连接成功");
            client.GetStream().BeginRead(readBuff, 0, readBuff.Length, new AsyncCallback(OnRead), null);
        }
        else
        {
            Debug.LogError("连接失败");
        }
        client.EndConnect(ar);
    }


    //读取前面4个字节,是小端字节序的int32类型
    private int ReadInt32(byte []data,int offset)
    {
        int ret = 0;
        ret =
            data[0+offset] |
            data[1+offset] << 8 |
            data[2+offset] << 16 |
            data[3+offset] << 24;
        return ret;
    }

    private void OnRead(IAsyncResult ar)
    {
        try
        {
            //当有数据可以读取的时候就会回调这个函数,就在这个函数取处理数据 
            int readCount = client.GetStream().EndRead(ar); //读取到多少个字节,调用了这个函数之后才结束read
                                                            //获取透传参数,是上次读取数据遗留下来还没有处理的字节数组
            byte[] lastbuff = (byte[])ar.AsyncState;
            if (lastbuff == null)
            {
                //最后读取到的数据的buff
                lastbuff = new byte[readCount];
                //将读取缓冲区里边的数据先拷贝过来
                Array.Copy(readBuff, lastbuff, readCount);
            }
            else
            {
                //最后读取到的数据不为null,说明上次处理报文的时候还有数据遗留下来
                //要跟当前读到的数据进行拼凑,要对原来的buff数组进行扩容
                int copyPos = lastbuff.Length;
                Array.Resize<byte>(ref lastbuff, lastbuff.Length + readCount);
                Array.ConstrainedCopy(readBuff, 0, lastbuff, copyPos, readCount);
            }
            if (LogIO)
            {
                string strBuff = "";
                foreach (var c in lastbuff)
                {
                    strBuff += c.ToString("X2") + " ";
                }
                Debug.Log("Receiv data:" + strBuff);
            }

            OnReceivedData(lastbuff);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }

    }

    private void OnReceivedData(byte[] lastbuff)
    {
        //根据协议来进行判断
        while(lastbuff!=null&&lastbuff.Length>=8)
        {
            //前4个字节是主体数据的长度
            int packetLen = ReadInt32(lastbuff, 0);
            //中间4个字节是消息的id
            int messageId = ReadInt32(lastbuff, 4);

            //判断长度够不够
            if(lastbuff.Length>=8+packetLen)
            {
                //能够提取一个完整的报文
                byte[] packetData = new byte[packetLen];
                Array.ConstrainedCopy(lastbuff, 8, packetData, 0, packetLen);
                //报文分析
                OnMessage(messageId,packetData);

                //将前面的数据截取掉
                if(lastbuff.Length - 8 - packetLen>0)
                {
                    //创建一个临时数组保存后边的数据
                    byte[] tmp = new byte[lastbuff.Length - 8 - packetLen];
                    Array.ConstrainedCopy(lastbuff, 8 + packetLen, tmp, 0, lastbuff.Length - 8 - packetLen);
                    lastbuff = tmp;
                }
                else
                {
                    //如果后续没有跟其他数据,就置空
                    lastbuff = null;
                }
            }
            else
            {
                //报文不全,需要继续read,退出循环不要继续处理报文
                break;
            }
        }
        //退出循环之后还要继续read,此时要注意,透传一个lastbuff参数过来
        client.GetStream().BeginRead(readBuff, 0, readBuff.Length, new AsyncCallback(OnRead), lastbuff);
    }

    private void OnMessage(int messageId, byte[] packetData)
    {
        OnNewProtocolMessage(messageId, packetData);
    }
}
