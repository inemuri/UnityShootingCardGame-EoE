using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 主管理器（未整理）
 * 卡牌部分主流程整理版参照 -------- Script/CardBattle/CardBattleMain.cs
 */
public class BattleMgr : MonoBehaviour
{
    // 全局
    public int gameMode = 0;                            // 游戏模式
    public static int turn = 1;                         // 回合
    public static int gamePauseType = -1;               // 暂停类型
    public static int cube = 1;                         // 残机数
    public static int bomb = 0;                         // 炸弹数
    public static int weapon = -1;                      // 自机武器
    public static int tPhase = 0;                       // 教程步骤
    public static int bossPhase = -1;                   // BOSS战步骤
    public static int[] reward;                         // 掉落卡牌
    public static float mainTimer = 0f;                 // 从第1回合到现在经过的有效时间
    public static float turnMaxTime = 10f;              // 回合最大时间
    public static float turnTimer = turnMaxTime;        // 当前回合剩余时间
    public static float scrollSpeedMultiply = 1f;       // 关卡画面全体卷动速度倍率
    public static bool tMode = false;                   // 是否教程战
    public static bool gamePause = false;               // 暂停中
    public static bool showingHelp = false;             // 正在显示卡牌说明
    public static bool controlable = false;             // 玩家可被控制
    public static bool gameOver = false;                // GameOver动画播放中
    public static bool gameRealOver = false;            // 准备返回标题
    public static bool stgStart = false;                // STG阶段开始
    public static bool nSlow = true;                    // 慢镜头      
    public static bool muteki = false;                  // 无敌模式
    public static Vector3 rewardPos;                    // 卡牌掉落位置

    // 各项卡牌容器
    public static List<GameObject> activeUnits = new List<GameObject>();
    public static List<GameObject> pDeck = new List<GameObject>();
    public static List<GameObject> pHand = new List<GameObject>();
    public static List<GameObject> pLibrary = new List<GameObject>();
    public static List<GameObject> pGraveyard = new List<GameObject>();
    public static List<GameObject> eCards = new List<GameObject>();
    public static List<GameObject> cSelecting = new List<GameObject>();
    public static List<GameObject> cReward = new List<GameObject>();
    public static List<int> buffs = new List<int>();

    // 卡牌相关
    public static int playerHandLimit = 1;
    private static float lifeFlashCnt = 0f;
    private int[] tdeck = { };
    private int[] ndeck = { 14, 9, 16, 12, 11, 15, 9, 12, 10, 6, 3, 16, 15, 9, 16, 2, 12, 9, 4, 16, 16, 12, 7, 5 };
    private float playedCardShowTime = 2f;
    private float showCnt;
    private bool animeEnd = false;
    private bool drawed = false;
    private bool readyForPlay = false;
    private bool showing = false;
    private bool rewardShow = false;
    private bool checkDeckMode = false;
    private Vector3 libPos = new Vector3(25f, -3.5f, 0);
    private Vector3 gravePos = new Vector3(-25f, -3.5f, 0);
    [SerializeField]
    private Image menubg;
    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private GameObject lifeInfo;
    [SerializeField]
    private GameObject deckInfo;
    [SerializeField]
    private Button checkdeckBtn;

    // 各项管理器
    [SerializeField]
    private LaserMgr laserMgr;
    [SerializeField]
    private CardDatabase cardData;
    [SerializeField]
    private BattleCamera mainCam;

    // 自机武器相关
    public int damageWA = 100;
    public int damageWB = 2;
    public int damageWC = 1;
    public int brustCountWB = 5;
    public float firerateWA = 0.1f;
    public float firerateWB = 0.25f;
    public float firerateWC = 5f;
    public float firecount = 0f;
    private bool ignoreDB = false;
    public GameObject player;
    public GameObject mousetarget;
    public GameObject playerFrontTarget;
    public GameObject enemy;


    // 游戏流程相关
    private float realGameoverCnt = 6f;
    public Image gameoverwhite;

    // 音效
    public AudioSource[] se;
    private AudioSource bgm;

    void Start()
    {
        if (gameMode == 0)
        {
            nSlow = false;
            tMode = true;
            turnMaxTime = 10f;
            turnTimer = 10f;
            playerHandLimit = 1;
        }
        else if (gameMode == 1)
        {
            bomb = 10;
            tMode = false;
            turnMaxTime = 20f;
            turnTimer = 20f;
            playerHandLimit = 5;
            weapon = 0;
            bgm = GameObject.Find("BGM").GetComponent<AudioSource>();
        }
        CardBattleSystemReady();
        StgReady();
    }

