using UnityEngine;
using UnityEngine.UI;

/*
 * 标题场景管理器
 */
public class TitleMgr : MonoBehaviour

{
    [SerializeField]
    private GameObject titlebgm;      // BGM
    [SerializeField]
    private GameObject blocks;        // 背景方块
    [SerializeField]
    private Button tbtn;              // 教程按钮
    [SerializeField]
    private Button nbtn;              // 普通模式按钮
    [SerializeField]
    private Button ibtn;              // 无限模式按钮
    [SerializeField]
    private Image timage;             // 标题文字图片
    [SerializeField]
    private TitleCameraControl mc;    // 摄像机控制器
    [SerializeField]
    private AudioSource clickSE;      // 点击按钮音效
    [SerializeField]
    private Image spThanksImg;        // 鸣谢
    [SerializeField]
    private GameObject spThanksBtn;   // 鸣谢按钮
    [SerializeField]
    private Image guide1;             // 卡牌操作引导
    [SerializeField]
    private Image guide2;             // STG操作引导

    private int phase = 0;            // 演出阶段
    private int mode = 0;             // 游戏模式
    private float uiPosShift = 0f;    // UI位置偏移量
    private bool startTitleAnime;     // 开始标题UI动画

    void Start()
    {
        Screen.SetResolution(1400, 720, false);
        blocks.SetActive(true);
        nbtn.interactable = false;
        ibtn.interactable = false;
    }

    void Update()
    {
        // 点击标题按钮后依次移动UI
        if (startTitleAnime)
        {
            titlebgm.GetComponent<AudioSource>().volume -= 0.005f;
            mc.glow += 0.3f;

            timage.transform.Translate(Vector3.left * uiPosShift);
            tbtn.transform.Translate(Vector3.right * uiPosShift * uiPosShift);
            uiPosShift += 0.05f;

            if (uiPosShift > 2f)
            {
                float shift2 = uiPosShift - 2f;
                nbtn.transform.Translate(Vector3.right * shift2 * shift2);
            }
            if (uiPosShift > 4f)
            {
                mc.rad += mc.rad < 6.5 ? 0.01f : 0f;
                float shift3 = uiPosShift - 4f;
                ibtn.transform.Translate(Vector3.right * shift3 * shift3);
                phase = 1;
            }
            // 显示引导图
            if (uiPosShift > 10f)
            {
                if (!guide2.gameObject.activeSelf)
                {
                    if (guide1.color.a < 1f)
                    {
                        var c = guide1.color;
                        c.a += 0.02f;
                        guide1.color = c;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        guide2.gameObject.SetActive(true);
                    }
                }
                else if (guide1.gameObject.activeSelf)
                {
                    if (guide1.color.a > 0)
                    {
                        var c = guide1.color;
                        c.a -= 0.04f;
                        guide1.color = c;
                    }
                    else if (guide2.color.a < 1f)
                    {
                        var c = guide2.color;
                        c.a += 0.02f;
                        guide2.color = c;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        guide1.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (guide2.color.a > 0)
                    {
                        var c = guide2.color;
                        c.a -= 0.04f;
                        guide2.color = c;
                    }
                    else
                    {
                        // 载入场景
                        startTitleAnime = false;
                        LoadMainScene(mode);
                    }
                }

            }
        }
        else
        {
            // 标题UI渐入
            Color c = timage.color;
            c.a += 0.002f;
            timage.color = c;
            tbtn.image.color = c;
            nbtn.image.color = c;
            ibtn.image.color = c;
        }
    }

    private void LoadMainScene(int m)
    {
        switch (m)
        {
            case 1:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Boss");
                break;
            case 2:
                // DontDestroyOnLoad(titlebgm);
                // UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
                break;
            case 3:
                // DontDestroyOnLoad(titlebgm);
                // UnityEngine.SceneManagement.SceneManager.LoadScene("Inf");
                break;
        }
    }

    // 开始DEMOBOSS战
    public void OnDemoStartBtnClick()
    {
        mode = 1;
        tbtn.colors = OnClicked(tbtn.colors);
    }

    // 开始普通模式
    public void OnNormalStartBtnClick()
    {
        mode = 2;
        nbtn.colors = OnClicked(nbtn.colors);
    }

    // 开始无限模式
    public void OnInfiniteStartBtnClick()
    {
        mode = 3;
        ibtn.colors = OnClicked(ibtn.colors);
    }

    // 通用
    private ColorBlock OnClicked(ColorBlock c)
    {
        clickSE.Play();
        startTitleAnime = true;
        tbtn.interactable = false;
        nbtn.interactable = false;
        ibtn.interactable = false;
        spThanksImg.enabled = false;
        spThanksBtn.SetActive(false);
        mc.stopAuto = true;
        c.disabledColor = new Color(0, 0, 0, 255);
        return c;
    }

    // 右下小按钮显示特别感谢
    public void ShowSpThanks()
    {
        spThanksImg.enabled = !spThanksImg.enabled;
    }

}
