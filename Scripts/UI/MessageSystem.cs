using System.Collections.Generic;
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

    public void PostMessage(string text, Vector3 worldPosition, bool isCritical)
    {
        messagePool[count].gameObject.SetActive(true);
        messagePool[count].transform.position = worldPosition;
        messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().text = text;
        messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(1, .8f, 0, 1); // yellow
        messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().sortingOrder = 50;
        if (isCritical)
        {
            messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(1, .3f, .3f, 1); // red
            messagePool[count].GetComponent<DamageMessage>().PlayCriticalDamage();
            messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>().sortingOrder = 51;
        }
        count++;

        if (count >= objectCount)
        {
            count = 0;
        }
    }
}
