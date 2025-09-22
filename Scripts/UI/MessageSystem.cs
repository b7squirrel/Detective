using System.Collections.Generic;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem instance;
    [SerializeField] GameObject damageMessage;
    [SerializeField] GameObject damageMessagePlayer;

    [SerializeField] int objectCount;
    [SerializeField] int playerObjectCount;
    int count;
    int countPlayerMessage;
    List<GameObject> messagePool = new List<GameObject>();
    List<GameObject> playerMssagePool = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < objectCount; i++)
        {
            Populate();
        }
        for (int i = 0; i < playerObjectCount; i++)
        {
            PopulatePlayerMessage();
        }
    }

    public void Populate()
    {
        GameObject go = Instantiate(damageMessage, transform);
        messagePool.Add(go);
        go.SetActive(false);
    }
    public void PopulatePlayerMessage()
    {
        GameObject go = Instantiate(damageMessagePlayer, transform);
        playerMssagePool.Add(go);
        go.SetActive(false);
    }

    public void PostMessage(string text, Vector3 worldPosition, bool isCritical)
    {
        TMPro.TextMeshPro damageText = messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>();

        messagePool[count].gameObject.SetActive(true);
        messagePool[count].transform.position = worldPosition;
        damageText.text = text;
        damageText.color = new Color(1, 1, 1, 1); // white
        damageText.sortingOrder = 50;
        if (isCritical)
        {
            damageText.color = new Color(1, .3f, .3f, 1); // red
            messagePool[count].GetComponent<DamageMessage>().PlayCriticalDamage();
            damageText.sortingOrder = 51;
        }
        count++;

        if (count >= objectCount)
        {
            count = 0;
        }
    }

    public void PostMessagePlayer(string text)
    {
        TMPro.TextMeshPro damageText = playerMssagePool[countPlayerMessage].GetComponentInChildren<TMPro.TextMeshPro>();

        playerMssagePool[countPlayerMessage].gameObject.SetActive(true);
        playerMssagePool[countPlayerMessage].transform.position = GameManager.instance.player.transform.position;
        damageText.text = text;
        damageText.color = new Color(1, .8f, 0, 1); // yellow
        damageText.sortingOrder = 50;
        
        countPlayerMessage++;

        if (countPlayerMessage >= playerObjectCount)
        {
            countPlayerMessage = 0;
        }
    }
}
