using UnityEngine;
using UnityEngine.UI;

/*
 * 额外信息
 */
public class DebugText : MonoBehaviour
{
    public Text timer;          // 总时间
    public Text turntimer;      // 回合剩余时间
    public Image[] imgs;        // 教程图片

    bool s1 = false;
    bool s2 = false;

    void Update()
    {
        timer.text = "" + System.Math.Round(BattleMgr.mainTimer, 3);
        turntimer.text = "Turn " + BattleMgr.turn + " (" + System.Math.Round(BattleMgr.turnTimer, 1) + "″)" +
            "\r\nBOMB : " + BattleMgr.bomb;
        if (BattleMgr.tMode) TutorialUpdate();
    }

    private void TutorialUpdate()
    {
        if (BattleMgr.mainTimer > 0f && BattleMgr.mainTimer < 1f)
        {
            var c = imgs[0].color;
            c.a += 0.05f;
            imgs[0].color = c;
            imgs[1].color = c;
        }
        else if (BattleMgr.mainTimer > 3f && BattleMgr.mainTimer < 5f)
        {
            var c = imgs[0].color;
            c.a -= 0.05f;
            imgs[0].color = c;
            imgs[1].color = c;
        }
        else if (BattleMgr.mainTimer > 7f && BattleMgr.mainTimer < 8f)
        {
            if (!s1)
            {
                s1 = true;
                BattleMgr.DropPause(new int[] { 0 }, new Vector3(0f, 5f, 0f));
            }
            if (BattleMgr.pDeck.Count <= 0)
            {
                if (BattleMgr.showingHelp)
                {
                    Color c = new Color(1, 1, 1, 0);
                    imgs[2].color = c;
                    imgs[3].color = c;
                }
                else
                {
                    Color c = new Color(1, 1, 1, 1);
                    imgs[2].color = c;
                    imgs[3].color = c;
                }
            }
            else
            {
                Color c = new Color(1, 1, 1, 0);
                imgs[2].color = c;
                imgs[3].color = c;
            }
        }
        else if (BattleMgr.mainTimer >= 10f && BattleMgr.mainTimer < 10.3f)
        {

            BattleMgr.turnMaxTime = 20f;
            if (BattleMgr.gamePause)
            {
                if (BattleMgr.showingHelp || BattleMgr.cSelecting.Count > 0)
                {
                    Color c = Color.yellow;
                    c.a = 0;
                    imgs[4].color = c;
                    imgs[5].color = c;
                }
                else
                {
                    Color c = Color.yellow;
                    c.a = 1;
                    imgs[4].color = c;
                    imgs[5].color = c;
                }
            }
            else
            {
                BattleMgr.weapon = 0;
                BattleMgr.playerHandLimit = 3;
                Color c = Color.yellow;
                c.a = 0;
                imgs[4].color = c;
                imgs[5].color = c;
            }
        }
        else if (BattleMgr.mainTimer > 10.5f && BattleMgr.mainTimer < 12f)
        {
            var c = imgs[6].color;
            c.a += 0.05f;
            imgs[6].color = c;
        }
        else if (BattleMgr.mainTimer > 14f && BattleMgr.mainTimer < 16f)
        {
            var c = imgs[6].color;
            c.a -= 0.05f;
            imgs[6].color = c;
        }
        else if (BattleMgr.mainTimer > 28f && BattleMgr.mainTimer < 29f && !BattleMgr.gamePause)
        {
            Color c = Color.white;
            c.a = 0;
            imgs[0].color = c;
            imgs[1].color = c;
            imgs[2].color = c;
            imgs[3].color = c;
            imgs[4].color = c;
            imgs[5].color = c;
            imgs[6].color = c;
            if (BattleMgr.activeUnits.Count > 0)
            {
                BattleMgr.mainTimer -= Time.deltaTime;
                BattleMgr.turnTimer += Time.deltaTime;
            }
        }
        if (BattleMgr.turn == 4 && BattleMgr.turnTimer <= 0)
        {
            BattleMgr.turnMaxTime = 40f;
        }
    }
}
