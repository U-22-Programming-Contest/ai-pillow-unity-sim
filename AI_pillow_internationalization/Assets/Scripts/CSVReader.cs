using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour
{
    public GameObject headObject;          // ���i���́j�̎Q��

    // �Z���T�[�̈ʒu�iUnity��Inspector�Œ��ړ��͉\�j
    public Vector3 sensor1Position;
    public Vector3 sensor2Position;
    public Vector3 sensor3Position;
    public Vector3 sensor4Position;

    private List<Vector3> headPositions = new List<Vector3>();
    private int currentIndex = 0;

    void Start()
    {
        LoadCSV();
        StartCoroutine(MoveHead());
    }

    private float[] latestPressures = new float[4]; // �ŐV���͒l

    void LoadCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "sensor_data.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV�t�@�C����������܂���: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 5) continue;

            float s1 = float.Parse(values[1]);
            float s2 = float.Parse(values[2]);
            float s3 = float.Parse(values[3]);
            float s4 = float.Parse(values[4]);

            Vector3 p1 = sensor1Position;
            Vector3 p2 = sensor2Position;
            Vector3 p3 = sensor3Position;
            Vector3 p4 = sensor4Position;

            Vector3 weightedPos = (s1 * p1 + s2 * p2 + s3 * p3 + s4 * p4) / (s1 + s2 + s3 + s4 + 0.0001f);
            headPositions.Add(weightedPos);

            // �ŐV�̈��͒l���L�^
            latestPressures = new float[] { s1, s2, s3, s4 };
        }
    }

    // PillowHeatmap����Ăяo���p
    public float[] GetLatestPressures()
    {
        return latestPressures;
    }

    IEnumerator MoveHead()
    {
        while (currentIndex < headPositions.Count)
        {
            Vector3 startPos = headObject.transform.position;
            Vector3 endPos = headPositions[currentIndex];
            float elapsed = 0f;
            float duration = 0.5f; // 0.5�b��1�X�e�b�v���ړ�

            // ���W���Ԃ��ăX���[�Y�Ɉړ�
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                headObject.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            currentIndex++;
        }
    }

}
