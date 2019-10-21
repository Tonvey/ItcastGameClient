using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public static SceneController CurrentSceneController = null;
    public bool PlayerCanAttack = false;
    public int SceneID;
    virtual protected void Awake()
    {
        Debug.Log("SceneController awake");
    }
    // Start is called before the first frame update
    virtual protected void Start()
    {
        Debug.Log("SceneController start");
        CurrentSceneController = this;
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        
    }
}
