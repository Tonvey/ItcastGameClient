using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public GameObject follow;  //摄像机跟随的对象,在视图上赋值了

    public bool MouseViewControlUseAxes = true;
    public float followDistance=4.5f;
    public float followOffsetX = 0.0f;
    public float followOffsetY = 1.5f;
    public float followOffsetZ = 0.0f;

    public float horizontalSpeed = 1.0f;
    public float verticalSpeed = 1.0f;
    public bool reverseVertical = true;
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

    }

    private void LateUpdate()
    {
        float h,v;
        if(MouseViewControlUseAxes)
        {
            h = horizontalSpeed * Input.GetAxis("Mouse X");
            v = verticalSpeed * Input.GetAxis("Mouse Y");
        }
        else
        {
            Vector3 diff = Input.mousePosition - mLastMousePosition;
            mLastMousePosition = Input.mousePosition;
            h = diff.x * horizontalSpeed * 0.1f;
            v = diff.y * verticalSpeed * 0.1f;
        }
        //Debug.Log("Get h : " + h + " v : " + v);
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
        //Debug.Log("angleVertical:" + angleVertical);

        transform.rotation = Quaternion.Euler(new Vector3(angleVertical, angleHorizontal, 0));

        this.transform.position = follow.transform.position + new Vector3(followOffsetX,followOffsetY,followOffsetZ) - transform.forward * followDistance;

    }
}
