using UnityEngine.PostProcessing;
using UnityEngine;

/*
 * 摄像机后期效果操作
 */
public class TitleCameraControl : MonoBehaviour
{

    [SerializeField]
    private TitleMgr tmgr;               // 标题流程控制器
    private PostProcessingProfile ppp;   // 后期效果参数

    public float glow = 0.2f;            // 辉光强度
    public float rad = 2.8f;             // 辉光范围
    public bool stopAuto;                // 停止自动变化

    void Start()
    {
        ppp = GetComponent<PostProcessingBehaviour>().profile;
        glow = 40f;
        rad = 2.8f;
    }

    void Update()
    {
        var p = ppp.bloom.settings;
        glow += stopAuto ? 0 : (0.2f - glow) / 50f;
        p.bloom.intensity = glow;
        p.bloom.radius = rad;
        ppp.bloom.settings = p;
    }

}
