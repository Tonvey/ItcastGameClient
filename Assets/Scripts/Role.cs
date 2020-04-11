using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Role : MonoBehaviour
{
    protected Queue<Action> actionsNextFrame = new Queue<Action>();
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
    private int _hp = 0;
    public int HP
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
            actionsNextFrame.Enqueue(() =>
            {
                var uiController = GetComponent<PlayerInformationUIController>();
                if (uiController != null)
                {
                    Debug.Log("Hp :" + this._hp);
                    uiController.hpBar.value = this._hp / 1000f;
                }
                else
                {
                    Debug.LogError("UiController is null");
                }
            });
        }
    }
    private string _playerName;
    public string PlayerName
    {
        get
        {
            return _playerName;
        }
        set
        {
            this._playerName = value;
            actionsNextFrame.Enqueue(() =>
            {
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
    // Start is called before the first frame update
    public virtual void Start()
    {
        var uiInfo = this.gameObject.AddComponent<PlayerInformationUIController>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        while (actionsNextFrame.Count > 0)
        {
            try
            {
                var act = actionsNextFrame.Dequeue();
                act();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
