using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CmdHelper
{
    public string ServerIP = "127.0.0.1";
    public string ServerPort = "32768";
    // Start is called before the first frame update
    public void ParseCMDArgs()
    {
        //获取命令行的参数进行解析 
        string[] args = Environment.GetCommandLineArgs();
        for(int i = 0; i<args.Length; ++i)
        {
            //如果使用unity 直接运行程序会发现有很多额外的参数
            if(args[i]=="-h")
            {
                //下一个参数就是我们要的ip地址
                if(args.Length>i+1)
                {
                    ServerIP = args[i + 1];
                }
                else
                {
                    //报错
                    Debug.LogError("Not enough args");
                }
            }
            if(args[i]=="-p")
            {
                //下一个参数就是我们要的端口号
                if(args.Length>i+1)
                {
                    ServerPort = args[i + 1];
                }
                else
                {
                    //报错
                    Debug.LogError("Not enough args");
                }
            }
        }
    }
}
