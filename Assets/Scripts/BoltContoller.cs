using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltContoller : MonoBehaviour
{
    public float aliveDistance=100f;
    private Rigidbody rgbody;
    private Vector3 bornPos;
    public int BulletId;
    public int SkillId;
    public int PlayerId;
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
        if(collision.collider.tag=="Player")
        {
            var pc = collision.collider.GetComponent<PlayerController>();
            if(PlayerId==pc.Pid)
            {
                return;
            }
        }
        else if(collision.collider.tag=="OtherPlayer")
        {
            var ai = collision.collider.GetComponent<AIController>();
            if (ai != null)
            {
                if(ai.Pid==this.PlayerId)
                {
                    return;
                }
                Pb.SkillContact contact = new Pb.SkillContact();
                contact.BulletId = this.BulletId;
                contact.SkillId = this.SkillId;
                contact.SrcPid = this.PlayerId;
                contact.TargetPid = ai.Pid;
                Pb.Position contacPos = new Pb.Position();
                contacPos.X = collision.contacts[0].point.x;
                contacPos.Y = collision.contacts[0].point.y;
                contacPos.Z = collision.contacts[0].point.z;
                contact.ContactPos = contacPos;
                NetworkController.Instance.SendMessage(NetworkController.Protocol.GAME_MSG_SKILL_CONTACT, contact);
            }
            else
            {
                Debug.Log("Can not get other player script");
            }
        }
        Destroy(this.gameObject);
    }
}
