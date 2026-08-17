using System.Collections;
using UnityEngine;

/// <summary>
/// 화살은 처음 지면에 닿을 때 큰 데미지를 줌.
/// 지면에 남아 있는 동안 데미지를 주기는 하지만 크게 의미가 없도록 빨리 사라지도록 함.
/// party 오리와 차별. 파티 오리는 지면에 있을 때 데미지가 의미가 있고 데미지가 작음.
/// </summary>
public class BowProjectile : MonoBehaviour
{
    // 데이터
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }

    bool isFirstFrameAfterLaunch = true; // ✅ 추가

    [Header("Ground Settings")]
    [SerializeField] float groundDuration = .15f;  // 지면에 남아있는 시간
    [SerializeField] float damageInterval = 0.5f; // 데미지 간격

    [Header("Target")]
    [SerializeField] LayerMask target;

    [Header("Shadow")]
    [SerializeField] GameObject shadowObject;

    [Header("Arrow Sprite")]
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Landing Settings")]
    [SerializeField] float landingVerticalAngle = 90f; // 착지 시 고정 각도 (수직으로 서는 각도)


    [Header("Sound")]
    [SerializeField] AudioClip hitGroundSound;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;
    ShadowHeight shadowHeight;

    bool isGrounded = false;

    // 회전을 위한 변수
    Vector3 previousBodyPosition;
    Transform bodyTransform;       // ShadowHeight의 trnsBody
    Quaternion landingRotation;    // 착지 각도 저장용
    Quaternion previousRotation;   // 직전 프레임의 각도 저장

    // ✅ NonAlloc용 버퍼: 매 데미지마다 배열 생성하지 않도록 재사용
    readonly Collider2D[] groundHitBuffer = new Collider2D[20];

    Coroutine groundDamageCo;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        hitEffects = GetComponent<HitEffects>();

        if (spriteRenderer != null)
        {
            bodyTransform = spriteRenderer.transform;
        }
    }

    void OnEnable()
    {
        isGrounded = false;
        isFirstFrameAfterLaunch = true;

        if (bodyTransform != null)
        {
            previousBodyPosition = bodyTransform.position;
        }

        shadowHeight?.ShowHeightShadow();
        ActivateGroundShadow(false);
    }

    void FixedUpdate()
    {
        if (shadowHeight == null) return;

        if (!isGrounded)
        {
            if (shadowHeight.IsGrounded) // ✅ IsDone 대신 IsGrounded 사용
            {
                OnGrounded();
            }
        }
    }

    void LateUpdate()
    {
        if (isGrounded)
        {
            if (bodyTransform != null)
            {
                bodyTransform.localRotation = landingRotation;
            }
        }
        else if (bodyTransform != null)
        {
            if (isFirstFrameAfterLaunch)
            {
                previousBodyPosition = bodyTransform.position;
                isFirstFrameAfterLaunch = false;
            }
            else
            {
                RotateArrow();
            }
        }
    }

    void RotateArrow()
    {
        if (bodyTransform == null) return;

        Vector3 currentBodyPosition = bodyTransform.position;

        // 이동 방향으로 velocity 계산
        Vector2 velocity = (currentBodyPosition - previousBodyPosition) / Time.fixedDeltaTime;

        // velocity가 충분히 크면 회전
        if (velocity.magnitude > 0.5f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            bodyTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }

        previousBodyPosition = currentBodyPosition;
    }

    void OnGrounded()
    {
        isGrounded = true;

        if (bodyTransform != null)
        {
            landingRotation = Quaternion.Euler(0, 0, landingVerticalAngle);
            bodyTransform.localRotation = landingRotation;
        }

        ActivateGroundShadow(true);

        if (hitGroundSound != null)
            PlayGroundHitSound();

        // ✅ 착지 즉시 데미지 1회만 주고, groundDuration 후 사라짐
        CastGroundDamage();

        if (groundDamageCo != null)
            StopCoroutine(groundDamageCo);
        groundDamageCo = StartCoroutine(DeactivateAfterDelay(groundDuration));
    }
    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Deactivate();
    }

    IEnumerator GroundDamageCo()
    {
        float elapsedTime = 0f;

        while (elapsedTime < groundDuration)
        {
            CastGroundDamage();
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }

        Deactivate();
    }

    void CastGroundDamage()
    {
        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position, SizeOfArea, groundHitBuffer, target);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 제거
            Idamageable damageable = groundHitBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (groundHitBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, groundHitBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : null;
            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffectObj);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(
            damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Deactivate));

        if (groundDamageCo != null)
        {
            StopCoroutine(groundDamageCo);
            groundDamageCo = null;
        }

        isGrounded = false;
    }

    void PlayGroundHitSound()
    {
        SoundManager.instance.Play(hitGroundSound);
    }

    public void RandomRotation()
    {
        float angle = UnityEngine.Random.Range(-30f, 30f);
        bodyTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void ActivateGroundShadow(bool activate)
    {
        shadowObject.SetActive(activate);
    }

    // ✅ OnEnable 재실행 여부와 무관하게, 매 발사마다 반드시 호출해서 상태를 강제 초기화
    public void PrepareForLaunch()
    {
        isGrounded = false;
        isFirstFrameAfterLaunch = true;
        // previousBodyPosition은 실제 텔레포트 이후 위치를 잡아야 하므로
        // isFirstFrameAfterLaunch 로직이 LateUpdate에서 알아서 갱신함
    }
}