using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    //跟随目标
    public GameObject follow; 
    public float followDistance=4.5f;
    public float followOffsetX = 0.0f;
    public float followOffsetY = 1.5f;
    public float followOffsetZ = 0.0f;

    public float horizontalSpeed = 1.0f;
    public float verticalSpeed = 1.0f;
    //是否垂直方向反转
    public bool reverseVertical = true;
    //最大可视角度
    public float MaxVerticalViewAngle = 80f;

    private float angleHorizontal = 0.0f;
    private float angleVertical = 0.0f;

    private Vector3 mLastMousePosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        mLastMousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        angleHorizontal += h;
        if(reverseVertical)
        {
            angleVertical -= v;
        }
        else
        {
            angleVertical += v;
        }
        if (angleVertical < -MaxVerticalViewAngle)
        {
            angleVertical = -MaxVerticalViewAngle;
        }
        if (angleVertical > MaxVerticalViewAngle)
        {
            angleVertical = MaxVerticalViewAngle;
        }
        transform.rotation = Quaternion.Euler(new Vector3(angleVertical, angleHorizontal, 0));

        this.transform.position = follow.transform.position + new Vector3(followOffsetX,followOffsetY,followOffsetZ) - transform.forward * followDistance;

    }
}
