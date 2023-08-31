using UnityEngine;

public enum SlotType { Field, Up, Mat };
public class SlotAction : MonoBehaviour
{
    [SerializeField] protected SlotType currentSlotType;
    protected UpPanelManager upPanelManager;

    void Awake()
    {
        upPanelManager = FindAnyObjectByType<UpPanelManager>();
    }

    public void Onclick()
    {
        if (currentSlotType == SlotType.Field)
        {
            upPanelManager.AcquireCard(GetComponent<CardSlot>().GetCardData());
            return;
        }
        if (currentSlotType == SlotType.Up)
        {
            upPanelManager.GetIntoAllField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.Mat)
        {
            upPanelManager.BackToMatField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
    }
}
