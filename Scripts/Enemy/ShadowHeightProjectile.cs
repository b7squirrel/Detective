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
    [Header("SFX")]
    [SerializeField] AudioClip bounceSFX;
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
            // ⭐ 수정: "Enemy"로 하드코딩하지 않고 onLandingMask 기준으로 결정
            //         (SetLandingLayer와 동일한 로직을 GetLandingLayer()로 공유)
            gameObject.layer = GetLandingLayer();
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
            // ⭐ 바운스 사운드 재생
            if (bounceSFX != null) SoundManager.instance.Play(bounceSFX);
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
    // ⭐ 수정: "Enemy" 하드코딩 로직을 GetLandingLayer()로 분리하여 UpdateLayer()와 공유
    void SetLandingLayer()
    {
        gameObject.layer = GetLandingLayer();
    }
    // ⭐ 추가: onLandingMask 문자열을 실제 레이어 인덱스로 변환.
    //         지정된 레이어가 없거나 유효하지 않으면 기존과 동일하게 "Enemy"로 폴백.
    int GetLandingLayer()
    {
        if (!string.IsNullOrEmpty(onLandingMask))
        {
            int layer = LayerMask.NameToLayer(onLandingMask);
            if (layer != -1)
            {
                return layer;
            }
            Debug.LogWarning($"Layer '{onLandingMask}' not found. Using 'Enemy' layer instead.");
        }
        return LayerMask.NameToLayer("Enemy");
    }
    /// <summary>
    /// 착지 후 Kinematic으로 고정된 상태를, 다시 Dynamic으로 전환.
    /// 착지한 오브젝트끼리 서로 물리적으로 밀어내는 반응이 필요할 때 사용.
    /// (Bounce()의 기본 동작은 변경하지 않고, 필요한 곳에서만 별도로 호출)
    /// </summary>
    public void EnablePhysicsAfterLanding(RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation, float landedMass = 3f)
    {
        if (rb == null) return;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = constraints;
        rb.mass = landedMass; // 100은 분리력을 거의 무력화시키므로 적당한 값으로 낮춤
        rb.WakeUp(); // 혹시 Sleep 상태라면 강제로 깨워서 물리 연산이 실행되도록 함
    }

    /// <summary>
    /// 착지 후 짧은 시간(duration)만 Dynamic으로 전환해 겹친 오브젝트끼리 서로 밀어내게 한 뒤,
    /// 다시 Kinematic으로 되돌려 플레이어 등 외력에 밀리지 않도록 고정.
    /// EnablePhysicsAfterLanding()과 동일한 파라미터를 받되, 일정 시간 뒤 자동으로 되돌리는 버전.
    /// </summary>
    public void EnablePhysicsAfterLandingTemporary(float duration = 0.5f, RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation, float landedMass = 3f, float kinematicMass = 100f)
    {
        StartCoroutine(EnablePhysicsAfterLandingTemporaryCo(duration, constraints, landedMass, kinematicMass));
    }

    IEnumerator EnablePhysicsAfterLandingTemporaryCo(float duration, RigidbodyConstraints2D constraints, float landedMass, float kinematicMass)
    {
        EnablePhysicsAfterLanding(constraints, landedMass);

        yield return new WaitForSeconds(duration);

        // 이미 폭발 등으로 비활성화되었거나 다시 발사된 상태라면 되돌리지 않음
        if (rb == null || !gameObject.activeSelf || !IsDone) yield break;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.mass = kinematicMass;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}