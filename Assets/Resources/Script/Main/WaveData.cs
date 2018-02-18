using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌方出生数据
public class WaveData : MonoBehaviour
{

    public int turn;
    public bool turnBased;
    [Space]
    public float spawnTime;
    [Space]
    [SerializeField]
    public List<GameObject> waveUnits;
    public bool boss;
    private bool start;
    private float bossStartTime;

    private bool spawned = false;
    private float startTime = 0f;
    public AudioSource waveSound;

    void Start()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform c in children)
        {
            if (c.gameObject.tag == "EnemyUnit")
            {
                waveUnits.Add(c.gameObject);
                c.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (boss && !start && BattleMgr.bossPhase == 1)
        {
            start = true;
            bossStartTime = BattleMgr.mainTimer;
        }
        if (!spawned)
        {
            if (turnBased)
            {

            }
            else
            {
                if (boss)
                {
                    if (BattleMgr.bossPhase == 1 && start && BattleMgr.mainTimer - bossStartTime > spawnTime)
                    {
                        startTime = BattleMgr.mainTimer;
                        spawned = true;
                    }
                }
                else
                {
                    if (BattleMgr.mainTimer > spawnTime)
                    {
                        startTime = BattleMgr.mainTimer;
                        spawned = true;
                    }
                }
            }
        }
        else
        {
            waveUnits.ForEach(obj =>
            {
                if (!obj.activeSelf && BattleMgr.mainTimer - startTime > float.Parse(obj.name))
                {
                    if (boss)
                    {
                        waveSound.Play();
                    }
                    obj.SetActive(true);
                    BattleMgr.activeUnits.Add(obj);
                    waveUnits.Remove(obj);
                }
            });

        }


    }

}
