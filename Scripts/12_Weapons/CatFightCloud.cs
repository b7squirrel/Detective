using UnityEngine;
using System.Collections;

public class CatFightCloud : MonoBehaviour
{
    // 데이터
    public int Damage { get; set; }
    public float KnockBackChance { get; set; }
    public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    public string WeaponName { get; set; }
    public float SizeOfArea { get; set; }
    public float Duration { get; set; }

    [Header("Damage Settings")]
    [SerializeField] float damageInterval = 0.3f; // 데미지 간격
    
    [Header("Target")]
    [SerializeField] LayerMask target;
    
    [Header("Effects (Optional)")]
    [SerializeField] GameObject hitEffect; // 옵션 - null이어도 괜찮음
    [SerializeField] AudioClip hitSFX; // 옵션
    
    [Header("Smoke Sprites")]
    [SerializeField] CatFightCloudSprite[] smokeSprites; // 5개의 연기 스프라이트
    [SerializeField] float maxSmokeStartOffset = 0.3f; // 최대 시작 딜레이
    [SerializeField] Transform smokeTrns; // 전체 크기를 만드는 연기. size of area로 조절
    
    [Header("Claw Slashes")]
    [SerializeField] CatClawSlash[] clawSlashes; // 5개의 발톱 스프라이트
    [SerializeField] float maxClawStartOffset = 0.3f; // 최대 시작 딜레이
    
    [Header("Cat Sounds")]
    [SerializeField] AudioClip[] catMeows; // ✅ 고양이 소리 배열
    [SerializeField] float meowInterval = 0.5f; // ✅ 재생 간격
    