    void Update()
    {
        // 游戏结束和伤害效果
        GameOverAndDamageUpdate();

        // 卡牌掉落
        if (gamePauseType >= 2 && gamePauseType <= 4)
        {
            RewardProcess();
            return;
        }

        // 卡牌系统处理
        CardBattleSystemUpdate();

        // STG处理
        if (stgStart)
        {
            StgUpdate();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !gameOver && stgStart)
        {
            if (gamePause && gamePauseType == 0)
            {
                return;
            }
            Pause();
            gamePauseType = 1;

        }
    }

    private void CardBattleSystemUpdate()
    {

        lifeInfo.GetComponent<Text>().text = "-  CUBE " + player.GetComponent<PlayerCtrl>().Life + "  -";
        deckInfo.GetComponent<Text>().text = "-  " + pGraveyard.Count + "/" + pHand.Count + "/" + pLibrary.Count + "  -";

        if (!gamePause)
        {
            for (int i = 0; i < pHand.Count; i++)
            {
                Vector3 handpos = new Vector3((-pHand.Count + 1) * 0.75f + i * 1.5f, -10f, i * 0.001f);
                pHand[i].transform.position += (handpos - pHand[i].transform.position) / 10f;
            }

            if (turnTimer <= 0)
            {
                se[2].Play();
                TurnEndPhase();
                muteki = false;
                gamePauseType = 0;
                Pause();
                return;
            }


            if (stgStart && !gameOver)
            {
                mainTimer += Time.deltaTime;
                turnTimer -= Time.deltaTime;
            }
        }
        else
        {
            switch (gamePauseType)
            {
                case 0:
                    lifeInfo.GetComponent<Text>().text = "-  CUBE " + player.GetComponent<PlayerCtrl>().Life + " (" + (CheckCost() >= 0 ? "+" : "") + CheckCost() + ")  -";
                    if (showing)
                    {
                        if (cSelecting.Count == 0)
                        {
                            showCnt = 0;
                        }
                        pDeck.ForEach(c =>
                        {
                            CardDisplay cd = c.GetComponent<CardDisplay>();
                            cd.helpEnable = false;
                            cd.selectEnable = false;
                        });
                        for (int i = 0; i < pHand.Count; i++)
                        {
                            if (!cSelecting.Contains(pHand[i]))
                            {
                                Vector3 handpos = new Vector3((-pHand.Count + 1) * 0.75f + i * 1.5f, -10f, i * 0.001f);
                                pHand[i].transform.position += (handpos - pHand[i].transform.position) / 30f;
                            }
                        }
                        for (int i = 0; i < cSelecting.Count; i++)
                        {
                            Vector3 showScale = Vector3.one;
                            Vector3 showPos = new Vector3((-cSelecting.Count + 1) * 1.5f + i * 3f, -1f, -i * 0.002f);
                            cSelecting[i].transform.position += (showPos - cSelecting[i].transform.position) / 10f;
                            cSelecting[i].transform.localScale += (showScale - cSelecting[i].transform.localScale) / 10f;
                        }
                        showCnt -= Time.unscaledDeltaTime;
                        if (showCnt <= 0)
                        {
                            cSelecting.ForEach(c => { c.transform.position = gravePos; c.transform.localScale = Vector3.one / 2; });
                            showCnt = playedCardShowTime;
                            showing = false;
                            PlayCardsPhase();
                            Pause();
                        }
                    }
                    else
                    {
                        if (checkDeckMode)
                        {
                            return;
                        }
                        if (!animeEnd)
                        {
                            playBtn.gameObject.SetActive(true);
                            checkdeckBtn.gameObject.SetActive(true);
                            animeEnd = true;
                            pGraveyard.ForEach(c => c.transform.position += (gravePos - c.transform.position) / 10f);
                            pLibrary.ForEach(c => c.transform.position += (libPos - c.transform.position) / 10f);
                            for (int i = 0; i < pHand.Count; i++)
                            {
                                Vector3 handpos = new Vector3((-pHand.Count + 1) * 0.75f + i * 1.5f, -3.2f, i * 0.001f);
                                if (Mathf.Abs(pHand[i].transform.position.x - handpos.x) >= 0.01f || Mathf.Abs(pHand[i].transform.position.y - handpos.y) >= 0.01f)
                                {
                                    pHand[i].transform.position += (handpos - pHand[i].transform.position) / 4f;
                                    animeEnd = false;
                                    playBtn.gameObject.SetActive(false);
                                    checkdeckBtn.gameObject.SetActive(false);
                                }
                            }
                            if (!animeEnd)
                            {
                                return;
                            }
                        }
                        SelectCardPhase();
                    }
                    break;
                case 1:
                    if (!animeEnd)
                    {
                        checkdeckBtn.gameObject.SetActive(false);
                        animeEnd = true;
                        pGraveyard.ForEach(c => c.transform.position += (gravePos - c.transform.position) / 10f);
                        pLibrary.ForEach(c => c.transform.position += (libPos - c.transform.position) / 10f);
                        for (int i = 0; i < pHand.Count; i++)
                        {
                            Vector3 handpos = new Vector3((-pHand.Count + 1) * 0.75f + i * 1.5f, -3.2f, i * 0.001f);
                            if (Mathf.Abs(pHand[i].transform.position.x - handpos.x) >= 0.01f || Mathf.Abs(pHand[i].transform.position.y - handpos.y) >= 0.01f)
                            {
                                pHand[i].transform.position += (handpos - pHand[i].transform.position) / 4f;
                                animeEnd = false;
                            }
                        }
                        if (animeEnd)
                        {
                            checkdeckBtn.gameObject.SetActive(true);
                            pHand.ForEach(c => c.GetComponent<CardDisplay>().helpEnable = true);
                        }
                    }
                    break;
            }
        }

        if (!checkDeckMode)
        {
            pGraveyard.ForEach(c => c.transform.position += (gravePos - c.transform.position) / 10f);
            pLibrary.ForEach(c => c.transform.position += (libPos - c.transform.position) / 10f);
        }
    }

