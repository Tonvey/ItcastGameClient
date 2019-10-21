using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltContoller : MonoBehaviour
{
    public float aliveDistance=100f;
    private Rigidbody rgbody;
    private Vector3 bornPos;
    public Vector3 BornPos
    {
        get
        {
            return bornPos;
        }
        set
        {
            this.bornPos = value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rgbody = this.GetComponent<Rigidbody>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(this.BornPos,this.transform.position)>aliveDistance)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.rigidbody.gameObject.tag);
        Destroy(this.gameObject);
        
    }
}
