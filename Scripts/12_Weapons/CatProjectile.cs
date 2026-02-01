using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    ShadowHeight shadowHeight;
    LaserPointer targetPointer;
    bool hasArrived = false;
    
    // 회전을 위한 변수
    Vector3 previousBodyPosition;
    Transform bodyTransform; // ShadowHeight의 trnsBody
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (shadowHeight == null)
        {
            Debug.LogError("CatProjectile: ShadowHeight component not found!");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("CatProjectile: SpriteRenderer not found!");
        }
        
        // SpriteRenderer의 Transform을 bodyTransform으로 사용
        if (spriteRenderer != null)
        {
            bodyTransform = spriteRenderer.transform;
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
        
        // CRITICAL: 스프라이트 숨김 (Initialize에서 보이게 함)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        if (bodyTransform != null)
        {
            previousBodyPosition = bodyTransform.position;
        }
    }

    public void Initialize(Vector2 targetPosition, LaserPointer pointer, float trajectoryTime)
    {
        this.targetPointer = pointer;
        
        // CRITICAL: 스프라이트 표시
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (shadowHeight != null)
        {
            // ShadowHeight의 InitializeToTarget 사용 (목표 위치와 비행 시간)
            shadowHeight.InitializeToTarget(targetPosition, trajectoryTime);
        }
        else
        {
            Debug.LogError("CatProjectile: ShadowHeight is null!");
        }
    }

    void FixedUpdate()
    {
        // 착지 전에만 회전
        if (!hasArrived)
        {
            RotateCat();
        }
    }

    void RotateCat()
    {
        if (bodyTransform == null) return;

        // 현재 body 위치
        Vector3 currentBodyPosition = bodyTransform.position;
        
        // 이동 방향 계산 (velocity)
        Vector2 velocity = (currentBodyPosition - previousBodyPosition) / Time.fixedDeltaTime;
        
        // velocity가 충분히 크면 회전
        if (velocity.magnitude > 0.5f)
        {
            // 각도 계산 (고양이가 이동 방향을 향하도록)
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            
            // 로컬 rotation 사용
            bodyTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        
        // 이전 위치 업데이트
        previousBodyPosition = currentBodyPosition;
    }

    void OnArrived()
    {
        if (hasArrived) return; // 중복 호출 방지
        
        hasArrived = true;
        
        // 레이저 포인터에 도착 알림
        if (targetPointer != null)
        {
            targetPointer.OnCatArrived();
        }
        else
        {
            Debug.LogWarning("CatProjectile: targetPointer is null!");
        }
        
        // 비활성화
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        hasArrived = false;
        targetPointer = null;
        
        // 스프라이트 숨김
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
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