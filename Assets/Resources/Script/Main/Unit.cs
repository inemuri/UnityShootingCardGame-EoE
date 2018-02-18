using UnityEngine;
using UnityEngine.UI;

/*
 * 敌方单位运动和攻击模式（未整理）
 */
public class Unit : MonoBehaviour
{
    // 基本参数
    [Header("Property")]
    public int actionType = 0;
    [SerializeField]
    private float defaultSpeed = 1f;
    [SerializeField]
    private int maxhp = 100;
    [SerializeField]
    private ParticleSystem[] weapons;
    [SerializeField]
    private GameObject body;

    [Header("Debug")]
    [SerializeField]
    private int hp;
    [SerializeField]
    private bool dead = false;
    [SerializeField]
    private GameObject targeting;
    private Rigidbody targetrb;
    private Rigidbody rb;
    private float lifetime = 0;
    private float deadtime = 0;

    // BOSS战渐进计数和特效
    private float BossCount = 0f;
    private float damageFlashCnt = 0;
    private bool damageFlash = false;
    private bool shake = false;
    public BattleCamera cam;
    private Material bodymat;
    private Color origColor;

    // BOSS外形和演出
    private int[] ATKBcount = new int[6];
    private float accspd = 0f;
    private float shiftPos;
    private float soundDelay;
    public ParticleSystem BodyOneExplow;
    public ParticleSystem AtkMae;
    public ParticleSystem atk1;
    public GameObject BodyOne;
    public GameObject BossWing;
    public GameObject BossLeftHand;
    public GameObject BossLeftArm;
    public GameObject BossRightHand;
    public GameObject BossRightArm;
    public GameObject mainBody1;
    public GameObject mainBody2;
    public GameObject[] AtkB;
    public GameObject AtkBBody;

    // BOSS攻击相关
    public AudioSource B1;
    public AudioSource AtkBSE;
    public AudioSource WallSE;
    public AudioSource WallHandSE;
    public ParticleSystem botAtk;
    public GameObject WaveObject;

    // BOSS击破后演出
    private int deadPhase = 0;
    private float sasasaCnt = 0.3f;
    public Image FianlScreen;
    public AudioSource sasasasa;
    public AudioSource sasasasaBB;
    public AudioSource sasasasaCC;
    public ParticleSystem Dead;
    public ParticleSystem DeadSon;
    public GameObject FinalPos;

    // UI隐藏用
    public RectTransform hpbar;
    public GameObject holeHpBar;

    public bool IsDead
    {
        get
        {
            return dead;
        }
    }

    public int Hp
    {
        get
        {
            return dead ? 0 : hp;
        }

        set
        {
            if (actionType == 10 || actionType == 11 || actionType == 12)
            {
                hp = value;
                return;
            }
            if (!dead)
            {
                if (value > 0)
                {
                    damageFlashCnt = 0.3f;
                }
                hp = value;
                if (hp <= 0)
                {
                    hp = 0;
                    Kill(actionType);
                }
                if (hp > maxhp)
                {
                    hp = maxhp;
                }
                if (actionType == 2 || actionType == 3 || actionType == 4)
                {
                    return;
                }
                float hpPercent = ((float)hp / (float)maxhp) * 0.8f + 0.2f;
                Color hpfade = new Color(origColor.r * hpPercent, origColor.g * hpPercent, origColor.b * hpPercent, origColor.a);
                bodymat.SetColor("_Color", hpfade);
            }
        }
    }

