using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightProjectile : MonoBehaviour
{
    [SerializeField] int bouncingNumbers;
    [SerializeField] int bounceCounter;
    [SerializeField] string onLandingMask;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent; // 지면에 닿았을 때
    public UnityEvent onDone; // 움직임이 완전히 끝났을 때

    [SerializeField] Transform trnsObject; // 부모 물체
    [SerializeField] Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    [SerializeField] Transform trnsShadow; // 그림자 스프라이트 오브젝트
    [SerializeField] SpriteRenderer bodySprite; // 공중에 뜨는 스프라이트
    [SerializeField] SpriteRenderer shadowSprite; // 그림자 스프라이트

    Rigidbody2D rb;
    float gravity = -100f;
    float verticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    // 감속 변수
    [SerializeField] float forceMultiplier = 1f; // 속도에 곱해줄 힘 계수
    [SerializeField] float deceleration = 0.98f; // 0.95~0.99 권장
    [SerializeField] float minVelocityToStop = 0.05f;
    [SerializeField] float checkInterval = 0.02f; // 감속 체크 주기 (FixedUpdate 비슷하게)

    void FixedUpdate()
    {
        UpdateVerticalMovement();
        CheckGroundHit();
    }
    void Update()
    {
        UpdateLayer();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        IsDone = false;
        isGrounded = false;
        bounceCounter = 0; // ← 추가! 바운스 카운터 리셋
        trnsBody.position = Vector2.zero;
        this.verticalVelocity = verticalVelocity;

        // rb와 anim은 한 번만 초기화
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        // ★ Body와 Shadow 위치 초기화 (추가!)
        if (trnsBody != null && trnsShadow != null)
        {
            // Body를 Shadow와 같은 높이로 리셋
            trnsBody.localPosition = new Vector3(0, 0, 0);
            trnsShadow.localPosition = new Vector3(0, -0.3f, 0); // Inspector에서 설정한 값과 동일하게
        }

        // 매번 초기화되어야 하는 것들
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.mass = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        gameObject.layer = LayerMask.NameToLayer("InAir");

        // 힘을 적용
        rb.AddForce(groundVelocity * forceMultiplier, ForceMode2D.Impulse);

        // 기존 코루틴 정리 후 시작
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
            rb.velocity *= deceleration;

            // 부드러운 감속을 위해 짧은 간격으로 반복
            // yield return new WaitForSeconds(checkInterval);
            yield return new WaitForFixedUpdate();
        }

        // 완전히 멈춤
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
        // Debug.Log($"Body yPos = {trnsBody.position.y}, Shadow yPos = {trnsShadow.position.y}");
        if (trnsBody.position.y <= trnsShadow.position.y && !isGrounded)
        {
            // Debug.Log("Is grounds Check");
            // 몸체를 그림자 위치로 정확히 맞춤
            trnsBody.position = new Vector3(trnsBody.position.x, trnsShadow.position.y, trnsBody.position.z);
            isGrounded = true;
            GroundHit();
        }
    }

    //외부에서 바운싱이 완전히 끝나서 지면에 멈춰있는 상태인지 확인할 때
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

    // 애니메이션 이벤트들
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

        // 바운스 시 수직 속도를 양수로 만들어야 함
        verticalVelocity = Mathf.Abs(verticalVelocity) / divisionFactor;

        bounceCounter++;
        isGrounded = false;

        // Debug.Log($"Bouncing! New verticalVelocity: {verticalVelocity}, Remaining bounces: {bounceCounter}");
    }

    // 완전히 착지했을 때 레이어를 변경하는 메서드
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