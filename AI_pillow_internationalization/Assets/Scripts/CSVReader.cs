using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour
{
    public GameObject headObject;

    public PillowHeatmapTexture pillowHeatmap;  // ヒートマップ表示スクリプト
    public Transform planeTransform;           // 枕の親オブジェクト（例: pillowRoot）
    public Vector2 pillowSize = new Vector2(0.5f, 0.3f);  // 実寸サイズ（自動取得するのでInspector入力は不要）

    public Vector3 sensor1Position;
    public Vector3 sensor2Position;
    public Vector3 sensor3Position;
    public Vector3 sensor4Position;

    private List<Vector3> headPositions = new List<Vector3>();
    private List<Quaternion> headRotations = new List<Quaternion>();
    private int currentIndex = 0;

    private float[] latestPressures = new float[4];

    IEnumerator Start()
    {
        // 1フレーム待つことで他オブジェクトの初期化を保証
        yield return null;

        // planeTransformにアタッチされているRendererコンポーネントを取得
        Renderer pillowRenderer = planeTransform.GetComponent<Renderer>();

        if (pillowRenderer != null)
        {
            // ★★★ 修正点 ★★★
            // localScaleの代わりに、Rendererのバウンディングボックス（bounds）から
            // ワールド空間での正確なサイズを取得します。
            Vector3 worldSize = pillowRenderer.bounds.size;
            pillowSize = new Vector2(worldSize.x, worldSize.z);
        }
        else
        {
            // もしRendererが見つからなかった場合のエラー処理
            Debug.LogError("planeTransformにRendererコンポーネントが見つかりません。サイズを正しく取得できません。");
            // 従来の方法に戻す
            pillowSize = new Vector2(planeTransform.localScale.x, planeTransform.localScale.z);
        }

        // 取得したサイズをログで確認
        Debug.Log($"枕の実際のサイズを検出: Width={pillowSize.x}, Depth={pillowSize.y}");

        // CSVを読み込んでシミュレーションを開始
        LoadCSV();
        StartCoroutine(MoveHead());
    }

    void LoadCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "sensor_data.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSVファイルが見つかりません: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 6) continue;

            float s1 = float.Parse(values[1]);
            float s2 = float.Parse(values[2]);
            float s3 = float.Parse(values[3]);
            float s4 = float.Parse(values[4]);
            string posture = values[5].Trim();

            Vector3 p1 = sensor1Position;
            Vector3 p2 = sensor2Position;
            Vector3 p3 = sensor3Position;
            Vector3 p4 = sensor4Position;

            Vector3 weightedPos = (s1 * p1 + s2 * p2 + s3 * p3 + s4 * p4) / (s1 + s2 + s3 + s4 + 0.0001f);
            headPositions.Add(weightedPos);

            // UV座標に変換
            Vector3 localPos = planeTransform.InverseTransformPoint(weightedPos);
            float uvX = (localPos.x / pillowSize.x) + 0.5f;
            float uvY = (localPos.z / pillowSize.y) + 0.5f;
            // 補正
            uvX = Mathf.Clamp01(uvX);
            uvY = Mathf.Clamp01(uvY);
            Vector2 uvCenter = new Vector2(uvX, uvY);

            // 初回だけヒートマップ更新
            if (i == 1 && pillowHeatmap != null)
            {
                pillowHeatmap.SetCenter(uvCenter);
            }

            // sleep_postureに基づく回転
            Quaternion baseRotation = Quaternion.Euler(90f, 0f, 0f);
            Quaternion rotation = baseRotation;

            switch (posture)
            {
                case "ue":
                    rotation = baseRotation;
                    break;
                case "migi":
                    rotation = baseRotation * Quaternion.Euler(0f, 90f, 0f);
                    break;
                case "hidari":
                    rotation = baseRotation * Quaternion.Euler(0f, -90f, 0f);
                    break;
            }

            headRotations.Add(rotation);
            latestPressures = new float[] { s1, s2, s3, s4 };
        }
    }

    public float[] GetLatestPressures()
    {
        return latestPressures;
    }

    private Vector2 ConvertHeadPositionToUV(Vector3 headWorldPos)
    {
        Vector3 localPos = planeTransform.InverseTransformPoint(headWorldPos);

        // ローカル中心からの位置を[0,1]に正規化
        float uvX = (localPos.x / pillowSize.x) + 0.5f;
        float uvY = (localPos.z / pillowSize.y) + 0.5f;

        // オーバーフロー対策
        uvX = Mathf.Clamp01(uvX);
        uvY = Mathf.Clamp01(uvY);

        Debug.Log($"localPos = {localPos}, uv = ({uvX}, {uvY}), pillowSize = {pillowSize}");
        return new Vector2(uvX, uvY);
    }


    IEnumerator MoveHead()
    {
        while (currentIndex < headPositions.Count)
        {
            Vector3 startPos = headObject.transform.position;
            Vector3 endPos = headPositions[currentIndex];

            Quaternion startRot = headObject.transform.rotation;
            Quaternion endRot = headRotations[currentIndex];

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                headObject.transform.position = Vector3.Lerp(startPos, endPos, t);
                headObject.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

                // ヒートマップ更新を毎フレーム行う
                if (pillowHeatmap != null)
                {
                    Vector2 uvCenter = ConvertHeadPositionToUV(headObject.transform.position);
                    pillowHeatmap.SetCenter(uvCenter);
                }

                yield return null;
            }


            currentIndex++;
        }
    }
}
