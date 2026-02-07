using UnityEngine;
using System.Collections;

public class BowProjectile : MonoBehaviour
{
    // 데이터
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }

    [Header("Ground Settings")]
    [SerializeField] float groundDuration = .3f; // 지면에 남아있는 시간
    [SerializeField] float damageInterval = 0.5f; // 데미지 간격
    
    [Header("Target")]
    [SerializeField] LayerMask target;

    [Header("Shadow")]
    [SerializeField] GameObject shadowObject;
    
    ShadowHeight shadowHeight;
    SpriteRenderer spriteRenderer;
    bool isGrounded = false;
    
    // 회전을 위한 변수
    Vector3 previousBodyPosition;
    Transform bodyTransform; // ShadowHeight의 trnsBody
    
    Coroutine groundDamageCo;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (shadowHeight == null)
        {
            Debug.LogError("BowProjectile: ShadowHeight component not found!");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("BowProjectile: SpriteRenderer not found!");
        }
        
        // SpriteRenderer의 Transform을 bodyTransform으로 사용
        if (spriteRenderer != null)
        {
            bodyTransform = spriteRenderer.transform;
        }
    }

    void OnEnable()
    {
        isGrounded = false;
        
        if (bodyTransform != null)
        {
            previousBodyPosition = bodyTransform.position;
        }
        // bodyTransform.localRotation = Quaternion.Euler(0, 0, 0);

        ActivateGroundShadow(false);
    }

    void FixedUpdate()
    {
        if (shadowHeight == null) return;

        // 착지 전에만 회전
        if (!isGrounded)
        {
            RotateArrow();
            
            // 착지 체크
            if (shadowHeight.IsDone)
            {
                OnGrounded();
            }
        }
    }

    void LateUpdate()
    {
        // 착지 후에는 수직으로 고정
        if (isGrounded && bodyTransform != null)
        {
            bodyTransform.localRotation = Quaternion.Euler(0, 0, -90f); // 아래 방향
        }
    }

    void RotateArrow()
    {
        if (bodyTransform == null) return;

        // 현재 body 위치
        Vector3 currentBodyPosition = bodyTransform.position;
        
        // 이동 방향 계산 (velocity)
        Vector2 velocity = (currentBodyPosition - previousBodyPosition) / Time.fixedDeltaTime;
        
        // velocity가 충분히 크면 회전
        if (velocity.magnitude > 0.5f)
        {
            // 각도 계산 (화살촉이 이동 방향을 향하도록)
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            
            // 로컬 rotation 사용
            bodyTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        
        // 이전 위치 업데이트
        previousBodyPosition = currentBodyPosition;
    }

    void OnGrounded()
    {
        isGrounded = true;
        
        Debug.Log("BowProjectile: Arrow landed, starting ground damage");
        
        // 지면 데미지 코루틴 시작
        if (groundDamageCo != null)
            StopCoroutine(groundDamageCo);
        groundDamageCo = StartCoroutine(GroundDamageCo());
    }

    IEnumerator GroundDamageCo()
    {
        float elapsedTime = 0f;
        
        // 지속 시간 동안 반복
        while (elapsedTime < groundDuration)
        {
            // 주변 적들에게 데미지
            CastGroundDamage();
            
            // 다음 데미지까지 대기
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }
        
        // 비활성화
        Deactivate();
    }

    void CastGroundDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, SizeOfArea, target);
        
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enemy = hit[i].GetComponent<Transform>();
            
            if (enemy.GetComponent<Idamageable>() != null)
            {
                if (enemy.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enemy.transform.position);
                
                GameObject hitEffectObj = GetComponent<HitEffects>()?.hitEffect;
                enemy.GetComponent<Idamageable>().TakeDamage(
                    Damage,
                    KnockBackChance,
                    KnockBackSpeedFactor,
                    transform.position,
                    hitEffectObj
                );
                
                // 데미지 기록
                if (!string.IsNullOrEmpty(WeaponName))
                {
                    DamageTracker.instance.RecordDamage(WeaponName, Damage);
                }
            }
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // Invoke 정리
        CancelInvoke(nameof(Deactivate));
        
        // 코루틴 정리
        if (groundDamageCo != null)
        {
            StopCoroutine(groundDamageCo);
            groundDamageCo = null;
        }
        
        isGrounded = false;
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
}