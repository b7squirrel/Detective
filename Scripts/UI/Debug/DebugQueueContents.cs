using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugQueueContents : MonoBehaviour
{
    public static DebugQueueContents Instance;
    [SerializeField] TMPro.TextMeshProUGUI text;
    List<string> events = new List<string>();

    private void Awake()
    {
        Instance = this;
    }

    void UpdateQueueText()
    {
        text.text = "Queue Contents:\n";

        foreach (var item in events)
        {
            text.text += $"{item}\n"; // Assuming UIEvent has an 'eventName' property
        }
    }

    public void SetQueueContents(List<string> eventName)
    {
        this.events.Clear();
        this.events.AddRange(eventName);
        UpdateQueueText();
    }
}
