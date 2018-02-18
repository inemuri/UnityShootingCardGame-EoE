using UnityEngine;
using UnityEngine.UI;

/*
 * 卡牌在Unity中的显示和操作
 */
public class Card : MonoBehaviour
{
    // 数据
    [SerializeField]
    private int id = 0;                                 // 唯一编号
    public int Id { get { return id; } }

    [SerializeField]
    private string cardname = "Default";                // 实际显示直接调整卡牌上的文字object
    public string Cardname { get { return cardname; } }

    [SerializeField]
    private int cost = 0;                               // 消耗资源
    public int Cost { get { return cost; } }

    [SerializeField]
    [TextArea]
    private string help = "Default";                    // 技能说明文
    public string Help { get { return help; } }

    [HideInInspector]
    public CardPosition nowIsIn = CardPosition.None;    // 卡牌当前位置

    // 操作相关
    [HideInInspector]
    public bool helpEnable = true;                      // 是否启用说明
    [HideInInspector]
    public bool selectEnable = true;                    // 是否可选
    [HideInInspector]
    public bool selected = false;                       // 已选择

    private float mouseOverWaitTime = 0.6f;             // 光标指向卡牌后弹出说明的等待时间
    private float mouseOverCount = 0f;                  // 等待时间计数
    private float selectShiftY = 0.4f;                  // 选定状态Y坐标偏移量
    private static bool showingHelp = false;            // 正在显示说明
    private bool startCount = false;                    // 开始等待时间
    private Vector3 origPos = Vector3.zero;             // 原坐标
    private Vector3 origScale = Vector3.one;            // 原缩放
    private Vector3 helpPos = new Vector3(-4f, 0, -5f); // 启用说明时的坐标

    // 场景中获取
    private Text costtext;                              // cost文字
    private Text helptext;                              // 说明文
    private Renderer artRend;                           // 卡图
    private Renderer frameRend;                         // 卡框
    private Image helpbg;                               // 说明文弹出时背景
    private AudioSource se01;                           // mouseover音效
    private AudioSource se02;                           // 弹出说明音效
    private AudioSource se03;                           // 选定音效
    private GameObject body;                            // 图像容器


    void Start()
    {
        // 查找获取
        try
        {
            body = transform.Find("CardBody").gameObject;
            costtext = transform.Find("CardBody/CardCanvas/Cost").GetComponent<Text>();
            artRend = transform.Find("CardBody/Art").GetComponent<Renderer>();
            frameRend = transform.Find("CardBody/Frame").GetComponent<Renderer>();
            helpbg = GameObject.Find("HelpBG").GetComponent<Image>();
            helptext = GameObject.Find("HelpTEXT").GetComponent<Text>();
            //se01 = GameObject.Find("MouseOverCardSE").GetComponent<AudioSource>();
            //se02 = GameObject.Find("CardHelpSE").GetComponent<AudioSource>();
            //se03 = GameObject.Find("SelectSE").GetComponent<AudioSource>();
        }
        catch
        {
            Debug.Log("Card setup error");
        }
    }

    void Update()
    {
        // 为shader动画传入unscaledTime
        if (artRend.material.GetFloat("USEUNSCALEDTIME") == 1) artRend.material.SetFloat("UNSCALEDTIME", Time.unscaledTime / 20f);
        if (frameRend.material.GetFloat("USEUNSCALEDTIME") == 1) frameRend.material.SetFloat("UNSCALEDTIME", Time.unscaledTime / 20f);
        HelpProcess();
        SelectProcess();
    }

    // 选择处理
    private void SelectProcess()
    {
        // 操作body局部坐标
        Vector3 pos = body.transform.localPosition;
        // 此卡显示说明时/非选中状态时归位 
        if (mouseOverCount > mouseOverWaitTime || !selected)
        {
            pos += (Vector3.zero - pos) / 4f;
        }
        else
        // 选中时向上飘起
        {
            pos.y += (selectShiftY - pos.y) / 4f;
        }
        body.transform.localPosition = pos;
    }

    // 说明文处理
    private void HelpProcess()
    {
        if (helpEnable)
        {
            if (startCount)
            {
                mouseOverCount += Time.unscaledDeltaTime;
                // 如其他卡牌占用了说明界面则重置
                if (showingHelp)
                {
                    CardHelpReset();
                    return;
                }
                // 鼠标指针停留时间到后转入主处理
                if (mouseOverCount > mouseOverWaitTime)
                {
                    startCount = false;
                    showingHelp = true;
                    helpbg.raycastTarget = true;
                    helptext.text = help; // 传入此卡牌说明文字
                }
            }
            // 主处理
            if (mouseOverCount > mouseOverWaitTime && showingHelp)
            {
                HelpDisplay();
            }
        }
        // 不显示说明时记录原本位置缩放
        if (!startCount && !showingHelp)
        {
            origScale = transform.localScale;
            origPos = transform.position;
        }
    }

    // 说明界面主处理
    private void HelpDisplay()
    {
        // 各项动画
        helpbg.color += (new Color(0, 0, 0, 0.9f) - helpbg.color) / 12f;
        helptext.color += (new Color(1f, 1f, 1f, 1f) - helptext.color) / 12f;
        transform.position += (helpPos - transform.position) / 8f;
        transform.localScale += ((Vector3.one * 2) - transform.localScale) / 8f;
        var rot = transform.rotation.eulerAngles;
        rot.y += (359.99f - rot.y) / 18f;
        rot.z += (359.99f - rot.z) / 8f;
        transform.rotation = Quaternion.Euler(rot);
        // 退出处理
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            showingHelp = false;
            mouseOverCount = 0;
            transform.position = origPos;
            transform.localScale = origScale;
            transform.rotation = Quaternion.identity;
            helpbg.color = new Color(0, 0, 0, 0);
            helpbg.raycastTarget = false;
            helptext.text = "";
            helptext.color = new Color(1f, 1f, 1f, 0);
        }
    }

    // EventTrigger用指针移入区域
    public void CardMouseEnter()
    {
        if (helpEnable && !showingHelp)
        {
            startCount = true;
            transform.localScale = origScale * 1.1f;
        }
    }

    // EventTrigger用指针移出区域或其他重置场合
    public void CardHelpReset()
    {
        if (!showingHelp)
        {
            startCount = false;
            mouseOverCount = 0f;
            transform.localScale = origScale;
        }
    }

    // EventTrigger用点击
    public void CardMouseClick()
    {
        if (selectEnable && !showingHelp)
        {
            selected = !selected;
            mouseOverCount = 0f;
        }
    }

}
