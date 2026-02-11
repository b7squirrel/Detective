using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    ShadowHeight shadowHeight;
    LaserPointer targetPointer;
    bool hasArrived = false;
    bool isFlying = false;

    // 방향 감지를 위한 변수
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

        // 스프라이트 숨김
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.flipX = false;  // 기본 방향으로 리셋
        }

        if (bodyTransform != null)
        {
            previousBodyPosition = bodyTransform.position;
            bodyTransform.localRotation = Quaternion.identity;  // 회전 리셋
        }

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;  // 잠깐 끄기
        }
    }

    public void Initialize(Vector2 targetPosition, LaserPointer pointer, float trajectoryTime)
    {
        if (isFlying)
        {
            Debug.LogWarning($"CatProjectile {gameObject.name}: Already flying! Ignoring Initialize call.");
            return;
        }

        this.targetPointer = pointer;
        isFlying = true;

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

        // 스프라이트 표시할 때 같이 켜기
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }

        Debug.Log($"CatProjectile {gameObject.name}: Initialize COMPLETE");
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

        // X축 속도만 체크 (일정 속도 이상일 때만 방향 변경)
        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            // 왼쪽으로 이동 시 뒤집기, 오른쪽으로 이동 시 정상
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
        isFlying = false;
        targetPointer = null;
    }

    void OnDestroy()
    {
        if (shadowHeight != null && shadowHeight.onGroundHitEvent != null)
        {
            shadowHeight.onGroundHitEvent.RemoveListener(OnArrived);
        }
    }
}