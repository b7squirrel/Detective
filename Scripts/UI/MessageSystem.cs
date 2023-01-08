using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem instance;
    [SerializeField] GameObject damageMessage;

    [SerializeField] int objectCount;
    int count;
    List<GameObject> messagePool = new List<GameObject>();

    void Awake()
    {
        instance= this;
    }

    void Start()
    {
        for (int i = 0; i < objectCount; i++)
        {
            Populate();
        }
    }

    public void Populate()
    {
        GameObject go = Instantiate(damageMessage, transform);
        messagePool.Add(go);
        go.SetActive(false);
    }

    public void PostMessage(string text, Vector3 worldPosition)
    {
        messagePool[count].gameObject.SetActive(true);
        messagePool[count].transform.position = worldPosition;
        messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().text = text;
        count++;

        if (count >= objectCount)
        {
            count = 0;
        }
    }
}