    void Start()
    {
        if (actionType == 10)
        {
            FinalPos.gameObject.SetActive(false);
            botAtk.gameObject.SetActive(false);
            Dead.gameObject.SetActive(false);
            AtkMae.gameObject.SetActive(false);
            mainBody1.SetActive(false);
            mainBody2.SetActive(false);
            BossLeftArm.SetActive(false);
            BossRightArm.SetActive(false);
            BossWing.SetActive(false);
            hp = maxhp;
            origColor = Color.red;
            foreach (GameObject ps in AtkB)
            {
                ps.gameObject.SetActive(false);
            }

            foreach (ParticleSystem ps in weapons)
            {
                ps.gameObject.SetActive(false);
            }

            for (int i = 0; i < ATKBcount.Length; i++)
            {
                ATKBcount[i] = Random.Range(0, 180);
            }

        }
        else if (actionType == 11)
        {
        }
        else if (actionType == 12)
        {

        }
        else
        {
            hp = maxhp;
            bodymat = body.GetComponent<Renderer>().material;
            origColor = bodymat.GetColor("_Color");
            rb = GetComponent<Rigidbody>();
            Vector3 scale = transform.localScale;

            foreach (ParticleSystem ps in weapons)
            {
                ps.gameObject.SetActive(false);
            }

            switch (actionType)
            {
                case 2:
                    scale.x *= Random.Range(0.8f, 2.5f);
                    scale.y *= Random.Range(0.8f, 2.5f);
                    scale.z = 2.0f;
                    transform.localScale = scale;
                    break;
                case 3:
                    scale.x *= Random.Range(0.8f, 2.5f);
                    scale.y *= Random.Range(0.8f, 2.5f);
                    scale.z = 2.0f;
                    transform.localScale = new Vector3(Random.Range(0.8f, 2.5f), Random.Range(0.8f, 2.5f), 2f);
                    shiftPos = transform.position.x + 14f;
                    break;
                case 4:
                    scale.x *= Random.Range(0.8f, 2.5f);
                    scale.y *= Random.Range(0.8f, 2.5f);
                    scale.z = 2.0f;
                    transform.localScale = new Vector3(Random.Range(0.8f, 2.5f), Random.Range(0.8f, 2.5f), 2f);
                    shiftPos = transform.position.x - 14f;
                    break;
            }

            if (targeting != null)
            {
                targetrb = targeting.GetComponent<Rigidbody>();
            }
        }
    }

    void Update()
    {
        if (actionType == 10)
        {
            if (BattleMgr.gameRealOver)
            {
                return;
            }
            if (!BattleMgr.gamePause)
            {
                switch (actionType)
                {
                    case 10:
                        Act10();
                        break;
                }
            }
        }
        else
        {
            NormalUpdate();
        }
    }

