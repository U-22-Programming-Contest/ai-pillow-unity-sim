using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour
{
    void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "sensor_data.csv");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                Debug.Log(line);
            }
        }
        else
        {
            Debug.LogError("CSV�t�@�C����������܂���: " + filePath);
        }
    }
}
