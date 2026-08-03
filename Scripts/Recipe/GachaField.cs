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
    [SerializeField] float popInterval = 0.08f;
    [SerializeField] float popDuration = 0.3f;
    [SerializeField] float lastCardExtraDelay = 0.5f;
    [SerializeField] Ease popEase = Ease.OutBack;

    [Header("사운드")]
    [SerializeField] AudioClip cardPopSound;
    [SerializeField] float cardPopPitchStart = 0.95f; // 첫 카드 피치
    [SerializeField] float cardPopPitchEnd = 1.25f;    // 마지막 직전 카드 피치

    [Header("마지막 카드 (Mythic 아닐 때)")]
    [SerializeField] AudioClip[] lastCardPopSounds;
    [SerializeField] GameObject lastCardEffectPrefab;
    [SerializeField] float lastCardEffectLifetime = 2f;

    [Header("마지막 카드 (Mythic일 때)")]
    [SerializeField] AudioClip[] mythicCardPopSounds;
    [SerializeField] GameObject mythicCardEffectPrefab;
    [SerializeField] float mythicCardEffectLifetime = 2f;

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

        for (int i = 0; i < numSlots; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = Vector3.zero;
            slots.Add(slot);
        }

        List<CardData> cardDataSorted = new();
        cardDataSorted.AddRange(cardDatas);
        cardDataSorted.Sort((a, b) => new Sort().ByGrade(a, b));

        for (int i = 0; i < numSlots; i++)
        {
            displayCardOnSlot.PutCardDataIntoSlot(cardDataSorted[i], slots[i].GetComponent<CardSlot>());
        }

        // ⭐ 마지막 카드가 Mythic 등급인지 판별
        bool isLastCardMythic = numSlots > 0 && cardDataSorted[numSlots - 1].Grade == MyGrade.Mythic;

        PlayPopSequence(slots, isLastCardMythic);
    }

    void PlayPopSequence(List<GameObject> slots, bool isLastCardMythic)
    {
        popSequence?.Kill();
        popSequence = DOTween.Sequence();

        int lastIndex = slots.Count - 1;

        for (int i = 0; i < slots.Count; i++)
        {
            float delay = i * popInterval;
            bool isLastCard = (i == lastIndex);

            if (isLastCard)
            {
                delay += lastCardExtraDelay;
            }

            Transform slotTransform = slots[i].transform;
            int index = i;

            popSequence.Insert(delay, slotTransform.DOScale(slotSize, popDuration)
                .SetEase(popEase)
                .OnStart(() =>
                {
                    if (isLastCard)
                    {
                        if (isLastCardMythic)
                        {
                            PlayMythicCardSounds();
                            PlayMythicCardEffect(slotTransform);
                        }
                        else
                        {
                            PlayLastCardSounds();
                            PlayLastCardEffect(slotTransform);
                        }
                    }
                    else if (cardPopSound != null && SoundManager.instance != null)
                    {
                        float t = lastIndex > 1 ? (float)index / (lastIndex - 1) : 0f;
                        float pitch = Mathf.Lerp(cardPopPitchStart, cardPopPitchEnd, t);
                        SoundManager.instance.PlaySoundWith(cardPopSound, 1f, pitch, 0f, 10);
                    }
                }));
        }
    }

    // ⭐ 마지막 카드용 사운드 배열을 전부 동시 재생 (Mythic 아닐 때)
    void PlayLastCardSounds()
    {
        if (lastCardPopSounds == null || lastCardPopSounds.Length == 0) return;
        if (SoundManager.instance == null) return;

        foreach (var clip in lastCardPopSounds)
        {
            if (clip == null) continue;
            SoundManager.instance.PlaySoundWith(clip, 1f, 1f, 0f, 10);
        }
    }

    void PlayLastCardEffect(Transform slotTransform)
    {
        if (lastCardEffectPrefab == null) return;

        GameObject effect = Instantiate(lastCardEffectPrefab, slotTransform.position, Quaternion.identity, slotTransform);

        Animator effectAnimator = effect.GetComponent<Animator>();
        if (effectAnimator != null)
        {
            effectAnimator.SetTrigger("On");
        }
        else
        {
            Logger.LogWarning("[GachaField] lastCardEffectPrefab에 Animator 컴포넌트가 없습니다.");
        }

        Destroy(effect, lastCardEffectLifetime);
    }

    // ⭐ Mythic 전용 사운드 배열 전부 동시 재생
    void PlayMythicCardSounds()
    {
        if (mythicCardPopSounds == null || mythicCardPopSounds.Length == 0) return;
        if (SoundManager.instance == null) return;

        foreach (var clip in mythicCardPopSounds)
        {
            if (clip == null) continue;
            SoundManager.instance.PlaySoundWith(clip, 1f, 1f, 0f, 10);
        }
    }

    void PlayMythicCardEffect(Transform slotTransform)
    {
        if (mythicCardEffectPrefab == null) return;

        GameObject effect = Instantiate(mythicCardEffectPrefab, slotTransform.position, Quaternion.identity, slotTransform);

        Animator effectAnimator = effect.GetComponent<Animator>();
        if (effectAnimator == null)
        {
            Logger.LogWarning("[GachaField] mythicCardEffectPrefab에 Animator 컴포넌트가 없습니다.");
        }

        Destroy(effect, mythicCardEffectLifetime);
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