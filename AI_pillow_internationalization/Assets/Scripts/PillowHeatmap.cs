using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillowHeatmap : MonoBehaviour
{
    public CSVReader csvReader; // CSVReaderへの参照（Inspectorで設定）
    public Gradient heatGradient; // 圧力に応じた色のグラデーション
    public MeshRenderer pillowRenderer; // 枕のMeshRenderer（Inspectorで設定）

    private float maxPressure = 100f; // 圧力の最大値（適宜調整）

    void Start()
    {
        if (csvReader == null || pillowRenderer == null)
        {
            Debug.LogError("CSVReader または MeshRenderer が未設定です。");
            enabled = false;
            return;
        }

        StartCoroutine(UpdateHeatmap());
    }

    IEnumerator UpdateHeatmap()
    {
        while (true)
        {
            float[] pressures = csvReader.GetLatestPressures(); // 最新の圧力値取得
            if (pressures.Length >= 4)
            {
                // 4点の平均圧力を使って色を決定（後で改良可能）
                float avg = (pressures[0] + pressures[1] + pressures[2] + pressures[3]) / 4f;
                float normalized = Mathf.Clamp01(avg / maxPressure);
                Color heatColor = heatGradient.Evaluate(normalized);

                pillowRenderer.material.color = heatColor;
            }

            yield return new WaitForSeconds(0.5f); // 0.5秒ごとに更新
        }
    }
}
