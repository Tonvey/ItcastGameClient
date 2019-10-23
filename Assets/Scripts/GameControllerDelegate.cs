using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerDelegate : MonoBehaviour
{
    public string ServerIP = "";
    public ushort ServerPort = 0;
    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.ServerIP = this.ServerIP;
        GameController.Instance.ServerPort = this.ServerPort;
        GameController.Instance.Start();
    }

    // Update is called once per frame
    void Update()
    {
        GameController.Instance.Update();        
    }
    private void OnDestroy()
    {
        //GameController.Instance.OnDestroy();
    }
}
