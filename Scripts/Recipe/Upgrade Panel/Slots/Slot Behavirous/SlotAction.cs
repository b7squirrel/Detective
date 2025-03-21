using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 업그레이드 슬롯
/// </summary>
public class SlotAction : MonoBehaviour
{
    [SerializeField] protected SlotType currentSlotType;
    protected UpPanelManager upPanelManager;

    void Awake()
    {
        
    }

    public void Onclick()
    {
        StartCoroutine(OnClickCo());
    }
    IEnumerator OnClickCo()
    {
        // 터치하면 일단 또잉또잉
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;
        slotRec.transform.localScale = new Vector2(initialValue * 1.1f, initialValue * 1.1f);
        slotRec.DOScale(initialValue, .04f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.066f);

        if (upPanelManager == null)
        {
            upPanelManager = FindAnyObjectByType<UpPanelManager>();
        }

        ActionType();
    }

    void ActionType()
    {
        if (currentSlotType == SlotType.Field)
        {
            // 장착이 되어 있는지 확인하는 메서드 실행
            upPanelManager.CheckIsEquipped(GetComponent<CardSlot>().GetCardData());
            return;
        }
        if (currentSlotType == SlotType.Up)
        {
            upPanelManager.SetUpSlotCanceled(true);
            upPanelManager.GetIntoAllField(GetComponent<CardSlot>().GetCardData().Type);
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.Mat)
        {
            upPanelManager.BackToMatField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.None)
            return;
    }
}
