using UnityEngine;

/*
 * 粒子系统弹幕与碰撞处理
 */
public class Danmaku : MonoBehaviour
{
    public int type = 0;                    // 类型
    private int pcnt = 0;                   // 记录粒子数量
    private AudioSource se;                 // 自带音效
    private ParticleSystem p;
    private ParticleSystem.MainModule ps;

    void Start()
    {
        p = GetComponent<ParticleSystem>();
        ps = GetComponent<ParticleSystem>().main;
        se = GetComponent<AudioSource>();
    }

    // 粒子碰撞到自机时造成伤害
    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerCtrl>().Damage();
        }
    }

    void Update()
    {
        // BOSS击破后取消所有处理
        if(BattleMgr.bossPhase >= 3)
        {
            return;
        }

        // 炸出子粒子时的音效
        if (type == 1 && p.particleCount > pcnt)
        {
            se.Play();
        }
        pcnt = p.particleCount;

        // 暂停时处理
        if (BattleMgr.gamePause)
        {
            ps.simulationSpeed = 0;
        }
        else
        {
            ps.simulationSpeed = 1;
        }

        // 自毁
        if (transform.parent == null)
        {
            if (!GetComponent<ParticleSystem>().IsAlive(true))
            {
                Destroy(gameObject);
            }
        }
    }

}
