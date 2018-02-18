using UnityEngine;

/*
 * 标题背景方块效果
 */
public class TitleBlockEffect : MonoBehaviour {

    private Material m;         // 方块材质
    public float spd = 0.5F;    // 速度
    public float pos = 0.5F;    // 位置偏移
    [Range(0F, 1F)]
    public float opacity = 1F;  // 不透明度

    void Start () {
        m = GetComponent<Renderer>().material;
    }
	
	void Update () {
        // 直接设置材质
        float offset = Time.unscaledTime * spd;
        m.SetTextureOffset("_MainTex", new Vector2(offset, pos));
        m.SetColor("_TintColor", new Color(0, 0, 0, opacity));
    }
}
