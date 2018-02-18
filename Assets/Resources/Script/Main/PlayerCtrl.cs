using UnityEngine;

/*
 * 自机管理
 */
public class PlayerCtrl : MonoBehaviour
{
    // 基本参数
    [SerializeField]
    private float speed;
    [SerializeField]
    private GameObject lifeObj;
    [SerializeField]
    private int life;
    [SerializeField]
    private ParticleSystem damagePart;
    [SerializeField]
    private float immortalTime = 0.2f;
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private BattleCamera camctrl;
    [SerializeField]
    private GameObject bomb;
    [SerializeField]
    private GameObject back;
    [SerializeField]
    private ParticleSystem gameoverPart;

    // 限制区域
    [Space]
    [SerializeField]
    private float xMin;
    [SerializeField]
    private float xMax;
    [SerializeField]
    private float yMin;
    [SerializeField]
    private float yMax;

    // 被弹相关处理
    [Space]
    [HideInInspector]
    public float slowCnt = 2f;
    [HideInInspector]
    public bool slow = false;
    [HideInInspector]
    public bool normalSlow = false;
    [HideInInspector]
    public float nslowCnt = 2f;

    private float immortalLeft = 0f;
    private float gameoverCnt = 3f;
    private float bombCnt = 1f;
    private bool waringFlash = false;
    private bool clearBomb = false;
    private Material bodymat;
    private Rigidbody rb;

    // 音效
    [SerializeField]
    private AudioSource se00;
    [SerializeField]
    private AudioSource se01;
    [SerializeField]
    private AudioSource se02;
    [SerializeField]
    private AudioSource se03;
    [SerializeField]
    private AudioSource se04;
    private float seCnt04 = 0f;
    [SerializeField]
    private AudioSource se05;
    [SerializeField]
    private AudioSource se06;
    [SerializeField]
    private AudioSource se07;

    // 地图两边的墙
    [SerializeField]
    private GameObject lwall;
    [SerializeField]
    private GameObject rwall;

    public int Life { get { return life; } set { life = value; } }

    void Start()
    {
        life = 5;
        rb = GetComponent<Rigidbody>();
        bodymat = body.GetComponent<Renderer>().material;
        bomb.SetActive(false);
        bomb.gameObject.transform.localScale = new Vector3(0, 0, 0.1f);
        gameoverPart.gameObject.SetActive(false);
        // 忽略B和自身的碰撞判定
        Physics.IgnoreCollision(bomb.GetComponent<Collider>(), GetComponent<Collider>(), true);
    }

    void Update()
    {
        // 终局
        if (BattleMgr.bossPhase == 4)
        {
            transform.position = Vector3.zero;
            return;
        }

        // Gameover自爆
        if (BattleMgr.gameOver && !BattleMgr.gameRealOver)
        {
            gameoverCnt -= Time.unscaledDeltaTime;
            if (seCnt04 <= 0)
            {
                se04.Play();
                seCnt04 = 0.1f;
            }
            else
            {
                seCnt04 -= Time.unscaledDeltaTime;
            }
            if (gameoverCnt <= 0)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                body.SetActive(false);
                back.SetActive(false);
                se03.Play();
                BattleMgr.gameRealOver = true;
                camctrl.Shake2();
            }
            return;
        }


