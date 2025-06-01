using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogGroup : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI[] logs;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (var log in logs)
            if( log != null)
                log.text = string.Empty;
    }

    public void Log(string message)
    {
        if (logs.Length <= 0)
            return;

        for (int i = 0; i< logs.Length - 1; i++)
        {
            logs[i].text = logs[i + 1].text;
        }

        logs[logs.Length - 1].text = message;
    }
}
