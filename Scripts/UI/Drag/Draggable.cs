using UnityEngine;
using UnityEngine.EventSystems;

// 드래그 앤 드롭을 할 아이템
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Transform root;

    RectTransform rect;
    Transform previousParent; // 드래그 되기 전 부모
    CanvasGroup canvasGroup;
    
    SlotUpCard slotUpCard;
    SlotManager slotManager;

    bool isOnUpSlot; // 업그레이드 슬롯 위에 올라가 있는지 여부

    void Awake()
    {
        root = FindAnyObjectByType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        slotUpCard = FindAnyObjectByType<SlotUpCard>();
        slotManager = FindAnyObjectByType<SlotManager>();
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
            if (isOnUpSlot)
            {
                slotManager.GetIntoMyCardsmanager();
                Destroy(gameObject);
                return; // return이 없으면 Destroy 이후에도 아래로 내려가서 실행한다
            }
            else
            {
                BackToPrevParent();
                canvasGroup.blocksRaycasts = true;
                return;
            }
        }

        // 업그레이드 슬롯 위로 올릴 수 있는지 체크
        if (slotUpCard.IsAvailable(GetComponent<Card>()))
        {
            // 업그레이드 슬롯위로 카드를 떨어트리면 draggable의 역할은 끝
            slotUpCard.AcquireCard(GetComponent<Card>());
            isOnUpSlot = true;
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