    // BOSS战流程
    private void Act10()
    {
        if (dead)
        {
            if (BattleMgr.bossPhase >= 3)
            {
                GameObject.Find("BGM").GetComponent<AudioSource>().volume -= 0.01f;
                FianlScreen.color = new Color(FianlScreen.color.r, FianlScreen.color.g, FianlScreen.color.b, FianlScreen.color.a - 0.002f);
                if (FianlScreen.color.a <= 0.01f)
                {
                    FianlScreen.color = new Color(FianlScreen.color.r, FianlScreen.color.g, FianlScreen.color.b, FianlScreen.color.a + 0.01f);
                    BattleMgr.gameRealOver = true;
                }
            return;
            }
        }
        else
        {
            if (BattleMgr.bossPhase < 3)
            {
                BossCount += Time.deltaTime;
            }
            else
            {
                FianlScreen.color = new Color(FianlScreen.color.r, FianlScreen.color.g, FianlScreen.color.b, FianlScreen.color.a - 0.005f);
                BossCount += Time.unscaledDeltaTime;
            }
            var c = hpbar.localScale;
            c.x = (float)hp / maxhp;
            hpbar.localScale = c;

            switch (BattleMgr.bossPhase)
            {
                // 第一阶段
                case -1:
                    if (BattleMgr.turn == 2)
                    {
                        BossCount = 0;
                        BattleMgr.bossPhase++;
                    }
                    else
                    {
                        if (BattleMgr.turnTimer < 20)
                        {
                            if (!AtkMae.gameObject.activeSelf)
                            {
                                AtkMae.gameObject.SetActive(true);
                            }
                        }
                    }
                    break;
                // 第二阶段
                case 0:
                    if (hp <= 0)
                    {
                        Destroy(AtkMae.gameObject);
                        BattleMgr.turnTimer = 8f;
                        mainBody1.SetActive(true);
                        mainBody2.SetActive(true);
                        BossLeftArm.SetActive(true);
                        BossRightArm.SetActive(true);
                        for (int i = 0; i < AtkB.Length; i++)
                        {
                            AtkB[i].SetActive(false);
                        }
                        GetComponent<SphereCollider>().radius = 0.3f;
                        B1.Play();
                        cam.Shake3();
                        Destroy(BodyOne);
                        BodyOneExplow.Play();
                        hp = maxhp;
                        BossCount = 0;
                        BattleMgr.bossPhase++;
                    }
                    else
                    {
                        if (transform.position.y > 3f)
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y - 0.01f, transform.position.z);
                        }

                        if (BossCount > 3f && BossCount < 6f)
                        {
                            for (int i = 0; i < AtkB.Length; i++)
                            {
                                AtkB[i].SetActive(true);
                                var line = AtkB[i].GetComponent<LineRenderer>();
                                if (line.GetPosition(1).x < 8)
                                {
                                    line.SetPosition(1, new Vector3((line.GetPosition(1).x + 0.08f), 0, 0));
                                }
                            }
                            AtkBBody.transform.Rotate(0, 0, 6f - BossCount);
                        }
                        if (BossCount > 6f)
                        {
                            for (int i = 0; i < AtkB.Length; i++)
                            {
                                var ps = AtkB[i].GetComponent<ParticleSystem>();
                                ATKBcount[i]--;
                                if (ATKBcount[i] <= 0)
                                {
                                    ATKBcount[i] = Random.Range(120, 180);
                                    ps.Emit(100);
                                    AtkBSE.Play();
                                }
                            }
                        }
                    }
                    break;
                // 第三阶段
                case 1:
                    if (hp <= 0)
                    {
                        BattleMgr.turnTimer += 10f;
                        for (int i = 0; i < AtkB.Length; i++)
                        {
                            AtkB[i].SetActive(true);
                            AtkB[i].GetComponent<SelfRotation>().enabled = true;
                            AtkB[i].GetComponent<SelfRotation>().spd = i;
                        }
                        Destroy(WaveObject);
                        Destroy(botAtk.gameObject);
                        B1.Play();
                        Destroy(BossLeftArm);
                        Destroy(BossRightArm);
                        hp = maxhp = 35000;
                        BossCount = 0;
                        BattleMgr.bossPhase++;
                    }
                    else
                    {
                        Vector3 lpos = BossLeftHand.transform.position;
                        Vector3 rpos = BossRightHand.transform.position;
                        if (BossCount > 1f && BossCount < 3f)
                        {
                            lpos.x += (-7 - lpos.x) / 10f;
                            rpos.x += (7 - rpos.x) / 10f;
                            if (lpos.x < -6.9f)
                            {
                                if (!shake)
                                {
                                    WallHandSE.Play();
                                    cam.ShakeSmall();
                                    shake = true;
                                }
                                cam.ShakeSmall();
                            }
                        }
                        else if (BossCount > 3f && BossCount < 5f)
                        {
                            if (rpos.x > -1 && lpos.x < 1)
                            {

                                if (rpos.x < 0.4 && shake)
                                {
                                    WallSE.Play();
                                    cam.Shake2();
                                    cam.Shake3();
                                    shake = false;
                                }
                                rpos.x += -accspd;
                                lpos.x += accspd;
                                accspd += Time.deltaTime / 2;
                            }
                            rpos.y += (0.75f - rpos.y) / 15f;
                            lpos.y += (0.75f - lpos.y) / 15f;
                        }
                        else if (BossCount > 5f && BossCount < 8f)
                        {
                            rpos.x += (1.75f - rpos.x) / 20f;
                            lpos.x += (-1.75f - lpos.x) / 20f;
                            rpos.y += (4f - rpos.y) / 20f;
                            lpos.y += (4f - lpos.y) / 20f;
                        }
                        if (BossCount > 12f)
                        {
                            if (!botAtk.gameObject.activeSelf)
                            {
                                botAtk.gameObject.SetActive(true);
                                botAtk.Play();
                            }
                        }
                        BossLeftHand.transform.position = lpos;
                        BossRightHand.transform.position = rpos;

                    }
                    break;
                // 第四阶段
                case 2:
                    if (hp <= 0)
                    {
                        FianlScreen.color = Color.red;
                        Time.timeScale = 0.1f;
                        Time.fixedDeltaTime = 0.1f * 0.02f;
                        Destroy(AtkBBody);
                        B1.Play();
                        atk1.Stop();
                        Destroy(atk1.gameObject);
                        BossCount = 0;
                        holeHpBar.gameObject.SetActive(false);
                        GetComponent<SelfRotation>().enabled = false;
                        BattleMgr.bossPhase++;
                    }
                    else
                    {
                        if (BossCount < 0.5f)
                        {
                            cam.Shake3();
                        }
                        if (BossCount > 2f)
                        {
                            if (!atk1.isPlaying)
                            {
                                atk1.Play();
                            }
                        }
                        cam.gameObject.GetComponent<Camera>().backgroundColor = new Color((75 + Mathf.Sin(Time.time * 3) * 25) / 255, 0, 0, 255);
                        BossWing.SetActive(true);
                        if (BossCount > 8f)
                        {
                            for (int i = 0; i < AtkB.Length; i++)
                            {
                                var ps = AtkB[i].GetComponent<ParticleSystem>();
                                ATKBcount[i]--;
                                if (ATKBcount[i] <= 0)
                                {
                                    int iii = (int)BossCount;
                                    iii = 180 - iii < 60 ? 60 : 180 - iii;
                                    ATKBcount[i] = Random.Range(iii, 240);
                                    ps.Emit(100);
                                    AtkBSE.Play();
                                }
                            }
                        }
                    }
                    break;
                // 击破后演出
                case 3:
                    BattleMgr.turnTimer = 999f;
                    transform.position = new Vector3(transform.position.x, transform.position.y + (0 - transform.position.y) / 100f, transform.position.z);
                    GameObject.Find("BGM").GetComponent<AudioSource>().volume -= 0.0005f;
                    if (BossCount < 2)
                    {
                        cam.ShakeSmall();
                    }
                    else
                    {
                        if (deadPhase < 2 && BossCount > 4)
                        {
                            if (sasasaCnt > 0)
                            {
                                sasasaCnt -= Time.unscaledDeltaTime;
                            }
                            else
                            {
                                sasasaCnt = 0.2f;
                                sasasasa.Play();
                            }

                        }
                    }
                    if (BossCount > 4 && BossCount < 9)
                    {
                        if (deadPhase == 0)
                        {
                            FianlScreen.color = Color.white;
                            Dead.gameObject.SetActive(true);
                            Dead.Play();
                            deadPhase++;
                        }
                    }
                    else if (BossCount > 9)
                    {
                        if (deadPhase == 1)
                        {
                            cam.StartFinalEffect();
                            var dt = Dead.main;
                            var st = DeadSon.main;
                            dt.simulationSpeed -= 0.01f;
                            st.simulationSpeed -= 0.01f;

                            if (dt.simulationSpeed < 0.1f)
                            {
                                deadPhase++;
                            }
                        }
                    }
                    if (deadPhase == 2)
                    {
                        BattleMgr.controlable = false;
                        WallSE.Play();
                        sasasasaBB.Play();
                        sasasasaCC.Play();
                        FinalPos.gameObject.SetActive(true);
                        cam.gameObject.GetComponent<Camera>().backgroundColor = new Color(0.7f, 0.7f, 0.7f);
                        Destroy(Dead.gameObject);
                        FianlScreen.color = Color.white;
                        Destroy(BossWing.gameObject);
                        cam.Shake2();
                        Time.timeScale = 1f;
                        Time.fixedDeltaTime = 0.02f;
                        dead = true;
                        BattleMgr.bossPhase++;
                        mainBody1.SetActive(false);
                        mainBody2.SetActive(false);
                    }
                    break;
            }
        }
    }

    // 通用处理
    private void NormalUpdate()
    {
        if (BattleMgr.gameRealOver)
        {
            return;
        }
        if (BattleMgr.gamePause)
        {
        }
        else
        {
            if (dead)
            {
                Vector3 pos = transform.position;
                pos.y -= BattleMgr.scrollSpeedMultiply * Time.deltaTime;
                transform.position = pos;
                bodymat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 0.5f));
            }
            else
            {
                Act(actionType);
                lifetime += Time.deltaTime;

                if (damageFlashCnt > 0)
                {
                    damageFlashCnt -= Time.deltaTime;
                    if (damageFlashCnt > 0)
                    {
                        if (damageFlash)
                        {
                            Color flash = bodymat.GetColor("_Color");
                            flash.r -= 0.3f;
                            flash.g -= 0.3f;
                            flash.b -= 0.3f;
                            bodymat.SetColor("_Color", flash);
                            damageFlash = false;
                        }
                        else
                        {
                            Color flash = bodymat.GetColor("_Color");
                            flash.r += 0.3f;
                            flash.g += 0.3f;
                            flash.b += 0.3f;
                            bodymat.SetColor("_Color", flash);
                            damageFlash = true;
                        }
                    }
                    else
                    {
                        if (damageFlash)
                        {
                            Color ff = bodymat.GetColor("_Color");
                            ff.r -= 0.3f;
                            ff.g -= 0.3f;
                            ff.b -= 0.3f;
                            bodymat.SetColor("_Color", ff);
                            damageFlash = false;
                        }
                    }
                }

            }
            if (transform.position.y < -5f)
            {
                foreach (ParticleSystem ps in weapons)
                {
                    var e = ps.emission;
                    e.enabled = false;

                }
            }

            if (transform.position.y < -15f)
            {
                foreach (ParticleSystem ps in weapons)
                {
                    ps.gameObject.transform.parent = null;
                }
                BattleMgr.activeUnits.Remove(gameObject);
                Destroy(gameObject);
            }
        }
    }

    // 碰撞处理
    private void OnTriggerStay(Collider collision)
    {
        if (actionType == 10)
        {
            Act10();
            return;
        }
        if (!dead)
        {
            if (collision.gameObject.name == "MainPlayer")
            {
                if (actionType != 2 || actionType != 3 || actionType != 4)
                {
                    collision.gameObject.GetComponent<PlayerCtrl>().Damage();
                }
            }
            else if (collision.gameObject.name == "Bomb")
            {
                Hp -= 2;
                return;
            }
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), collision.gameObject.GetComponent<Collider>());
        }
    }

    // 通用死亡处理
    private void Kill(int type)

    {
        BattleMgr.activeUnits.Remove(gameObject);
        var scale = gameObject.transform.localScale;
        gameObject.tag = "Dead";
        gameObject.layer = 13;
        body.gameObject.tag = "Dead";
        body.gameObject.layer = 13;
        deadtime = Time.time;
        dead = true;

        switch (type)
        {
            case 1:
                foreach (var c in GetComponentsInChildren<Transform>())
                {
                    c.parent = gameObject.transform;
                    if (c.gameObject.name == "Core" || c.gameObject.name == "RotateCenter")
                    {
                        c.gameObject.GetComponent<Animator>().enabled = false;
                        c.gameObject.tag = "Dead";
                        c.rotation = Quaternion.identity;
                        c.gameObject.layer = 0;
                    }
                    else if (c.gameObject.name == "SubCube")
                    {
                        c.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 0.5f));
                        c.gameObject.GetComponent<SelfRotation>().enabled = false;
                        var pos = c.gameObject.transform.position;
                        pos.x += Random.Range(-0.5f, 0.5f);
                        pos.y += Random.Range(-0.5f, 0.5f);
                        c.position = pos;
                        c.tag = "Dead";

                        c.rotation = Quaternion.identity;
                        c.gameObject.layer = 0;
                    }
                }
                BattleMgr.DropPause(new int[] { 1, 1, 1 }, transform.position);
                break;
            case 2:
                scale.z = 1.0f;
                gameObject.transform.localScale = scale;
                break;
            case 3:
                scale.z = 1.0f;
                gameObject.transform.localScale = scale;
                break;
            case 4:
                scale.z = 1.0f;
                gameObject.transform.localScale = scale;
                break;
            case 5:
                GetComponent<AudioSource>().Play();
                switch (BattleMgr.tPhase)
                {
                    case 0:
                        BattleMgr.DropPause(new int[] { 0, 11, 9 }, transform.position);
                        break;
                    case 1:
                        BattleMgr.DropPause(new int[] { 11, 11, 13 }, transform.position);
                        break;
                    case 2:
                        BattleMgr.DropPause(new int[] { 0, 0, 0 }, transform.position);
                        break;
                    case 4:
                        BattleMgr.DropPause(new int[] { 10, 1 }, transform.position);
                        break;
                    case 5:
                        BattleMgr.DropPause(new int[] { 11, 0 }, transform.position);
                        break;
                    case 7:
                        BattleMgr.DropPause(new int[] { 1, 11, 1 }, transform.position);
                        break;
                    case 8:
                        BattleMgr.DropPause(new int[] { 0, 0, 0 }, transform.position);
                        break;
                    case 9:
                        BattleMgr.DropPause(new int[] { 12 }, transform.position);
                        break;
                    case 10:
                        BattleMgr.DropPause(new int[] { 1, 4, 12 }, transform.position);
                        break;
                    case 11:
                        BattleMgr.DropPause(new int[] { 1, 1, 0 }, transform.position);
                        break;
                }
                BattleMgr.tPhase++;
                body.transform.rotation = Quaternion.identity;
                break;
            case 0:
            default:
                break;
        }

        gameObject.GetComponent<Collider>().isTrigger = true;
        bodymat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 1f));
        gameObject.transform.rotation = Quaternion.identity;
        body.transform.rotation = Quaternion.identity;
        foreach (ParticleSystem ps in weapons)
        {
            var e = ps.emission;
            e.enabled = false;
        }
        if (body.GetComponent<SelfRotation>() != null)
        {
            body.GetComponent<SelfRotation>().enabled = false;
        }
    }

    // 通用行为处理
    private void Act(int type)
    {
        Vector3 pos = rb.transform.position;
        switch (type)
        {
            case 4:
                pos.x += (shiftPos - pos.x) / 20f;
                pos.y -= Time.deltaTime * defaultSpeed;
                rb.transform.position = pos;
                break;
            case 3:
                pos.x += (shiftPos - pos.x) / 20f;
                pos.y -= Time.deltaTime * defaultSpeed;
                rb.transform.position = pos;
                break;
            case 2:
                pos.y -= Time.deltaTime * defaultSpeed;
                rb.transform.position = pos;
                break;
            case 1:
                if (lifetime < 5)
                {
                    if (lifetime > 3 && lifetime < 4)
                    {
                        weapons[0].gameObject.SetActive(true);
                    }
                    rb.drag += 0.1f;
                }
                if (weapons[0].gameObject.activeSelf && soundDelay <= 0)
                {
                    weapons[0].Emit(22);
                    GetComponent<AudioSource>().Play();
                    soundDelay = 0.5f;
                }
                weapons[0].gameObject.transform.Rotate(new Vector3(0, 0, 1f));
                soundDelay -= Time.deltaTime;
                break;
            case 5:
                if (targeting != null)
                {
                    float step = 1.5f * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, targeting.transform.position, step);
                }
                break;
            case 0:
            default:
                if (targeting != null)
                {
                    float step = 5.5f * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, targeting.transform.position, step);
                }
                break;
        }
    }
}

