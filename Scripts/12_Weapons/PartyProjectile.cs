using System.Collections;
using UnityEngine;

public class PartyProjectile : MonoBehaviour
{
    // 데이터
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }

    [Header("Ground Behavior")]
    [SerializeField] float groundDuration = 3f; // 지면에 머무는 시간
    [SerializeField] float damageInterval = 0.5f; // 데미지 간격
    
    [Header("Effects")]
    [SerializeField] GameObject hitEffect;
    [SerializeField] GameObject starEffect;
    [SerializeField] AudioClip hitSFX;
    [SerializeField] GameObject groundLoopEffect; // 지면에 있을 때 반복 이펙트 (옵션)
    
    [Header("Target")]
    [SerializeField] LayerMask target;

    ShadowHeight shadowHeight;
    bool isGrounded = false; // 완전히 안착했는지
    Coroutine groundDamageCo;
    Animator anim;

    void Awake()
    {
        shadowHeight = GetComponent<ShadowHeight>();
        
        if (shadowHeight == null)
        {
            Debug.LogError("PartyProjectile: ShadowHeight component not found!");
        }
    }

    void OnEnable()
    {
        isGrounded = false;
        
        // TrailRenderer 초기화는 PartyWeapon에서 하지만, 여기서도 확인
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = true;
    }

    void FixedUpdate() // Update → FixedUpdate
    {
        if (shadowHeight == null) return;

        // 바운스가 완전히 끝났는지 체크
        if (!isGrounded && shadowHeight.IsDone)
        {
            OnFullyGrounded();
        }
    }

    void OnFullyGrounded()
    {
        isGrounded = true;
        Debug.Log("PartyProjectile: Fully grounded, starting ground damage");
        
        if(anim == null) anim = GetComponentInChildren<Animator>();
        anim.SetTrigger("Landed");

        // 지면 데미지 코루틴 시작
        if (groundDamageCo != null)
            StopCoroutine(groundDamageCo);
        groundDamageCo = StartCoroutine(GroundDamageCo());
    }

    IEnumerator GroundDamageCo()
    {
        float elapsedTime = 0f;
        
        // 지면 이펙트 생성 (옵션)
        GameObject loopEffect = null;
        if (groundLoopEffect != null)
        {
            loopEffect = GameManager.instance.poolManager.GetMisc(groundLoopEffect);
            if (loopEffect != null)
            {
                loopEffect.transform.position = transform.position;
                loopEffect.transform.localScale = Vector2.one * SizeOfArea;
            }
        }
        
        // 지속 시간 동안 반복
        while (elapsedTime < groundDuration)
        {
            // 주변 적들에게 데미지
            CastGroundDamage();
            
            // 다음 데미지까지 대기
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }
        
        // 지면 이펙트 종료
        if (loopEffect != null)
        {
            loopEffect.SetActive(false);
        }
        
        // 최종 폭발 (옵션)
        // Explode();
        
        // 비활성화
        Deactivate();
    }

    void CastGroundDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 2f, target);
        
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

    void Deactivate()
    {
        // Trail 비활성화
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false;
        
        gameObject.SetActive(false);
    }

    // 나중에 사용할 폭발 메서드 (현재는 주석 처리)
    public void Explode()
    {
        GenerateHitEffect();
        SoundManager.instance.Play(hitSFX);
        CastGroundDamage(); // 마지막 데미지
        
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false;
        
        gameObject.SetActive(false);
    }

    void GenerateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject smoke = GameManager.instance.poolManager.GetMisc(hitEffect);
            smoke.transform.position = transform.position;
            smoke.transform.localScale = Vector2.one * SizeOfArea;
        }
        
        if (starEffect != null)
        {
            GameObject stars = GameManager.instance.poolManager.GetMisc(starEffect);
            stars.transform.position = transform.position;
            stars.transform.localScale = Vector2.one * SizeOfArea;
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void OnDisable()
    {
        // 코루틴 정리
        if (groundDamageCo != null)
        {
            StopCoroutine(groundDamageCo);
            groundDamageCo = null;
        }
        isGrounded = false;
    }
}