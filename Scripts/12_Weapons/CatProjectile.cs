using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    ShadowHeight shadowHeight;
    LaserPointer targetPointer;
    bool hasArrived = false;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        
        if (shadowHeight == null)
        {
            Debug.LogError("CatProjectile: ShadowHeight component not found!");
        }
        
        // UnityEvent 리스너 등록
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            shadowHeight.onGroundHitEvent.AddListener(OnArrived);
        }
    }

    void OnEnable()
    {
        hasArrived = false;
    }

    public void Initialize(Vector2 targetPosition, LaserPointer pointer, float trajectoryTime)
    {
        this.targetPointer = pointer;
        
        if (shadowHeight != null)
        {
            // ShadowHeight의 InitializeToTarget 사용 (목표 위치와 비행 시간)
            shadowHeight.InitializeToTarget(targetPosition, trajectoryTime);
        }
    }

    void OnArrived()
    {
        if (hasArrived) return; // 중복 호출 방지
        
        hasArrived = true;
        
        Debug.Log("CatProjectile: Cat arrived at laser pointer!");
        
        // 레이저 포인터에 도착 알림
        if (targetPointer != null)
        {
            targetPointer.OnCatArrived();
        }
        
        // 비활성화
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        hasArrived = false;
        targetPointer = null;
    }
    
    void OnDestroy()
    {
        // 리스너 제거
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            shadowHeight.onGroundHitEvent.RemoveListener(OnArrived);
        }
    }
}