using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightProjectile : MonoBehaviour
{
    [SerializeField] int bouncingNumbers;
    [SerializeField] string onLandingMask;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent;

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

    void Update()
    {
        UpdateVerticalMovement();
        CheckGroundHit();
        UpdateLayer();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        if (!isInitialized)
        {
            isInitialized = true;
        }
        IsDone = false;

        isGrounded = false;
        this.verticalVelocity = verticalVelocity;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // 현재 속도 초기화 (선택 사항)
        rb.velocity = Vector2.zero;

        // 힘을 적용
        rb.AddForce(groundVelocity * forceMultiplier, ForceMode2D.Impulse);
        StartCoroutine(SlowDownCoroutine());

        gameObject.layer = LayerMask.NameToLayer("InAir");
        rb.mass = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
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
            yield return new WaitForSeconds(checkInterval);
        }

        // 완전히 멈춤
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void UpdateVerticalMovement()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 newPosition = trnsBody.position + new Vector3(0, verticalVelocity * Time.deltaTime, 0);
            trnsBody.position = newPosition;
        }
    }
    void CheckGroundHit()
    {
        Debug.Log($"Body yPos = {trnsBody.position.y}, Shadow yPos = {trnsShadow.position.y}");
        if (trnsBody.position.y <= trnsShadow.position.y && !isGrounded)
        {
            Debug.Log("Is grounds Check");
            // 몸체를 그림자 위치로 정확히 맞춤
            trnsBody.position = new Vector3(trnsBody.position.x, trnsShadow.position.y, trnsBody.position.z);
            isGrounded = true;
            GroundHit();
        }
    }

    void GroundHit()
    {
        if (IsDone) return;
        onGroundHitEvent?.Invoke();
    }

    // 애니메이션 이벤트들
    public void Bounce(float divisionFactor)
    {
        if (bouncingNumbers < 1)
        {
            IsDone = true;
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            rb.velocity = Vector2.zero;
            rb.mass = 100f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }

        // 바운스 시 수직 속도를 양수로 만들어야 함
        verticalVelocity = Mathf.Abs(verticalVelocity) / divisionFactor;

        bouncingNumbers--;
        isGrounded = false;

        Debug.Log($"Bouncing! New verticalVelocity: {verticalVelocity}, Remaining bounces: {bouncingNumbers}");
    }
}