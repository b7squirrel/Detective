using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ShadowHeightDeadProjectile : MonoBehaviour
{
    [Header("Body & Shadow")]
    [SerializeField] Transform trnsBody;
    [SerializeField] SpriteRenderer sprRndBody;
    [SerializeField] Transform trnsShadow;
    [SerializeField] SpriteRenderer sprRndshadow;

    [Header("Blink Settings")]
    [SerializeField] float blinkInterval = 0.1f; // 깜빡이는 간격
    [SerializeField] float blinkAlpha = 0.3f;    // 반투명 정도 (0~1)

    Vector2 shadowOffset = new Vector2(.1f, -.2f);
    [SerializeField] int bouncingNumbers;
    [SerializeField] bool noHeightShadow;
    [SerializeField] string onLandingMask;
    
    private bool isDone;
    public bool IsDone 
    { 
        get => isDone;
        private set 
        {
            if (!isDone && value)
            {
                isDone = value;
                onDoneEvent?.Invoke();
            }
            else
            {
                isDone = value;
            }
        }
    }
    
    public UnityEvent onGroundHitEvent;
    public UnityEvent onDoneEvent;

    Transform trnsObject;

    float gravity = -100f;
    Vector2 groundVelocity;
    Vector2 pastGroundVelocity;
    float verticalVelocity;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    Vector2 startPosition;
    Vector2 targetPosition;
    float trajectoryTime;
    float currentTime;
    bool useTargetBasedTrajectory;

    void Update()
    {
        UpdatePosition();
        UpdateShadow();
        CheckGroundHit();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity, Sprite sprite)
    {
        InitializeCommon();
        
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;
        useTargetBasedTrajectory = false;

        if(sprite != null) sprRndBody.sprite = sprite;
        
        // ★ 깜빡임 시작
        StartCoroutine(BlinkEffect());
    }

    void InitializeCommon()
    {
        if (!isInitialized)
        {
            trnsObject = transform;
            
            // Body 찾기
            if (trnsBody == null)
            {
                Transform bodyTransform = transform.Find("BodyTrns");
                if (bodyTransform == null)
                    bodyTransform = transform.Find("Body");
                
                if (bodyTransform != null)
                {
                    trnsBody = bodyTransform;
                    sprRndBody = bodyTransform.GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            // Shadow 찾기
            if (noHeightShadow == false && trnsShadow == null)
            {
                Transform shadowTransform = transform.Find("Shadow Trns");
                if (shadowTransform == null)
                    shadowTransform = transform.Find("Shadow");
                
                if (shadowTransform != null)
                {
                    trnsShadow = shadowTransform;
                    sprRndshadow = shadowTransform.GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            isInitialized = true;
        }
        
        // Body 초기 위치
        if (trnsBody != null)
        {
            trnsBody.localPosition = Vector3.zero;
        }
        
        // ★ 스프라이트 색상 초기화
        if (sprRndBody != null)
        {
            Color originalColor = sprRndBody.color;
            sprRndBody.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
        
        IsDone = false;
        isGrounded = false;
        anim = GetComponent<Animator>();
    }

    // ★ 깜빡임 코루틴
    IEnumerator BlinkEffect()
    {
        if (sprRndBody == null) yield break;
        
        Color originalColor = sprRndBody.color;
        
        while (!IsDone)
        {
            // 반투명
            sprRndBody.color = new Color(originalColor.r, originalColor.g, originalColor.b, blinkAlpha);
            yield return new WaitForSeconds(blinkInterval);
            
            // 불투명
            sprRndBody.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // 완료 후 원래 색상으로
        sprRndBody.color = originalColor;
    }

    void UpdatePosition()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            
            // ★ Local position 사용
            Vector3 currentLocalPos = trnsBody.localPosition;
            currentLocalPos.y += verticalVelocity * Time.deltaTime;
            trnsBody.localPosition = currentLocalPos;
        }
        
        if (IsDone == false)
        {
            trnsObject.position += (Vector3)groundVelocity * Time.deltaTime;
        }
    }

    void UpdateShadow()
    {
        if (noHeightShadow == false)
        {
            sprRndshadow.sprite = sprRndBody.sprite;
            sprRndshadow.flipX = sprRndBody.flipX;
            sprRndshadow.flipY = sprRndBody.flipY;

            trnsShadow.localRotation = trnsBody.localRotation;
            trnsShadow.localScale = trnsBody.localScale;
        }

        if (isGrounded)
        {
            sprRndBody.sortingLayerName = onLandingMask;
            if (noHeightShadow == false)
            {
                trnsShadow.position = new Vector2(trnsObject.position.x + shadowOffset.x,
                        trnsObject.position.y + shadowOffset.y);

                sprRndshadow.sortingLayerName = "Shadow";
            }
        }
        else
        {
            sprRndBody.sortingLayerName = "FloatingOver";

            if (noHeightShadow == false)
            {
                trnsShadow.position = new Vector2(trnsObject.position.x,
                        trnsObject.position.y);

                sprRndshadow.sortingLayerName = "ShadowOver";
            }
        }
    }

    void CheckGroundHit()
    {
        // ★ Local position 체크
        if (!useTargetBasedTrajectory && trnsBody.localPosition.y < 0 && !isGrounded)
        {
            trnsBody.localPosition = new Vector3(trnsBody.localPosition.x, 0, trnsBody.localPosition.z);
            isGrounded = true;
            GroundHit();
        }
    }

    void GroundHit()
    {
        if (IsDone) return;
        onGroundHitEvent?.Invoke();
    }

    public void Stick()
    {
        groundVelocity = Vector2.zero;
    }

    public void Bounce(float divisionFactor)
    {
        if (IsDone)
        {
            groundVelocity = Vector2.zero;
            return;
        }
        if (bouncingNumbers < 1)
        {
            IsDone = true;
            return;
        }
        
        useTargetBasedTrajectory = false;
        Initialize(groundVelocity, lastInitaialVerticalVelocity / divisionFactor, null);
        bouncingNumbers--;
    }

    public void SlowDownGroundVelocity(float divisionFactor)
    {
        if (IsDone) return;
        if (Vector2.Distance(groundVelocity, pastGroundVelocity) < .1f)
        {
            IsDone = true;
            groundVelocity = Vector2.zero;
            return;
        }
        groundVelocity = groundVelocity / divisionFactor;
        pastGroundVelocity = groundVelocity;
    }

    public void RemoveHeigihtShadow()
    {
        if (noHeightShadow) return;

        if (verticalVelocity < 0.1f && bouncingNumbers == 0)
        {
            sprRndshadow.color = new Color(0, 0, 0, 0f);
        }
    }

    public void SetToKinematic()
    {
        if (IsDone)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }

    public void TriggerIdleAnim()
    {
        if (bouncingNumbers == 0)
        {
            anim.SetTrigger("Idle");
        }
    }
    
    public void Deactivate()
    {
        // ★ 비활성화 전 코루틴 정리
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    
    // ★ OnDisable에서도 정리
    void OnDisable()
    {
        StopAllCoroutines();
        
        // 스프라이트 색상 초기화
        if (sprRndBody != null)
        {
            Color originalColor = sprRndBody.color;
            sprRndBody.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }
}