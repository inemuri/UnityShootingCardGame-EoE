using UnityEngine;

/*
 * 物体周期运动（主要用于内层body）
 */
public class SelfRotation : MonoBehaviour
{
    public float spd = 1;   // 速度
    public int type = 0;    // 类型

    void FixedUpdate()
    {
        switch (type)
        {
            case 1: // 上下运动
                float shift = Mathf.Sin(Time.time * 2) * spd;
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + shift, transform.localPosition.z);
                break;
            case 2: // 上下运动
                float shift2 = -Mathf.Sin(Time.time * 2) * spd;
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + shift2, transform.localPosition.z);
                break;
            case 5:
                transform.Rotate(new Vector3(10, 0, 0) * Time.deltaTime * spd);
                break;
            case 10:
                transform.Rotate(new Vector3(0, 0, 10) * Time.deltaTime * spd);
                break;
            default:
                transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime * spd);
                break;
        }
    }
}