        ScreenClearBomb();
        MainProcess();

    }


    // 放B处理
    private void ScreenClearBomb()
    {
        // 利用B的gameobject回合结束清屏
        if (!BattleMgr.tMode)
        {
            if (BattleMgr.turnTimer <= 0f)
            {
                if (!clearBomb)
                {
                    se05.Play();
                    Vector3 s = new Vector3(200, 200, 0);
                    bomb.SetActive(true);
                    camctrl.Shake2();
                    bomb.GetComponent<Renderer>().enabled = false;
                    bomb.transform.localScale = s;
                    clearBomb = true;
                }
            }
            else if (clearBomb)
            {
                bomb.SetActive(false);
                bomb.gameObject.transform.localScale = new Vector3(0, 0, 0.1f);
                bomb.transform.parent = gameObject.transform;
                bomb.GetComponent<Renderer>().enabled = true;
                bomb.transform.localPosition = new Vector3(0, 0, 5f);
                clearBomb = false;
            }

            xMin = lwall.GetComponent<WallTextureScroll>().GetX() + 8;
            xMax = rwall.GetComponent<WallTextureScroll>().GetX() - 8;
        }
        // 普通的B
        if (bomb.activeSelf)
        {
            if (bombCnt <= 0)
            {
                bomb.SetActive(false);
                bombCnt = 1f;
                bomb.gameObject.transform.localScale = new Vector3(0, 0, 0.1f);
                bomb.transform.parent = gameObject.transform;
                bomb.transform.localPosition = new Vector3(0, 0, 5f);
                bomb.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.7f, 0.7f, 0.7f, 1f));
            }
            else
            {
                bomb.transform.parent = null;
                bombCnt -= Time.unscaledDeltaTime;
                Vector3 scale = bomb.gameObject.transform.localScale;
                scale.x += (10 - scale.x) / 15f;
                scale.y += (10 - scale.y) / 15f;
                bomb.gameObject.transform.localScale = scale;
                Color color = bomb.GetComponent<Renderer>().material.GetColor("_Color");
                color.r /= 1.005f;
                color.g /= 1.005f;
                color.b /= 1.005f;
                if (bombCnt < 0.5f)
                {
                    color.r -= 0.02f;
                    color.g -= 0.02f;
                    color.b -= 0.02f;
                }
                bomb.GetComponent<Renderer>().material.SetColor("_Color", color);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) && !BattleMgr.gamePause && BattleMgr.bomb > 0 && !BattleMgr.gameOver && BattleMgr.controlable)
            {
                se01.Play();
                BattleMgr.bomb--;
                bomb.SetActive(true);
                camctrl.Shake2();
            }
        }
    }

    // 自机主处理
    private void MainProcess()
    {

        if (!BattleMgr.gamePause)
        {
            if (normalSlow)
            {
                if (nslowCnt > 0)
                {

                    nslowCnt -= Time.unscaledDeltaTime;
                    Time.timeScale += 0.01f;
                    Time.fixedDeltaTime += 0.0002f;
                }
                else
                {

                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02f;
                    normalSlow = false;
                }
            }
            // 移动处理
            if (BattleMgr.controlable)
            {
                MoveProcess();
            }
            // 地图边缘处理
            if (transform.position.x > xMax + 0.1f || transform.position.x < xMin - 0.1f || transform.position.y > yMax + 0.1f || transform.position.y < yMin - 0.1f)
            {
                Damage();
            }
            // 其他
            immortalLeft -= Time.deltaTime;
            if (slow)
            {
                slowCnt -= Time.unscaledDeltaTime;
                if (slowCnt <= 0)
                {
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02f;
                }

                if (waringFlash)
                {
                    Color color = back.GetComponent<Renderer>().material.GetColor("_Color");
                    if (color.r >= 1f)
                    {
                        waringFlash = false;
                    }
                    color.r += 0.05f;
                    back.GetComponent<Renderer>().material.SetColor("_Color", color);
                }
                else
                {
                    Color color = back.GetComponent<Renderer>().material.GetColor("_Color");
                    if (color.r <= 0f)
                    {
                        waringFlash = true;
                    }
                    color.r -= 0.05f;
                    back.GetComponent<Renderer>().material.SetColor("_Color", color);
                }
            }

        }
    }

    // 移动操作
    private void MoveProcess()
    {
        float lr = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
        float ud = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? -1 : Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
        float moveHorizontal = ((Input.GetAxis("Horizontal") + lr * 4) / 5) * Time.deltaTime;
        float moveVertical = ((Input.GetAxis("Vertical") + ud * 4) / 5) * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveHorizontal /= 2;
            moveVertical /= 2;
        }

        rb.position = new Vector3(rb.position.x + moveHorizontal * speed, rb.position.y + moveVertical * speed, 10.0f);
        rb.position = new Vector3
       (
           Mathf.Clamp(rb.position.x, xMin, xMax),
           Mathf.Clamp(rb.position.y, yMin, yMax),
            10.0f
        );

    }


    // 脱离致命状态
    public void ResetCrit()
    {
        slow = false;
        slowCnt = 2f;
        back.GetComponent<Renderer>().material.SetColor("_Color", Color.black);

    }
    // 被弹
    public void Damage()
    {
        if (BattleMgr.muteki)
        {
            se06.Play();
            return;
        }
        if (immortalLeft <= 0)
        {
            // 如果在致命状态下再次被弹gameover
            if (slow == true)
            {
                GetComponent<Collider>().enabled = false;
                gameoverPart.gameObject.SetActive(true);
                BattleMgr.gameOver = true;
                BattleMgr.controlable = false;
                Time.timeScale = 0.1f;
                damagePart.Emit(150);
                Time.fixedDeltaTime = 0.1f * 0.02f;
                waringFlash = false;
                se02.Play();
                return;
            }
            immortalLeft = immortalTime; // 重置无敌时间
            // 正常被弹
            if (life > 0)
            {
                camctrl.Shake1();
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                se00.Play();
                life--;
                damagePart.Emit(50);
                waringFlash = false;
                if (BattleMgr.nSlow)
                {
                    nslowCnt = 1f;
                    normalSlow = true;
                    Time.fixedDeltaTime = 0.1f * 0.02f;
                    Time.timeScale = 0.1f;
                }
            }
            // 进入致命伤状态
            else
            {
                camctrl.Shake3();
                slow = true;
                damagePart.Emit(50);
                se07.Play();
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = 0.1f * 0.02f;
                waringFlash = true;
                life = 0;
            }
        }
    }


}
