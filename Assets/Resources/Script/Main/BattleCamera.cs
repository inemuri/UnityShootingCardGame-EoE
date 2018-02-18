using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

/*
 * 主场景摄像机演出相关（未整理）
 */
public class BattleCamera : MonoBehaviour
{
    public float shakeDecay;                    // 摇屏衰减速度
    public float shakeDuration;                 // 摇屏持续时间
    public float maxDuration = 1f;              // 摇屏最大默认持续时间
    public Image damageScreen;                  // 白屏红屏用图
    public ParticleSystem back;                 // 方块背景
    public Camera frontCam;                     // 前景物体摄像机
    public GameObject leftwall;                 // 左墙
    public GameObject rightwall;                // 右墙
    public GameObject playerBody;               // 自机方块
    public GameObject playerBodyBack;           // 自机方块描边

    private float shakeTime;                    // 当前摇屏计数
    private float glowstart = 20f;              // bloom辉度上限
    private float glowend = 1.2f;               // bloom辉度下限
    private float gameStartCount = 10f;         // 场景开始入场时间计数
    private bool gameStartAnime = false;        // 场景开始时入场
    private bool gameEndAnime = false;          // 游戏结束时出场
    private PostProcessingProfile ppp;          // 后期效果参数
    private Vector3 originPosition;             // 摇屏初始坐标

    void Start()
    {
        // 场景开始时摄像机和后期参数
        ppp = GetComponent<PostProcessingBehaviour>().profile;
        var g = ppp.bloom.settings;
        g.bloom.intensity = glowstart;
        ppp.bloom.settings = g;
        GetComponent<Camera>().orthographicSize = 0.01f;
        frontCam.orthographicSize = 0.01f;
    }

    void Update()
    {
        if (BattleMgr.bossPhase == 2)
        {
            damageScreen.color = new Color(damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, damageScreen.color.a - 0.005f);
        }

        // BOSS击破后爆炸瞬间的处理
        if (gameEndAnime)
        {
            if (BattleMgr.bossPhase == 4)
            {
                var g2 = ppp.bloom.settings;
                g2.bloom.intensity = 2f;
                ppp.bloom.settings = g2;
                gameEndAnime = false;
            }
            var g = ppp.bloom.settings;
            g.bloom.intensity += 0.1f;
            ppp.bloom.settings = g;
        }

        // 回标题前淡出动画
        if (BattleMgr.gameRealOver)
        {
            var g = ppp.bloom.settings;
            g.bloom.intensity += (200f - g.bloom.intensity) / 10f;
            ppp.bloom.settings = g;
        }

        // 开场淡入动画
        if (!gameStartAnime)
        {
            var g = ppp.bloom.settings;
            g.bloom.intensity += (glowend - g.bloom.intensity) / 50f;
            ppp.bloom.settings = g;
            GetComponent<Camera>().orthographicSize += (5f - GetComponent<Camera>().orthographicSize) / 150f;
            frontCam.orthographicSize += (5f - frontCam.orthographicSize) / 150f;
            gameStartCount -= 0.02f;
            if (gameStartCount <= 5)
            {
                GetComponent<Camera>().orthographicSize += (5f - GetComponent<Camera>().orthographicSize) / 100f;
                frontCam.orthographicSize += (5f - frontCam.orthographicSize) / 100f;
                Vector3 posl = leftwall.transform.position;
                Vector3 posr = rightwall.transform.position;
            }
            // 计数结束后玩家方可操作
            if (gameStartCount <= 0)
            {
                g.bloom.intensity = 1.7f;
                ppp.bloom.settings = g;
                playerBody.layer = 8;
                playerBodyBack.transform.localPosition = new Vector3(0, 0, -3f);
                GetComponent<Camera>().orthographicSize = 5f;
                frontCam.orthographicSize = 5f;
                gameStartAnime = true;
                BattleMgr.stgStart = true;
                BattleMgr.controlable = true;
            }
            return;
        }

        // 摇屏闪屏处理
        if (shakeTime > 0)
        {
            transform.position = originPosition + Random.insideUnitSphere * shakeTime;
            shakeTime -= shakeDecay;

            damageScreen.color = new Color(damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, damageScreen.color.a - 0.05f);

            if (BattleMgr.tMode)
            {
                var noise = back.noise;
                noise.positionAmount = shakeTime * 2;
                noise.sizeAmount = shakeTime * 4;
            }
        }
        else if (shakeTime < 0)
        {

            damageScreen.color = new Color(255, 0, 0, 0f);
            if (BattleMgr.tMode)
            {
                var noise = back.noise;
                noise.positionAmount = 0;
                noise.sizeAmount = 0;
            }
            shakeTime = 0;
        }
    }

    public void ShakeSmall()
    {
        if (shakeTime <= 0)
        {
            originPosition = transform.position;
        }
        shakeTime = shakeTime > maxDuration ? 0 : shakeDuration;
    }

    public void Shake1()
    {
        if (shakeTime <= 0)
        {
            originPosition = transform.position;
        }
        shakeTime = shakeTime > maxDuration ? 0 : shakeDuration;
        damageScreen.color = new Color(255, 0, 0, 0.3f);
    }

    public void Shake2()
    {
        if (shakeTime <= 0)
        {
            originPosition = transform.position;
        }
        shakeTime = shakeTime > maxDuration ? 0 : shakeDuration + 0.5f;
        damageScreen.color = new Color(255, 255, 255, 1f);
    }

    public void Shake3()
    {
        if (shakeTime <= 0)
        {
            originPosition = transform.position;
        }
        shakeTime = shakeTime > maxDuration ? 0 : shakeDuration;
        damageScreen.color = new Color(255, 0, 0, 1f);
    }

    public void RedScreen()
    {
        damageScreen.color = new Color(1f, 0, 0, 1f);
    }

    public void WhiteScreen()
    {
        damageScreen.color = new Color(1f, 1f, 1f, 1f);
    }

    // 终局演出
    public void StartFinalEffect()
    {
        if (BattleMgr.bossPhase < 4)
        {
            gameEndAnime = true;
        }
    }
}