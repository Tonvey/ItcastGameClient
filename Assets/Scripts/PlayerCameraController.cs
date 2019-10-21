using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public GameObject follow;  //摄像机跟随的对象,在视图上赋值了

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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
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
