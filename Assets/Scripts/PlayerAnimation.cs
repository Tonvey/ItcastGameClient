using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator playerAnimator; // 角色骨骼动画
    private float m_AnimatorSpeed;
    private float m_AnimatorDirection;
    public Vector3 m_LastPostion;

    // Start is called before the first frame update
    void Start()
    {
        //自动获取当前角色的动画控制器 
        this.playerAnimator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //播放角色动画
        PlayAnimation();
    }
    private void PlayAnimation()
    {
        //重新计算移动的向量
        Vector3 vMovement = this.transform.position - m_LastPostion;
        float speed = Vector3.Dot(vMovement.normalized, this.transform.forward);
        float direction = Vector3.Dot(vMovement.normalized, this.transform.right);

        //修正左右方向上没有动画的问题,速度很小,左右方向值很大
        if (Mathf.Abs(speed) <= 0.1f && Mathf.Abs(direction) >= 0.9f)
            speed = 1f;
        //修正一下1,3, 7,9 方向碎步
        if (speed < 0 && speed > -0.9f && direction > 0.2f)
        {
            speed = -1f;
            direction = 1f;
        }
        if (speed < 0 && speed > -0.9f && direction < -0.2f)
        {
            speed = -1f;
            direction = -1f;
        }
        m_AnimatorSpeed = Mathf.MoveTowards(m_AnimatorSpeed, speed, 3 * Time.deltaTime);
        m_AnimatorDirection = Mathf.MoveTowards(m_AnimatorDirection, direction, 3 * Time.deltaTime);

        //速度
        playerAnimator.SetFloat("Speed", m_AnimatorSpeed);
        //方向
        playerAnimator.SetFloat("Direction", m_AnimatorDirection);
        m_LastPostion = this.transform.position;
    }

}
