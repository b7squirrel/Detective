using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    ShadowHeight shadowHeight;
    LaserPointer targetPointer;
    bool hasArrived = false;
    bool isFlying = false;
    
    Vector3 previousBodyPosition;
    Transform bodyTransform;
    SpriteRenderer spriteRenderer;
    TrailRenderer trailRenderer;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        
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
        isFlying = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.flipX = false;
        }
        
        if (bodyTransform != null)
        {
            previousBodyPosition = bodyTransform.position;
            bodyTransform.localRotation = Quaternion.identity;
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    // ✅ 기존 메서드 (시간 기반)
    public void Initialize(Vector2 targetPosition, LaserPointer pointer, float trajectoryTime)
    {
        if (isFlying)
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: Already flying! Ignoring Initialize call.");
            return;
        }
        
        this.targetPointer = pointer;
        isFlying = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
        
        if (shadowHeight != null)
        {
            shadowHeight.InitializeToTarget(targetPosition, trajectoryTime);
        }
    }

    // ✅ 새로운 메서드 (높이 기반) - 높이를 고정하고 시간은 자동 계산
    public void InitializeWithHeight(Vector2 targetPosition, LaserPointer pointer, float maxHeight)
    {
        if (isFlying)
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: Already flying! Ignoring Initialize call.");
            return;
        }
        
        this.targetPointer = pointer;
        isFlying = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
        
        if (shadowHeight != null)
        {
            shadowHeight.InitializeWithHeight(targetPosition, maxHeight);
        }
    }

    void FixedUpdate()
    {
        if (!hasArrived && isFlying)
        {
            UpdateSpriteDirection();
        }
    }

    void UpdateSpriteDirection()
    {
        if (bodyTransform == null || spriteRenderer == null) return;

        Vector3 currentBodyPosition = bodyTransform.position;
        Vector2 velocity = (currentBodyPosition - previousBodyPosition) / Time.fixedDeltaTime;
        
        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            spriteRenderer.flipX = velocity.x < 0;
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
        isFlying = false;
        
        if (targetPointer != null)
        {
            targetPointer.OnCatArrived();
        }

        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        hasArrived = false;
        isFlying = false;
        targetPointer = null;
    }

    void OnDestroy()
    {
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            trailRenderer.enabled = false;
            shadowHeight.onGroundHitEvent.RemoveListener(OnArrived);
        }
    }
}