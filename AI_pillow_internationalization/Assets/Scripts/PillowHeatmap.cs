using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillowHeatmap : MonoBehaviour
{
    public CSVReader csvReader; // CSVReader�ւ̎Q�ƁiInspector�Őݒ�j
    public Gradient heatGradient; // ���͂ɉ������F�̃O���f�[�V����
    public MeshRenderer pillowRenderer; // ����MeshRenderer�iInspector�Őݒ�j

    private float maxPressure = 100f; // ���͂̍ő�l�i�K�X�����j

    void Start()
    {
        if (csvReader == null || pillowRenderer == null)
        {
            Debug.LogError("CSVReader �܂��� MeshRenderer �����ݒ�ł��B");
            enabled = false;
            return;
        }

        StartCoroutine(UpdateHeatmap());
    }

    IEnumerator UpdateHeatmap()
    {
        while (true)
        {
            float[] pressures = csvReader.GetLatestPressures(); // �ŐV�̈��͒l�擾
            if (pressures.Length >= 4)
            {
                // 4�_�̕��ψ��͂��g���ĐF������i��ŉ��ǉ\�j
                float avg = (pressures[0] + pressures[1] + pressures[2] + pressures[3]) / 4f;
                float normalized = Mathf.Clamp01(avg / maxPressure);
                Color heatColor = heatGradient.Evaluate(normalized);

                pillowRenderer.material.color = heatColor;
            }

            yield return new WaitForSeconds(0.5f); // 0.5�b���ƂɍX�V
        }
    }
}
