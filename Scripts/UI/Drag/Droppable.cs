using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 드래그 한 아이템을 떨어트릴 장소, upgrade slot
public class Droppable : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    Image image;
    RectTransform rect;
    UpgradeSlot upgradeSlot;


    void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        upgradeSlot = GetComponent<UpgradeSlot>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
    public void OnDrop(PointerEventData eventData)
    {
        // pointerDrag는 현재 드래그 하고 있는 아이템
        if (eventData.pointerDrag == null)
            return;

        Transform prevParent = eventData.pointerDrag.GetComponent<Draggable>().GetPreviousParent();
        upgradeSlot.SetPrevParent(prevParent);

        eventData.pointerDrag.transform.SetParent(transform);
        eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
        RectTransform cardRect = eventData.pointerDrag.GetComponent<RectTransform>();
        // cardRect = rect;
    }
}
