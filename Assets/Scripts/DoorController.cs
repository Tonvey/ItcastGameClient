using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float rotateSpeed = 60.0f;
    public int TargetSceneID = 2;
    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(new Vector3(0, Time.deltaTime * this.rotateSpeed, 0));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collistion Enter");
        if (collision.collider.tag=="Player")
        {
            GameController.Instance.RequestChangeScene(TargetSceneID);
        }
        //SceneManager.LoadScene("BattleScene");
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Collistion Stay");
    }
    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collistion Exit");
    }
    private void OnDestroy()
    {
        Debug.Log("Door destroyed");
    }
    ~DoorController()
    {
        Debug.Log("Door destructor");
    }
}
