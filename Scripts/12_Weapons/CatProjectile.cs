using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    ShadowHeight shadowHeight;
    LaserPointer targetPointer;
    bool hasArrived = false;
    bool isFlying = false;  // NEW: 비행 중 플래그
    
    // 회전을 위한 변수
    Vector3 previousBodyPosition;
    Transform bodyTransform;
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
        
        if (spriteRenderer != null)
        {
            bodyTransform = spriteRenderer.transform;
        }
        
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            shadowHeight.onGroundHitEvent.AddListener(OnArrived);
        }
    }

    void OnEnable()
    {
        hasArrived = false;
        isFlying = false;  // 아직 비행 시작 안 함
        
        // 스프라이트 숨김
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
        // CRITICAL: 이미 비행 중이면 무시!
        if (isFlying)
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: Already flying! Ignoring Initialize call.");
            return;
        }
        
        this.targetPointer = pointer;
        isFlying = true;  // 비행 시작
        
        Debug.Log($"CatProjectile {gameObject.name}: Initialize START - target={targetPosition}, time={trajectoryTime}");
        
        // 스프라이트 표시
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log($"CatProjectile {gameObject.name}: Sprite enabled");
        }
        else
        {
            Debug.LogError($"CatProjectile {gameObject.name}: SpriteRenderer is NULL!");
        }
        
        if (shadowHeight != null)
        {
            Debug.Log($"CatProjectile {gameObject.name}: About to call ShadowHeight.InitializeToTarget");
            shadowHeight.InitializeToTarget(targetPosition, trajectoryTime);
            Debug.Log($"CatProjectile {gameObject.name}: ShadowHeight.InitializeToTarget called, IsDone={shadowHeight.IsDone}");
        }
        else
        {
            Debug.LogError($"CatProjectile {gameObject.name}: ShadowHeight is null!");
        }
        
        Debug.Log($"CatProjectile {gameObject.name}: Initialize COMPLETE");
    }

    void FixedUpdate()
    {
        if (!hasArrived && isFlying)
        {
            RotateCat();
        }
    }

    void RotateCat()
    {
        if (bodyTransform == null) return;

        Vector3 currentBodyPosition = bodyTransform.position;
        Vector2 velocity = (currentBodyPosition - previousBodyPosition) / Time.fixedDeltaTime;
        
        if (velocity.magnitude > 0.5f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            bodyTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        
        previousBodyPosition = currentBodyPosition;
    }

    void OnArrived()
    {
        if (hasArrived) 
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: OnArrived called but already arrived!");
            return;
        }
        
        hasArrived = true;
        isFlying = false;  // 비행 종료
        
        Debug.Log($"CatProjectile {gameObject.name}: OnArrived - notifying LaserPointer");
        
        if (targetPointer != null)
        {
            targetPointer.OnCatArrived();
        }
        else
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: targetPointer is null!");
        }
        
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        hasArrived = false;
        isFlying = false;  // 리셋
        targetPointer = null;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    void OnDestroy()
    {
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            shadowHeight.onGroundHitEvent.RemoveListener(OnArrived);
        }
    }
}