    Coroutine damageCo;
    Coroutine soundCo; // ✅ 사운드 코루틴
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // 애니메이션 시작
        if (anim != null)
        {
            anim.SetTrigger("Start");
        }
    }

    public void Initialize(int damage, float knockBackChance, float knockBackSpeedFactor, bool isCriticalDamage, string weaponName, float sizeOfArea, float duration)
    {
        this.Damage = damage;
        this.KnockBackChance = knockBackChance;
        this.KnockBackSpeedFactor = knockBackSpeedFactor;
        this.IsCriticalDamageProj = isCriticalDamage;
        this.WeaponName = weaponName;
        this.SizeOfArea = sizeOfArea;
        this.Duration = duration;
        
        // 연기 스프라이트 시작 (각각 랜덤 offset)
        StartSmokeSprites();
        
        // 발톱 스프라이트 시작 (각각 랜덤 offset)
        StartClawSlashes();
        
        // 데미지 코루틴 시작
        if (damageCo != null)
            StopCoroutine(damageCo);
        damageCo = StartCoroutine(DamageCo());
        
        // ✅ 사운드 코루틴 시작
        if (soundCo != null)
            StopCoroutine(soundCo);
        soundCo = StartCoroutine(SoundCo());
    }

    void StartSmokeSprites()
    {
        if (smokeSprites == null || smokeSprites.Length == 0)
        {
            Debug.LogWarning("CatFightCloud: No smoke sprites assigned!");
            return;
        }

        for (int i = 0; i < smokeSprites.Length; i++)
        {
            if (smokeSprites[i] != null)
            {
                // 랜덤 offset (0 ~ maxSmokeStartOffset)
                float randomDelay = Random.Range(0f, maxSmokeStartOffset);
                
                // 연기 시작 (delay, radius)
                smokeSprites[i].StartSmoke(randomDelay, SizeOfArea);
            }
        }

        smokeTrns.localScale = (SizeOfArea * 2f) * Vector2.one;
    }

    void StartClawSlashes()
    {
        if (clawSlashes == null || clawSlashes.Length == 0)
        {
            Debug.LogWarning("CatFightCloud: No claw slashes assigned!");
            return;
        }

        for (int i = 0; i < clawSlashes.Length; i++)
        {
            if (clawSlashes[i] != null)
            {
                // 랜덤 offset (0 ~ maxClawStartOffset)
                float randomDelay = Random.Range(0f, maxClawStartOffset);
                
                // 발톱 시작 (delay, radius)
                clawSlashes[i].StartSlash(randomDelay, SizeOfArea);
            }
        }
    }

    IEnumerator DamageCo()
    {
        float elapsedTime = 0f;
        
        // 지속 시간 동안 반복
        while (elapsedTime < Duration)
        {
            // 범위 내 적들에게 데미지
            CastDamage();
            
            // 다음 데미지까지 대기
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }
        // 비활성화
        Deactivate();
    }

    // ✅ 새 코루틴: 고양이 소리 재생
    IEnumerator SoundCo()
    {
        // 배열이 비어있으면 실행하지 않음
        if (catMeows == null || catMeows.Length == 0)
        {
            Debug.LogWarning("CatFightCloud: No cat meow sounds assigned!");
            yield break;
        }

        float elapsedTime = 0f;
        
        // 지속 시간 동안 반복
        while (elapsedTime < Duration)
        {
            // 배열에서 랜덤하게 하나 선택
            int randomIndex = Random.Range(0, catMeows.Length);
            AudioClip selectedMeow = catMeows[randomIndex];
            
            // 사운드 재생
            if (selectedMeow != null)
            {
                SoundManager.instance.Play(selectedMeow);
            }
            
            // 다음 재생까지 대기
            yield return new WaitForSeconds(meowInterval);
            elapsedTime += meowInterval;
        }
    }

    void CastDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, SizeOfArea, target);
        
        for (int i = 0; i < hits.Length; i++)
        {
            Transform enemy = hits[i].transform;
            
            if (enemy.GetComponent<Idamageable>() != null)
            {
                // 데미지 메시지 표시
                if (enemy.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enemy.transform.position);
                
                // 히트 이펙트 찾기 (optional)
                GameObject hitEffectObj = null;
                
                // 1. HitEffects 컴포넌트에서 찾기
                HitEffects hitEffects = GetComponent<HitEffects>();
                if (hitEffects != null)
                {
                    hitEffectObj = hitEffects.hitEffect;
                }
                
                // 2. 직접 할당된 hitEffect 사용
                if (hitEffectObj == null && hitEffect != null)
                {
                    hitEffectObj = hitEffect;
                }
                
                // 데미지 적용 (hitEffectObj는 null이어도 괜찮음)
                enemy.GetComponent<Idamageable>().TakeDamage(
                    Damage,
                    KnockBackChance,
                    KnockBackSpeedFactor,
                    transform.position,
                    hitEffectObj
                );
                
                // 데미지 트래커 기록
                if (!string.IsNullOrEmpty(WeaponName))
                {
                    DamageTracker.instance.RecordDamage(WeaponName, Damage);
                }
            }
        }
        
        // 히트 사운드 재생 (적이 있을 때만)
        if (hits.Length > 0 && hitSFX != null)
        {
            SoundManager.instance.Play(hitSFX);
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    void Deactivate()
    {
        // 모든 연기 스프라이트 정지
        StopSmokeSprites();
        
        // 모든 발톱 스프라이트 정지
        StopClawSlashes();
        
        gameObject.SetActive(false);
    }

    void StopSmokeSprites()
    {
        if (smokeSprites == null) return;

        for (int i = 0; i < smokeSprites.Length; i++)
        {
            if (smokeSprites[i] != null)
            {
                smokeSprites[i].StopSmoke();
            }
        }
    }

    void StopClawSlashes()
    {
        if (clawSlashes == null) return;

        for (int i = 0; i < clawSlashes.Length; i++)
        {
            if (clawSlashes[i] != null)
            {
                clawSlashes[i].StopSlash();
            }
        }
    }

    void OnDisable()
    {
        // 코루틴 정리
        if (damageCo != null)
        {
            StopCoroutine(damageCo);
            damageCo = null;
        }
        
        // ✅ 사운드 코루틴 정리
        if (soundCo != null)
        {
            StopCoroutine(soundCo);
            soundCo = null;
        }
        
        // 연기 스프라이트 정리
        StopSmokeSprites();
        
        // 발톱 스프라이트 정리
        StopClawSlashes();
    }

    // Gizmos로 범위 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SizeOfArea);
    }
}