using System.Collections.Generic;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem instance;

    [SerializeField] GameObject damageMessage;
    [SerializeField] GameObject damageMessagePlayer;
    [SerializeField] int objectCount;
    [SerializeField] int playerObjectCount;

    [Header("버프 메시지")]
    [SerializeField] GameObject buffMessage;
    [SerializeField] int buffObjectCount = 5;

    [Header("버프 메시지 색상")]
    [SerializeField] Color speedBoostColor = Color.white;
    [SerializeField] Color damageBoostColor = Color.white;
    [SerializeField] Color doubleExpColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] Color doubleCoinColor = new Color(1f, 0.9f, 0.4f);

    int count;
    int countPlayerMessage;
    int countBuffMessage;

    List<GameObject> messagePool = new List<GameObject>();
    List<GameObject> playerMssagePool = new List<GameObject>();
    List<GameObject> buffMessagePool = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < objectCount; i++)
            Populate();

        for (int i = 0; i < playerObjectCount; i++)
            PopulatePlayerMessage();

        for (int i = 0; i < buffObjectCount; i++)
            PopulateBuffMessage();
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

    void PopulateBuffMessage()
    {
        GameObject go = Instantiate(buffMessage, transform);
        buffMessagePool.Add(go);
        go.SetActive(false);
    }

    public void PostMessage(string text, Vector3 worldPosition, bool isCritical)
    {
        TMPro.TextMeshPro damageText = messagePool[count].GetComponentInChildren<TMPro.TextMeshPro>();
        messagePool[count].gameObject.SetActive(true);
        messagePool[count].transform.position = worldPosition;
        damageText.text = text;
        damageText.color = new Color(1, 1, 1, 1);
        damageText.sortingOrder = 50;

        if (isCritical)
        {
            damageText.color = new Color(1, .3f, .3f, 1);
            messagePool[count].GetComponent<DamageMessage>().PlayCriticalDamage();
            damageText.sortingOrder = 51;
        }

        count++;
        if (count >= objectCount)
            count = 0;
    }

    public void PostMessagePlayer(string text)
    {
        TMPro.TextMeshPro damageText = playerMssagePool[countPlayerMessage].GetComponentInChildren<TMPro.TextMeshPro>();
        playerMssagePool[countPlayerMessage].gameObject.SetActive(true);
        playerMssagePool[countPlayerMessage].transform.position = GameManager.instance.player.transform.position;
        damageText.text = text;
        damageText.color = new Color(1, .8f, 0, 1);
        damageText.sortingOrder = 50;

        countPlayerMessage++;
        if (countPlayerMessage >= playerObjectCount)
            countPlayerMessage = 0;
    }

    /// <summary>
    /// 버프 획득 시 플레이어 위치에 메시지 표시
    /// FieldItemEffect.ApplyBuff()에서 호출
    /// </summary>
    public void PostBuffMessage(string text, Color color)
    {
        if (buffMessage == null) return;

        TMPro.TextMeshPro tmp = buffMessagePool[countBuffMessage].GetComponentInChildren<TMPro.TextMeshPro>();
        buffMessagePool[countBuffMessage].gameObject.SetActive(true);
        buffMessagePool[countBuffMessage].transform.position = GameManager.instance.player.transform.position;
        tmp.text = text;
        tmp.color = color;

        countBuffMessage++;
        if (countBuffMessage >= buffObjectCount)
            countBuffMessage = 0;
    }

    /// <summary>
    /// 버프 타입에 맞는 색상 반환
    /// </summary>
    public Color GetBuffColor(FieldBuffType buffType)
    {
        return buffType switch
        {
            FieldBuffType.SpeedBoost  => speedBoostColor,
            FieldBuffType.DamageBoost => damageBoostColor,
            FieldBuffType.DoubleExp   => doubleExpColor,
            FieldBuffType.DoubleCoin  => doubleCoinColor,
            _ => Color.white
        };
    }
}