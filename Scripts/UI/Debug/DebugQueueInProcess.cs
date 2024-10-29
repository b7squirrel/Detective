using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugQueueInProcess : MonoBehaviour
{
    public static DebugQueueInProcess Instance;
    [SerializeField] TMPro.TextMeshProUGUI text;

    private void Awake()
    {
        Instance = this;
    }
    public void SetInProcess()
    {
        text.text = "Queue In Process";
    }
    public void SetDone()
    {
        text.text = "Queue Done";
    }
}