    private void StgUpdate()
    {
        if (!gamePause && !gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (weapon == 1)
                {
                    weapon = 0;
                }
                else
                {
                    weapon = 1;
                }
            }
            if (weapon == 1)
            {
                if (firecount <= 0)
                {
                    firecount = firerateWB;
                    brustCountWB = 10;
                }

                if (brustCountWB > 0 && Input.GetMouseButton(0))
                {
                    if (brustCountWB % 3 == 0)
                    {
                        se[3].Play();
                    }
                    firecount = firerateWB;
                    laserMgr.ReqLaserAttack(1, player, playerFrontTarget, CanHit.Enemy, damageWB);
                    brustCountWB--;
                }
                firecount = firecount > 0 ? firecount - Time.deltaTime : 0;
            }
            else if (weapon == 0)
            {
                if (firecount <= 0 && Input.GetMouseButton(0))
                {
                    se[4].Play();
                    firecount = firerateWA;
                    if (ignoreDB)
                    {
                        laserMgr.ReqLaserAttack(0, player, mousetarget, CanHit.Enemy, damageWA);
                    }
                    else
                    {
                        laserMgr.ReqLaserAttack(0, player, mousetarget, CanHit.EnemyAndBody, damageWA);
                    }
                }
                else
                {
                    firecount = firecount > 0 ? firecount - Time.deltaTime : 0;
                }
            }
        }
    }

    private void SelectCardPhase()
    {

        while (!drawed && pHand.Count < playerHandLimit)
        {
            if (pLibrary.Count == 0)
            {
                if (pGraveyard.Count == 0)
                {
                    break;
                }
                pLibrary.AddRange(pGraveyard);
                Shuffle(ref pLibrary);
                pGraveyard.ForEach(c => c.transform.position = libPos);
                pGraveyard.Clear();
            }
            GameObject draw = pLibrary[0];
            pHand.Add(pLibrary[0]);
            pLibrary.RemoveAt(0);
        }
        drawed = true;

        if (!readyForPlay)
        {
            readyForPlay = true;
            for (int i = 0; i < pHand.Count; i++)
            {
                Vector3 handpos = new Vector3((-pHand.Count + 1) * 0.75f + i * 1.5f, -3.2f, i * 0.001f);
                pHand[i].transform.position += (handpos - pHand[i].transform.position) / 10f;
                if (Mathf.Abs(pHand[i].transform.position.x - handpos.x) >= 0.01f || Mathf.Abs(pHand[i].transform.position.y - handpos.y) >= 0.01f)
                {
                    readyForPlay = false;
                }
            }
        }
        else
        {
            pHand.ForEach(c =>
            {
                c.GetComponent<CardDisplay>().helpEnable = true;
                c.GetComponent<CardDisplay>().selectEnable = true;
            });
            playBtn.gameObject.SetActive(true);
            checkdeckBtn.gameObject.SetActive(true);
        }
    }

    public void PlayCardsPhase()
    {
        CardEffectsPhase();
        pGraveyard.AddRange(cSelecting);
        pHand.RemoveAll(c => cSelecting.Contains(c));
        ResetAllCardsStatus();
    }

    public void TurnEndPhase()
    {
        buffs.ForEach(i =>
        {

        });
        showCnt = playedCardShowTime;
        pHand.ForEach(c =>
        {
            CardDisplay cd = c.GetComponent<CardDisplay>();
            if (cd.cid == 13)
            {
                player.GetComponent<PlayerCtrl>().Life += 1;
            }
        });
    }

    public void TurnStart()
    {
        if (cube + CheckCost() < 0)
        {
            se[0].Play();
            lifeFlashCnt = 2f;
            return;
        }
        se[1].Play();
        showing = true;
        playBtn.gameObject.SetActive(false);
        checkdeckBtn.gameObject.SetActive(false);
    }

    private int CheckCost()
    {
        int allcost = 0;
        cSelecting.ForEach(c =>
        {
            CardDisplay cd = c.GetComponent<CardDisplay>();
            allcost += cd.GetCost();
        });
        return allcost;
    }

    public void CheckDeck()
    {
        se[2].Play();
        checkDeckMode = !checkDeckMode;
        if (checkDeckMode)
        {
            checkdeckBtn.GetComponentInChildren<Text>().text = "[  RESUME  ]";
            List<GameObject> cards = new List<GameObject>();
            cards.AddRange(pLibrary);
            cards.AddRange(pGraveyard);
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].SetActive(true);
                cards[i].GetComponent<CardDisplay>().helpEnable = true;
                cards[i].GetComponent<CardDisplay>().selectEnable = false;
                cards[i].GetComponent<CardDisplay>().selected = false;
                cards[i].transform.position = new Vector3(-5f + (i % 8) * 1.5f, 3.5f - ((i / 8) * 2f), -i * 0.002f);
            }
            pHand.ForEach(c =>
            {
                c.GetComponent<CardDisplay>().helpEnable = true;
                c.GetComponent<CardDisplay>().selectEnable = false;
                c.GetComponent<CardDisplay>().selected = false;
            });

            playBtn.gameObject.SetActive(false);
            cSelecting.Clear();
        }
        else
        {
            animeEnd = false;
            checkdeckBtn.GetComponentInChildren<Text>().text = "[  DECK  ]";
            checkdeckBtn.gameObject.SetActive(false);
        }
    }

    public void CardEffectsPhase()
    {
        int lifeplus = 0;
        int atk = -1;

        pHand.ForEach(c =>
        {
            CardDisplay cd = c.GetComponent<CardDisplay>();
            if (cd.cid == 14 && atk == -1)
            {
                atk = 0;
            }
        });
        cSelecting.ForEach(c =>
        {
            CardDisplay cd = c.GetComponent<CardDisplay>();
            lifeplus += cd.GetCost();
            if ((cd.cid == 0 || cd.cid == 16) && atk < 0)
            {
                atk = 0;
            }
            if (cd.cid == 11 && atk == 0)
            {
                atk = 1;
            }
            if (cd.cid == 4 || cd.cid == 15)
            {
                pHand.Remove(c);
                pDeck.Remove(c);
                cSelecting.Remove(c);
                Destroy(c);
            }
            if (cd.cid == 16)
            {
                bomb++;
            }
            if (cd.cid == 7)
            {
                bomb += 3;
            }
            if (cd.cid == 4)
            {
                muteki = true;
            }
        });

        weapon = atk;
        player.GetComponent<PlayerCtrl>().Life += lifeplus;
        player.GetComponent<PlayerCtrl>().ResetCrit();
    }

    private void Shuffle(ref List<GameObject> obj)
    {
        System.Random random = new System.Random();
        List<GameObject> shuffled = new List<GameObject>();
        obj.ForEach(c => shuffled.Insert(random.Next(shuffled.Count + 1), c));
        obj = shuffled;
    }

    private void ResetAllCardsStatus()
    {
        pDeck.ForEach(c =>
        {
            CardDisplay cd = c.GetComponent<CardDisplay>();
            cd.selected = false;
            cd.helpEnable = false;
            cd.selectEnable = false;
        });
        cSelecting.Clear();
    }


    private void RewardProcess()
    {
        if (gamePauseType == 2)
        {
            foreach (int i in reward)
            {
                cReward.Add(Instantiate(cardData.cardDatabase[i]) as GameObject);
            }
            reward = null;
            cReward.ForEach(c =>
            {
                CardDisplay cd = c.GetComponent<CardDisplay>();
                c.transform.localScale = Vector3.zero;
                c.transform.position = rewardPos;
                cd.selectEnable = false;
                cd.helpEnable = false;
            });
            rewardPos = Vector3.zero;
            deckInfo.GetComponent<Text>().color = Color.white;
            lifeInfo.GetComponent<Text>().color = Color.white;
            deckInfo.gameObject.GetComponent<Outline>().enabled = false;
            lifeInfo.gameObject.GetComponent<Outline>().enabled = false;
            Time.timeScale = 0f;
            Color color = menubg.color;
            color.a = 0.5f;
            menubg.color = color;
            checkDeckMode = false;
            gamePauseType++;
            gamePause = true;
            return;
        }

        if (gamePauseType == 3)
        {
            if (cReward.Count > 0)
            {
                if (!rewardShow)
                {
                    for (int i = 0; i < cReward.Count; i++)
                    {
                        rewardShow = true;
                        Vector3 rewardPos = new Vector3((-cReward.Count + 1) * 1.5f + i * 3f, 0f, i * 0.001f);
                        if (Mathf.Abs(rewardPos.x - cReward[i].transform.position.x) > 0.1f || Mathf.Abs(1f - cReward[i].transform.localScale.x) > 0.1f)
                        {
                            cReward[i].transform.position += (rewardPos - cReward[i].transform.position) / 10f;
                            cReward[i].transform.localScale += (Vector3.one - cReward[i].transform.localScale) / 10f;
                            rewardShow = false;
                        }
                    }
                }
                else
                {
                    cReward.ForEach(c =>
                    {
                        CardDisplay cd = c.GetComponent<CardDisplay>();
                        cd.selectEnable = true;
                        cd.helpEnable = true;
                    });

                    if (cSelecting.Count > 0)
                    {
                        se[5].Play();
                        pDeck.Add(cSelecting[0]);
                        pLibrary.Add(cSelecting[0]);
                        cReward.Remove(cSelecting[0]);
                        cReward.ForEach(c => Destroy(c));
                        cReward.Clear();
                        cSelecting[0].GetComponent<CardDisplay>().selected = false;
                        cSelecting[0].GetComponent<CardDisplay>().helpEnable = false;
                        cSelecting[0].GetComponent<CardDisplay>().selectEnable = false;
                    }
                }
            }
            else
            {
                gamePauseType++;
            }
            return;
        }

        if (gamePauseType == 4)
        {
            Vector3 pos = new Vector3(3.8f, -4.5f, 0f);
            if (Mathf.Abs(pos.x - cSelecting[0].transform.position.x) > 0.1f || Mathf.Abs(pos.y - cSelecting[0].transform.position.y) > 0.1f)
            {
                cSelecting[0].transform.position += (pos - cSelecting[0].transform.position) / 10f;
                cSelecting[0].transform.localScale += (Vector3.one / 2 - cSelecting[0].transform.localScale) / 3f;

            }
            else
            {
                deckInfo.GetComponent<Text>().color = Color.black;
                lifeInfo.GetComponent<Text>().color = Color.black;
                deckInfo.gameObject.GetComponent<Outline>().enabled = true;
                lifeInfo.gameObject.GetComponent<Outline>().enabled = true;
                rewardShow = false;
                gamePause = false;
                gamePauseType = 0;
                Time.timeScale = 1f;
                animeEnd = false;
                Color color = menubg.color;
                color.a = 0f;
                menubg.color = color;
                cSelecting.Clear();
            }
            return;
        }
    }


    public void Pause()
    {
        if (gamePause)
        {
            deckInfo.GetComponent<Text>().color = Color.black;
            lifeInfo.GetComponent<Text>().color = Color.black;
            deckInfo.gameObject.GetComponent<Outline>().enabled = true;
            lifeInfo.gameObject.GetComponent<Outline>().enabled = true;
            if (!showingHelp)
            {
                gamePause = false;
                Time.timeScale = 1f;
                animeEnd = false;
                Color color = menubg.color;
                color.a = 0f;
                menubg.color = color;
                switch (gamePauseType)
                {
                    case 0:
                        drawed = false;
                        readyForPlay = false;
                        turn++;
                        turnTimer = turnMaxTime;
                        break;
                    case 1:
                        checkdeckBtn.gameObject.SetActive(false);
                        pGraveyard.ForEach(c => c.transform.position = gravePos);
                        pLibrary.ForEach(c => c.transform.position = libPos);
                        checkDeckMode = false;
                        pHand.ForEach(c => c.GetComponent<CardDisplay>().helpEnable = false);
                        break;
                }
            }
        }
        else
        {
            se[2].Play();
            deckInfo.GetComponent<Text>().color = Color.white;
            lifeInfo.GetComponent<Text>().color = Color.white;
            deckInfo.gameObject.GetComponent<Outline>().enabled = false;
            lifeInfo.gameObject.GetComponent<Outline>().enabled = false;
            gamePause = true;
            Time.timeScale = 0f;
            Color color = menubg.color;
            color.a = 0.5f;
            menubg.color = color;
            checkDeckMode = false;
            if (animeEnd)
            {
                pHand.ForEach(c => c.GetComponent<CardDisplay>().helpEnable = false);
            }
        }
    }

    private void CardBattleSystemReady()
    {
        cardData.LoadCards();
        if (gameMode == 0)
        {
            foreach (int i in tdeck)
            {
                pDeck.Add(Instantiate(cardData.cardDatabase[i]) as GameObject);
            }
        }
        else if (gameMode == 1)
        {
            foreach (int i in ndeck)
            {
                pDeck.Add(Instantiate(cardData.cardDatabase[i]) as GameObject);
            }
        }
        pDeck.ForEach(c =>
        {
            c.transform.localScale = Vector3.one / 2;
            c.transform.position = libPos;
        });
        pLibrary.AddRange(pDeck);
        ResetAllCardsStatus();
    }

    private void StgReady()
    {

    }

    // 卡牌掉落时的处理
    public static void DropPause(int[] drops, Vector3 pos)
    {
        reward = drops;
        rewardPos = pos;
        gamePauseType = 2;
    }

    // 处理玩家被弹和gameover时效果
    private void GameOverAndDamageUpdate()
    {
        if (gameRealOver)
        {
            bgm.volume -= 0.01f;
            deckInfo.SetActive(false);
            lifeInfo.SetActive(false);
            realGameoverCnt -= Time.unscaledDeltaTime;
            Color c = gameoverwhite.color;
            c.a += 0.005f;
            gameoverwhite.color = c;

            if (realGameoverCnt <= 0)
            {
                BackToTitle();
            }
        }

        cube = player.GetComponent<PlayerCtrl>().Life;
        if (lifeFlashCnt > 1f)
        {
            lifeInfo.GetComponent<Text>().color = Color.red;
            lifeFlashCnt -= Time.unscaledDeltaTime;
        }
        else
        {
            lifeFlashCnt = 0f;
            if (gamePause)
            {

                lifeInfo.GetComponent<Text>().color = Color.white;
            }
            else
            {
                lifeInfo.GetComponent<Text>().color = Color.black;
            }
        }
    }

    // 返回标题前处理
    public void BackToTitle()
    {
        // 全局变量重置
        tMode = false;
        gameMode = 0;
        mainTimer = 0f;
        turnMaxTime = 10f;
        turnTimer = turnMaxTime;
        turn = 1;
        scrollSpeedMultiply = 1f;
        gamePause = false;
        gamePauseType = -1;
        showingHelp = false;
        controlable = false;
        gameOver = false;
        gameRealOver = false;
        stgStart = false;
        cube = 1;
        bomb = 0;
        weapon = -1;
        tPhase = 0;
        nSlow = true;
        bossPhase = -1;
        muteki = false;
        activeUnits.Clear();
        pDeck.Clear();
        pHand.Clear();
        pLibrary.Clear();
        pGraveyard.Clear();
        eCards.Clear();
        cSelecting.Clear();
        cReward.Clear();
        Destroy(GameObject.Find("BGM"));
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        realGameoverCnt = 6f;
    }


}

// 敌方使用的武器类型
public enum WeaponType
{
    a00, a01, a02, a03, a04, a05
}


// layer过滤
public enum CanHit : int
{
    Player = 1 << 12 | 1 << 10,
    Enemy = 1 << 12 | 1 << 11,
    Body = 1 << 12 | 1 << 13,
    PlayerAndEnemy = 1 << 12 | 1 << 10 | 1 << 11,
    PlayerAndBody = 1 << 12 | 1 << 10 | 1 << 13,
    EnemyAndBody = 1 << 12 | 1 << 11 | 1 << 13,
    All = 1 << 12 | 1 << 10 | 1 << 11 | 1 << 13
}

