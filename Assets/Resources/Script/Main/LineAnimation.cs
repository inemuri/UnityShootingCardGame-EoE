using UnityEngine;

/*
 * 自机子弹LineRenderer动画
 */
public class LineAnimation : MonoBehaviour
{

    private LineRenderer lr;    // 线渲染器
    private Material m;         // 材质
    private int type = 0;       // 类型

    public int Type
    {
        set
        {
            type = value;
        }
    }

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        m = GetComponent<Renderer>().material;
    }

    void Update()
    {

        AntiBug();

        switch (type)
        {
            // 普通型
            case 0:
                lr.startWidth -= 0.01f;
                lr.endWidth -= 0.01f;
                Vector3 temp = lr.GetPosition(0);
                temp.z += 0.0001f;
                lr.SetPosition(0, temp);
                if (lr.startWidth <= 0)
                {
                    Destroy(gameObject);
                }

                break;
            // 贯通型
            case 1:
                m.SetTextureOffset("_MainTex", new Vector2(-Time.time, 0));
                Color c = lr.startColor;
                c.a -= 0.01f;
                lr.startColor = c;
                lr.endColor = c;
                if (lr.startColor.a <= 0f)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    // 稍微调整起始点z坐标防显示出错
    private void AntiBug()
    {
        Vector3 temp2 = lr.GetPosition(0);
        temp2.z += 0.0001f;
        lr.SetPosition(0, temp2);
    }

}
