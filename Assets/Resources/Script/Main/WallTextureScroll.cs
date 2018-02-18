using UnityEngine;

/*
 * 两边墙壁动画效果
 */
public class WallTextureScroll : MonoBehaviour
{

    public float spd = 0.5f;            // 速度
    private float accspd = 0f;          // 加速度
    private float bossCount1 = 0f;      // BOSS战演出用
    private float bossCount2 = 0f;      // BOSS战演出用
    public bool isLeft = false;         // 左右区分
    private bool shaked = false;        // BOSS战演出用
    public BattleCamera cam;            // BOSS战摄像机
    public Material mat2;               // BOSS战变更后材质
    private Material m;                 // 自身材质

    void Start()
    {
        m = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // 材质动画
        float offset = Time.time * spd;
        m.SetTextureOffset("BASE", new Vector2(offset, 0));

        // BOSS战特殊演出
        if (BattleMgr.bossPhase == 3)
        {
            if (isLeft)
            {
                transform.position = new Vector3(transform.position.x - 0.05f, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x + 0.05f, transform.position.y, transform.position.z);
            }
            return;
        }
        // BOSS战特殊演出
        if (BattleMgr.bossPhase == 1)
        {
            if (!BattleMgr.gamePause)
            {
                bossCount1 += Time.deltaTime;
            }
            if (bossCount1 > 3f)
            {
                if (isLeft)
                {
                    if (transform.position.x < -10)
                    {
                        transform.position = new Vector3(transform.position.x + accspd, transform.position.y, transform.position.z);
                        accspd += Time.deltaTime / 2;
                    }
                    else
                    {
                        if (!shaked)
                        {
                            GetComponent<Renderer>().material = mat2;
                            cam.ShakeSmall();
                            shaked = true;
                        }
                        accspd = 0;
                    }
                }
                else
                {
                    if (transform.position.x > 10)
                    {
                        transform.position = new Vector3(transform.position.x - accspd, transform.position.y, transform.position.z);
                        accspd += Time.deltaTime / 2;
                    }
                    else
                    {
                        if (!shaked)
                        {
                            GetComponent<Renderer>().material = mat2;
                            cam.ShakeSmall();
                            shaked = true;
                        }
                        accspd = 0;
                    }
                }
            }
        }
        // BOSS战特殊演出
        else if (BattleMgr.bossPhase == 2)
        {
            if (!BattleMgr.gamePause)
            {
                bossCount2 += Time.deltaTime;
            }
            if (bossCount2 > 0.5f)
            {
                if (isLeft)
                {
                    float x = transform.position.x;
                    x += (-15 - x) / 40f;
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                }
                else
                {
                    float x = transform.position.x;
                    x += (15 - x) / 40f;
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                }
            }
        }
    }

    public float GetX()
    {
        return transform.position.x;
    }
}
