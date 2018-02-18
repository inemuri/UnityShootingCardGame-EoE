using UnityEngine;
using UnityEngine.UI;


/*
 * 卡牌在Unity中的显示和操作
 * 整理版参照 -------- Script/CardBattle/Card.cs
 */
public class CardDisplay : MonoBehaviour
{

    public int cid;
    [SerializeField]
    private Text cardCost;
    [SerializeField]
    private Text cardName;
    [SerializeField]
    [TextArea]
    private string cardHelp;
    [SerializeField]
    private SpriteRenderer cardArt;
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private Renderer artMat;
    [SerializeField]
    private Renderer frameMat;
    [SerializeField]
    private ParticleSystem selectEffect;

    // 帮助模式相关
    [Space]
    public bool helpEnable = true;
    private bool zoom = false;
    private bool zoomed = false;
    private float zoomWait = 1f;
    private float zoomWaitNow = 0f;
    private Vector3 origPos = Vector3.zero;
    private Vector3 origScale = Vector3.zero;
    private Image helpbg;
    private Text helptext;

    [SerializeField]
    private Vector3 helpPos;

    [Space]
    public bool hidden = false;
    public bool selectEnable = true;
    public bool selected = false;

    private AudioSource se01;
    private AudioSource se02;
    private AudioSource se03;

    // Use this for initialization
    void Start()
    {
        se01 = GameObject.Find("MouseOverCardSE").GetComponent<AudioSource>();
        se02 = GameObject.Find("CardHelpSE").GetComponent<AudioSource>();
        se03 = GameObject.Find("SelectSE").GetComponent<AudioSource>();
        selectEffect.gameObject.SetActive(false);
        helpbg = GameObject.Find("HelpBG").GetComponent<Image>();
        helptext = GameObject.Find("HelpTXT").GetComponent<Text>();
    }

    // Update is called once per frame  "测试CARDネーム";
    void Update()
    {
        if (selectEnable)
        {
            SelectProcess();
        }
        if (!selected)
        {
            Vector3 pos = body.transform.localPosition;
            pos += (Vector3.zero - pos) / 3f;
            body.transform.localPosition = pos;
        }
        if (helpEnable)
        {
            HelpProcess();
        }
        body.SetActive(!hidden);
        selectEffect.gameObject.SetActive(selected && !zoomed);
        if (artMat.material.GetFloat("USEUNSCALEDTIME") == 1)
        {
            artMat.material.SetFloat("UNSCALEDTIME", Time.unscaledTime / 20f);
        }
        if (frameMat.material.GetFloat("USEUNSCALEDTIME") == 1)
        {
            frameMat.material.SetFloat("UNSCALEDTIME", Time.unscaledTime / 20f);
        }
    }

    private void SelectProcess()
    {
        Vector3 pos = body.transform.localPosition;
        {
            if (zoomed)
            {
                pos = Vector3.zero;
            }
            else
            {
                if (selected)
                {
                    pos.y += ((1.0f * transform.localScale.y) - pos.y) / 3f;
                }
            }
        }
        body.transform.localPosition = pos;
    }

    private void HelpProcess()
    {
        if (helpEnable)
        {
            if (zoom)
            {
                if (zoomed)
                {
                    helptext.text = cardHelp;
                    Color color = helpbg.color;
                    Vector3 targetPos = new Vector3(helpPos.x += Random.Range(-0.005f, 0.005f), helpPos.y += Random.Range(-0.005f, 0.005f), helpPos.z);
                    Vector3 nowRot = transform.rotation.eulerAngles;
                    color.a += (0.9f - color.a) / 4f;
                    nowRot.y += (359.99f - nowRot.y) / 12f;
                    nowRot.z += (359.99f - nowRot.z) / 6f;
                    helpbg.color = color;
                    helptext.color = new Color(255, 255, 255, color.a);
                    body.transform.localPosition = Vector3.zero;
                    transform.rotation = Quaternion.Euler(nowRot);
                    transform.position += (targetPos - transform.position) / 6f;
                    transform.localScale += ((Vector3.one * 2f) - transform.localScale) / 6f;
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        helptext.text = "";
                        transform.position = origPos;
                        transform.localScale = origScale;
                        transform.rotation = Quaternion.identity;
                        helptext.color = new Color(255, 255, 255, 0);
                        helpbg.color = new Color(0, 0, 0, 0);
                        zoomWaitNow = 0f;
                        zoomed = false;
                        zoom = false;
                        BattleMgr.showingHelp = false;
                    }
                }
                else
                {
                    if (BattleMgr.showingHelp)
                    {
                        zoomWaitNow = 0;
                    }
                    else
                    {
                        zoomWaitNow += Time.unscaledDeltaTime;
                        if (zoomWaitNow > zoomWait)
                        {
                            se02.Play();
                            zoomed = true;
                            BattleMgr.showingHelp = true;
                        }
                    }
                }
            }
            else
            {
                origPos = transform.position;
                origScale = transform.localScale;
            }
        }
    }



    public void MouseOverThisCard()
    {
        if (!BattleMgr.showingHelp && helpEnable)
        {
            se01.Play();
            zoom = true;
            transform.localScale = origScale * 1.1f;
        }
    }

    public void ResetZoomCount()
    {
        if (!BattleMgr.showingHelp && helpEnable)
        {
            zoomWaitNow = 0f;
            transform.localScale = origScale;
            zoom = false;
        }
    }

    public void ClickOnThisCard()
    {
        if (!BattleMgr.showingHelp && selectEnable)
        {
            se03.Play();
            if (selected)
            {
                BattleMgr.cSelecting.Remove(gameObject);
            }
            else
            {
                BattleMgr.cSelecting.Add(gameObject);
            }
            selected = !selected;
            zoomWaitNow = 0f;
            zoom = false;
            transform.localScale = origScale;
        }
    }

    public int GetCost()
    {
        int cost = 0;
        if (cardCost.text != "")
        {
            cost = int.Parse(cardCost.text);
        }
        return cost;
    }
}
