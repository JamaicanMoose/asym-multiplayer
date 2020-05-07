using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TTSDefaultIP : MonoBehaviour
{

    private static string path;
    InputField ipField;

    private void Awake()
    {
        path = Path.Combine(Application.persistentDataPath, "lastIP.conf");
        ipField = GameObject.Find("IpField").GetComponent<InputField>();
        if (File.Exists(path))
        {
            Debug.Log(File.ReadAllText(path));
            ipField.text = File.ReadAllText(path);
        }
    }

    public void UpdateConf()
    {
        File.WriteAllText(path, ipField.text);
    }
}
