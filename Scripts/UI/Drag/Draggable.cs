using UnityEngine;
using UnityEngine.EventSystems;

// 드래그 앤 드롭을 할 아이템
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Transform root;

    RectTransform rect;
    Transform previousParent; // 드래그 되기 전 부모
    CanvasGroup canvasGroup;
    UpgradeSlot upgradeSlot;

    void Awake()
    {
        root = FindAnyObjectByType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        upgradeSlot = FindAnyObjectByType<UpgradeSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;

        transform.SetParent(root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == root) // 슬롯 바깥쪽에 떨어트리면
        {
            BackToPrevParent();
        }

        // 업그레이드 슬롯 위로 올릴 수 있는지 체크
        if (upgradeSlot.Available())
        {
            // 업그레이드 슬롯위로 카드를 떨어트리면 draggable의 역할은 끝
            upgradeSlot.AcquireCard(GetComponent<Card>());
        }
        else
        {
            // 다시 원래 위치로
            BackToPrevParent();
        }

        canvasGroup.blocksRaycasts = true;
    }

    public void BackToPrevParent()
    {
        transform.SetParent(previousParent);
        rect.position = previousParent.GetComponent<RectTransform>().position;
    }

    public Transform GetPreviousParent()
    {
        return previousParent;
    }
}
