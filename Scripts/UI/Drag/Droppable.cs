using UnityEngine;
using UnityEngine.EventSystems;

// 드래그 한 아이템을 떨어트릴 장소, upgrade slot
public class Droppable : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
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

        eventData.pointerDrag.transform.SetParent(transform);
        eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
    }
}
