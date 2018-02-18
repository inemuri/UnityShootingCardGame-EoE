using UnityEngine;

/*
 * 音乐循环
 */
public class BgmLoop : MonoBehaviour
{
    [SerializeField]
    private float start = 0;        // 起始时间（调整用）
    [SerializeField]
    private float now;              // 当前时间（调整用）
    [SerializeField]
    private float loopPoint = 0;    // 循环起始点
    [SerializeField]
    private float loopEnd = 0;      // 循环结束

    private AudioSource source;


    void Start()
    {
        source = GetComponent<AudioSource>();
        source.time = start;
    }

    void Update()
    {
        now = source.time;
        if (source.time >= loopEnd) source.time = loopPoint;
    }
}
