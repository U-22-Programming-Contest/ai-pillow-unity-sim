using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour
{
    public GameObject headObject; // 頭（球体）の参照
    private List<Vector3> headPositions = new List<Vector3>();
    private int currentIndex = 0;

    void Start()
    {
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
        for (int i = 1; i < lines.Length; i++) // 1行目はヘッダー
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 5) continue;

            float s1 = float.Parse(values[1]);
            float s2 = float.Parse(values[2]);
            float s3 = float.Parse(values[3]);
            float s4 = float.Parse(values[4]);

            // センサー位置ベクトル
            Vector3 p1 = new Vector3(-4f, 0, 4f);
            Vector3 p2 = new Vector3(4f, 0, 4f);
            Vector3 p3 = new Vector3(-4f, 0, -4f);
            Vector3 p4 = new Vector3(4f, 0, -4f);


            // 重心的に位置を推定
            Vector3 weightedPos = (s1 * p1 + s2 * p2 + s3 * p3 + s4 * p4) / (s1 + s2 + s3 + s4 + 0.0001f);
            headPositions.Add(weightedPos);
        }
    }

    IEnumerator MoveHead()
    {
        while (currentIndex < headPositions.Count)
        {
            headObject.transform.localPosition = headPositions[currentIndex];
            currentIndex++;
            yield return new WaitForSeconds(0.5f); // 0.5秒ごとに更新
        }
    }
}
