using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GachaField : MonoBehaviour
{
    #region 참조 변수
    CardDataManager cardDataManager => CardDataManager.Instance;
    [SerializeField] SetCardDataOnSlot displayCardOnSlot;
    #endregion

    #region 슬롯 생성 관련 변수
    int numSlots;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Slots slotType;
    [SerializeField] Vector2 slotSize;
    #endregion

    #region 연출 관련 변수
    [Header("팝업 연출")]
    [SerializeField] float popInterval = 0.08f;      // 카드 간 팝 간격
    [SerializeField] float popDuration = 0.3f;        // 팝 애니메이션 길이
    [SerializeField] float lastCardExtraDelay = 0.5f; // 마지막 카드 추가 딜레이
    [SerializeField] Ease popEase = Ease.OutBack;

    Sequence popSequence;
    #endregion

    void OnDisable()
    {
        popSequence?.Kill();
        ClearSlots();
    }

    #region Refresh
    public void GenerateAllCardsOfType(List<CardData> cardList)
    {
        List<CardData> cardDatas = new();
        List<GameObject> slots = new();

        cardDatas.AddRange(cardList);
        numSlots = cardDatas.Count;

        Debug.Log($"[GachaField] GenerateAllCardsOfType 시작. numSlots={numSlots}");

        // 슬롯 생성
        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.zero;
            slots.Add(slot);

            Debug.Log($"[GachaField] 슬롯 {i} 생성 직후 localScale = {slot.transform.localScale}, activeInHierarchy = {slot.activeInHierarchy}");
        }

        // 카드 데이터 정렬
        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);
        cardDataSorted.Sort((a, b) => new Sort().ByGrade(a, b));

        // 카드 Display
        for (int i = 0; i < numSlots; i++)
        {
            displayCardOnSlot.PutCardDataIntoSlot(cardDataSorted[i], slots[i].GetComponent<CardSlot>());
            Debug.Log($"[GachaField] 슬롯 {i} PutCardDataIntoSlot 이후 localScale = {slots[i].transform.localScale}");
        }

        PlayPopSequence(slots);
    }

    void PlayPopSequence(List<GameObject> slots)
    {
        Debug.Log($"[GachaField] PlayPopSequence 시작. this.transform.localScale={transform.localScale}, this.transform.lossyScale={transform.lossyScale}");
        Debug.Log($"[GachaField] PlayPopSequence 시작. slots.Count={slots.Count}, this.activeInHierarchy={gameObject.activeInHierarchy}, timeScale={Time.timeScale}");

        popSequence?.Kill();
        popSequence = DOTween.Sequence();

        for (int i = 0; i < slots.Count; i++)
        {
            float delay = i * popInterval;
            if (i == slots.Count - 1)
            {
                delay += lastCardExtraDelay;
            }

            int index = i; // 클로저 캡처용
            Transform slotTransform = slots[i].transform;

            Debug.Log($"[GachaField] 슬롯 {index} 트윈 예약. delay={delay}, 현재 scale={slotTransform.localScale}, target={slotSize}, targetActive={slotTransform.gameObject.activeInHierarchy}");

            popSequence.Insert(delay, slotTransform.DOScale(slotSize, popDuration)
                .SetEase(popEase)
                .OnStart(() => Debug.Log($"[GachaField] 슬롯 {index} 트윈 OnStart 호출됨. 현재시각={Time.time}"))
                .OnComplete(() => Debug.Log($"[GachaField] 슬롯 {index} 트윈 OnComplete. 최종 scale={slotTransform.localScale}")));
        }

        Debug.Log($"[GachaField] Sequence 생성 완료. popSequence.IsActive()={popSequence.IsActive()}, popSequence.IsPlaying()={popSequence.IsPlaying()}");
    }
    #endregion

    public void ClearSlots()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}