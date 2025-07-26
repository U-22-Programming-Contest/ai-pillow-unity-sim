using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour
{
    public GameObject headObject;

    public PillowHeatmapTexture pillowHeatmap;  // �q�[�g�}�b�v�\���X�N���v�g
    public Transform planeTransform;           // ���̐e�I�u�W�F�N�g�i��: pillowRoot�j
    public Vector2 pillowSize = new Vector2(0.5f, 0.3f);  // �����T�C�Y�i�����擾����̂�Inspector���͕͂s�v�j

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
        // 1�t���[���҂��Ƃő��I�u�W�F�N�g�̏�������ۏ�
        yield return null;

        // planeTransform�ɃA�^�b�`����Ă���Renderer�R���|�[�l���g���擾
        Renderer pillowRenderer = planeTransform.GetComponent<Renderer>();

        if (pillowRenderer != null)
        {
            // ������ �C���_ ������
            // localScale�̑���ɁARenderer�̃o�E���f�B���O�{�b�N�X�ibounds�j����
            // ���[���h��Ԃł̐��m�ȃT�C�Y���擾���܂��B
            Vector3 worldSize = pillowRenderer.bounds.size;
            pillowSize = new Vector2(worldSize.x, worldSize.z);
        }
        else
        {
            // ����Renderer��������Ȃ������ꍇ�̃G���[����
            Debug.LogError("planeTransform��Renderer�R���|�[�l���g��������܂���B�T�C�Y�𐳂����擾�ł��܂���B");
            // �]���̕��@�ɖ߂�
            pillowSize = new Vector2(planeTransform.localScale.x, planeTransform.localScale.z);
        }

        // �擾�����T�C�Y�����O�Ŋm�F
        Debug.Log($"���̎��ۂ̃T�C�Y�����o: Width={pillowSize.x}, Depth={pillowSize.y}");

        // CSV��ǂݍ���ŃV�~�����[�V�������J�n
        LoadCSV();
        StartCoroutine(MoveHead());
    }

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

            // UV���W�ɕϊ�
            Vector3 localPos = planeTransform.InverseTransformPoint(weightedPos);
            float uvX = (localPos.x / pillowSize.x) + 0.5f;
            float uvY = (localPos.z / pillowSize.y) + 0.5f;
            // �␳
            uvX = Mathf.Clamp01(uvX);
            uvY = Mathf.Clamp01(uvY);
            Vector2 uvCenter = new Vector2(uvX, uvY);

            // ���񂾂��q�[�g�}�b�v�X�V
            if (i == 1 && pillowHeatmap != null)
            {
                pillowHeatmap.SetCenter(uvCenter);
            }

            // sleep_posture�Ɋ�Â���]
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

        // ���[�J�����S����̈ʒu��[0,1]�ɐ��K��
        float uvX = (localPos.x / pillowSize.x) + 0.5f;
        float uvY = (localPos.z / pillowSize.y) + 0.5f;

        // �I�[�o�[�t���[�΍�
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

                // �q�[�g�}�b�v�X�V�𖈃t���[���s��
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
