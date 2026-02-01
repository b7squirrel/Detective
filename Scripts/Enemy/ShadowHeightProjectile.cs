using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightProjectile : MonoBehaviour
{
    [SerializeField] int bouncingNumbers;
    [SerializeField] int bounceCounter;
    [SerializeField] string onLandingMask;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent;
    public UnityEvent onDone;

    [SerializeField] Transform trnsObject;
    [SerializeField] Transform trnsBody;
    [SerializeField] Transform trnsShadow;
    [SerializeField] SpriteRenderer bodySprite;
    [SerializeField] SpriteRenderer shadowSprite;

    Rigidbody2D rb;
    float gravity = -100f;
    float verticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    [SerializeField] float forceMultiplier = 1f;
    [SerializeField] float deceleration = 0.98f;
    [SerializeField] float minVelocityToStop = 0.05f;
    [SerializeField] float checkInterval = 0.02f;

    // ⭐ 시간 정지 관련 변수 추가
    FieldItemEffect fieldItemEffect;
    Vector2 savedVelocity; // 정지 전 속도 저장
    float savedAngularVelocity; // 정지 전 회전 속도 저장

    void Awake()
    {
        fieldItemEffect = FindObjectOfType<FieldItemEffect>();
    }

    void FixedUpdate()
    {
        // ⭐ 시간 정지 체크
        if (fieldItemEffect != null && fieldItemEffect.IsStopedWithStopwatch())
        {
            PauseProjectile();
            return;
        }
        else
        {
            ResumeProjectile();
        }

        UpdateVerticalMovement();
        CheckGroundHit();
    }

    void Update()
    {
        // ⭐ 시간 정지 중에는 레이어 업데이트 안 함
        if (fieldItemEffect != null && fieldItemEffect.IsStopedWithStopwatch())
            return;

        UpdateLayer();
    }

    // ⭐ 투사체 일시정지
    void PauseProjectile()
    {
        if (rb != null && rb.velocity != Vector2.zero)
        {
            // 현재 속도 저장
            savedVelocity = rb.velocity;
            savedAngularVelocity = rb.angularVelocity;

            // 완전히 정지
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ⭐ 투사체 재개
    void ResumeProjectile()
    {
        if (savedVelocity != Vector2.zero && rb != null)
        {
            // 저장된 속도 복구
            rb.velocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;

            // 복구 후 초기화
            savedVelocity = Vector2.zero;
            savedAngularVelocity = 0f;
        }
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        IsDone = false;
        isGrounded = false;
        bounceCounter = 0;
        trnsBody.position = Vector2.zero;
        this.verticalVelocity = verticalVelocity;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (trnsBody != null && trnsShadow != null)
        {
            trnsBody.localPosition = new Vector3(0, 0, 0);
            trnsShadow.localPosition = new Vector3(0, -0.3f, 0);
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.mass = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        gameObject.layer = LayerMask.NameToLayer("InAir");

        rb.AddForce(groundVelocity * forceMultiplier, ForceMode2D.Impulse);

        // ⭐ 저장된 속도 초기화
        savedVelocity = Vector2.zero;
        savedAngularVelocity = 0f;

        StopAllCoroutines();
        StartCoroutine(SlowDownCoroutine());

        isInitialized = true;
    }

    void UpdateLayer()
    {
        if (isGrounded)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("InAir");
        }
    }

    IEnumerator SlowDownCoroutine()
    {
        while (rb.velocity.magnitude > minVelocityToStop)
        {
            // ⭐ 시간 정지 중에는 감속 안 함
            if (fieldItemEffect == null || !fieldItemEffect.IsStopedWithStopwatch())
            {
                rb.velocity *= deceleration;
            }

            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void UpdateVerticalMovement()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
            Vector3 newPosition = trnsBody.position + new Vector3(0, verticalVelocity * Time.fixedDeltaTime, 0);
            trnsBody.position = newPosition;
        }
    }

    void CheckGroundHit()
    {
        if (trnsBody.position.y <= trnsShadow.position.y && !isGrounded)
        {
            trnsBody.position = new Vector3(trnsBody.position.x, trnsShadow.position.y, trnsBody.position.z);
            isGrounded = true;
            GroundHit();
        }
    }

    public bool GetIsDone()
    {
        return IsDone;
    }

    void GroundHit()
    {
        if (IsDone) return;
        onGroundHitEvent?.Invoke();
    }

    void DoneBouncing()
    {
        onDone?.Invoke();
    }

    public void Bounce(float divisionFactor)
    {
        if (bounceCounter > bouncingNumbers)
        {
            IsDone = true;
            SetLandingLayer();
            rb.velocity = Vector2.zero;
            rb.mass = 100f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            bounceCounter = 0;

            DoneBouncing();
            return;
        }

        verticalVelocity = Mathf.Abs(verticalVelocity) / divisionFactor;

        bounceCounter++;
        isGrounded = false;
    }

    void SetLandingLayer()
    {
        if (!string.IsNullOrEmpty(onLandingMask))
        {
            int layer = LayerMask.NameToLayer(onLandingMask);
            if (layer != -1)
            {
                gameObject.layer = layer;
            }
            else
            {
                Debug.LogWarning($"Layer '{onLandingMask}' not found. Using 'Enemy' layer instead.");
                gameObject.layer = LayerMask.NameToLayer("Enemy");
            }
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }
}