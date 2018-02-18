using System.Collections.Generic;
using UnityEngine;

/*
 * 自机攻击光束管理器
 */
public class LaserMgr : MonoBehaviour
{
    [SerializeField]
    private int testType = 0;               // 类型
    [SerializeField]
    private LineRenderer lrSample;          // 预制好参数的LineRenderer
    [SerializeField]
    private Transform start;                // 起始点
    [SerializeField]
    private Transform end;                  // 结束点

    [Space]
    [Header("Name")]
    [SerializeField]
    private string[] lrName;                // 自机武器名列表
    [Header("Width")]
    [SerializeField]
    private float[] lrWidth;                // 宽度列表
    [Header("Material")]
    [SerializeField]
    Material[] lrMat;                       // 材质表
    [Header("Start Effect")]
    [SerializeField]
    ParticleSystem[] lrStartEffect;         // 发射时粒子特效表
    [Header("End Effect")]
    [SerializeField]
    ParticleSystem[] lrEndEffect;           // 击中时粒子特效表
    [Header("Master Spark")]
    [SerializeField]
    bool[] isBeam;                          // 是否为贯通攻击

    private List<LineRenderer> lrList = new List<LineRenderer>();                               // 已经发射的line
    private List<ParticleSystem> prList = new List<ParticleSystem>();                           // 已生成的粒子特效
    private Dictionary<int, GameObject> lrFollowStart = new Dictionary<int, GameObject>();      // 起始点物体列表
    private Dictionary<int, GameObject> lrFollowEnd = new Dictionary<int, GameObject>();        // 结束点物体列表
    private Dictionary<int, CanHit> lrCanHit = new Dictionary<int, CanHit>();                   // 可命中的layer表

    void Update()
    {
        if (!BattleMgr.gamePause)
        {
            LaserAnime();
            prList.ForEach(ps =>
            {
                // 粒子动画结束后自毁
                if (!ps.IsAlive())
                {
                    Destroy(ps.gameObject);
                    prList.Remove(ps);
                }
            });
        }
    }

    // 光束动画
    private void LaserAnime()
    {

        // 调整活动中光束的粗细和shader等
        lrList.ForEach(lr =>
        {
            int id = lr.gameObject.GetInstanceID();
            Vector3 antiBug = lr.GetPosition(0);
            antiBug.z += 0.001f;
            lr.SetPosition(0, antiBug);
            switch (lr.name)
            {
                case "Laser0":
                    lr.startWidth -= 0.005f;
                    lr.endWidth -= 0.005f;
                    lr.SetPosition(0, lrFollowStart[id].transform.position);
                    break;
                case "Laser1":
                    lr.startWidth -= 0.02f;
                    lr.endWidth -= 0.02f;
                    Color c = lr.startColor;
                    c.a -= 0.1f;
                    lr.startColor = c;
                    lr.endColor = c;
                    Vector2 offset = lr.material.mainTextureOffset;
                    offset.x -= Time.deltaTime / 10;
                    lr.material.mainTextureOffset = offset;
                    break;
                default:
                    Destroy(lr.gameObject);
                    lrList.Remove(lr);
                    break;
            }
        });

        // 动画结束后自毁
        lrList.ForEach(lr =>
        {
            int id = lr.gameObject.GetInstanceID();
            switch (lr.name)
            {
                case "Laser0":
                    if (lr.startWidth <= 0)
                    {
                        Destroy(lr.gameObject);
                        lrList.Remove(lr);
                        lrFollowStart.Remove(id);
                    }
                    break;
                case "Laser1":
                    if (lr.startColor.a <= 0)
                    {
                        Destroy(lr.gameObject);
                        lrList.Remove(lr);
                    }
                    break;
                default:
                    Destroy(lr.gameObject);
                    lrList.Remove(lr);
                    break;
            }
        });
    }

    // 生成新光束攻击
    public void ReqLaserAttack(int type, GameObject startObj, GameObject endObj, CanHit canhit, int dmg)
    {
        Vector3 startPos = startObj.transform.position;
        Vector3 endPos = endObj.transform.position;
        LineRenderer lr = Instantiate(lrSample) as LineRenderer;
        int id = lr.gameObject.GetInstanceID();
        lr.material = lrMat[type];
        lr.name = "Laser" + type;
        lr.startWidth = lrWidth[type];
        lr.endWidth = lrWidth[type];
        lrCanHit.Add(id, canhit);
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);

        int mask = (int)CanHit.All;
        switch (type)
        {
            // 使用linecast判定命中的普通攻击
            case 0:
                lrFollowStart.Add(id, startObj);
                RaycastHit hit;
                if (lrCanHit.ContainsKey(id))
                {
                    mask = (int)lrCanHit[id];
                }
                if (mask != 1 << 2)
                {
                    if (Physics.Linecast(start.position, end.position, out hit, mask))
                    {

                        if (hit.collider)
                        {
                            lr.SetPosition(1, hit.point);
                            hit.transform.gameObject.GetComponent<Unit>().Hp -= dmg;
                        }
                    }
                }

                // 如果存在攻击和命中粒子特效则生成
                if (lrStartEffect[type] != null)
                {
                    ParticleSystem sps = Instantiate(lrStartEffect[type], lr.GetPosition(0), Quaternion.identity) as ParticleSystem;
                    sps.gameObject.SetActive(true);
                    prList.Add(sps);
                }
                if (lrEndEffect[type] != null)
                {
                    ParticleSystem eps = Instantiate(lrEndEffect[type], lr.GetPosition(1), Quaternion.identity) as ParticleSystem;
                    eps.gameObject.SetActive(true);
                    var col = eps.collision;
                    col.collidesWith = mask;
                    prList.Add(eps);
                }
                break;

            // 使用raycastall判定多个命中目标的贯通攻击
            case 1:
                lr.textureMode = LineTextureMode.Tile;
                Vector2 offset = lr.material.mainTextureOffset;
                offset.x = Random.Range(0f, 1f);
                lr.material.mainTextureOffset = offset;

                RaycastHit[] hits;
                hits = Physics.RaycastAll(start.position, transform.up, 15f, mask);

                foreach (RaycastHit everyhit in hits)
                {
                    if (everyhit.collider && everyhit.transform.gameObject.GetComponent<Unit>() != null)
                    {
                        everyhit.transform.gameObject.GetComponent<Unit>().Hp -= dmg;
                        if (lrEndEffect[type] != null)
                        {
                            ParticleSystem eps = Instantiate(lrEndEffect[type], everyhit.point, Quaternion.identity) as ParticleSystem;
                            eps.gameObject.SetActive(true);
                            var col = eps.collision;
                            col.collidesWith = mask;
                            prList.Add(eps);
                        }

                    }
                }

                // 调整显示坐标
                Vector3 shiftStart = lr.GetPosition(0);
                Vector3 shiftEnd = lr.GetPosition(1);
                shiftStart.z -= 5f;
                shiftEnd.z -= 5f;
                lr.SetPosition(0, shiftStart);
                lr.SetPosition(1, shiftEnd);

                break;
        }
        // 将新生成的光束加入列表
        lrList.Add(lr);
    }
}

