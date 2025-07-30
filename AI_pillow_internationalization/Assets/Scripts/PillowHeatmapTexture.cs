using UnityEngine;

public class PillowHeatmapTexture : MonoBehaviour
{
    public Renderer pillowRenderer; // 枕のRenderer
    public int textureSize = 20;   // テクスチャ解像度，増加でより鮮明化
    public Vector2 uvCenter;        // ヒートの中心 (UV 0~1)
    public float heatRadius = 0.5f; // ヒートの影響範囲 (0~1のUV距離)

    private Texture2D heatmapTex;
    public Transform pillowTransform;
    private Vector2 pillowSize;

    void Start()
    {
        heatmapTex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        heatmapTex.wrapMode = TextureWrapMode.Clamp;
        pillowRenderer.material.mainTexture = heatmapTex;
        Vector3 localScale = pillowTransform.localScale;
        pillowSize = new Vector2(localScale.x, localScale.z);  // 横幅と奥行き


        UpdateHeatmapTexture();
    }

    public void SetCenter(Vector2 newCenter)
    {
        // 左右・上下反転
        uvCenter = new Vector2(1.0f - newCenter.x, 1.0f - newCenter.y);
        uvCenter.x = Mathf.Clamp01(uvCenter.x);
        uvCenter.y = Mathf.Clamp01(uvCenter.y);

        if (heatmapTex == null)
        {
            Debug.LogWarning("Heatmap texture is not initialized.");
            return;
        }

        UpdateHeatmapTexture();


    }

    void UpdateHeatmapTexture()
    {
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 uv = new Vector2((float)x / textureSize, (float)y / textureSize);
                float dist = Vector2.Distance(uv, uvCenter);
                float t = Mathf.Clamp01(1 - dist / heatRadius);

                Color col = GetThermalColor(t);
                heatmapTex.SetPixel(x, y, col);
            }
        }

        heatmapTex.Apply();
    }

    Color GetThermalColor(float t)
    {
        // 赤→オレンジ→黄→緑→青の簡易グラデーション
        if (t > 0.8f) return Color.red;
        else if (t > 0.6f) return new Color(1f, 0.5f, 0f); // オレンジ
        else if (t > 0.4f) return Color.yellow;
        else if (t > 0.2f) return Color.green;
        else return Color.blue;
    }
}
