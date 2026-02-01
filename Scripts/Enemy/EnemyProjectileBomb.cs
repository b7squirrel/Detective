using System.Collections;
using UnityEngine;

public class EnemyProjectileBomb : MonoBehaviour, IEnemyProjectile
{
    [SerializeField] float waitingTime;
    int projectileDamage;
    [SerializeField] float radius;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] Transform projBody;
    [SerializeField] ShadowHeightProjectile shadowHeightProj;
    Coroutine co;
    [SerializeField] Animator anim;
    
    [Header("이펙트")]
    [SerializeField] GameObject explosionEffectPrefab;
    [SerializeField] GameObject shockWavePrefab;
    [SerializeField] GameObject damageIndicatorPrefab;
    DamageIndicator indicator;
    
    [Header("사운드")]
    [SerializeField] AudioClip initSound;
    [SerializeField] AudioClip explosionSound;
    
    [Header("디버그")]
    [SerializeField] bool isDubugMode;
    [SerializeField] GameObject debugCircleForCheckingRadius;

    // ⭐ 시간 정지 관련 변수 추가
    FieldItemEffect fieldItemEffect;
    float remainingTime; // 남은 폭발 시간
    bool isCountingDown; // 카운트다운 중인지

    void Awake()
    {
        fieldItemEffect = FindObjectOfType<FieldItemEffect>();
    }

    void OnEnable()
    {
        if (initSound != null) SoundManager.instance.Play(initSound);
    }
    
    // Update 추가: 애니메이터 정지/재개 처리
    void Update()
    {
        if (anim == null) return;

        // 시간 정지 체크
        if (fieldItemEffect != null && fieldItemEffect.IsStopedWithStopwatch())
        {
            // 애니메이터 정지
            if (anim.speed != 0f)
            {
                anim.speed = 0f;
            }
        }
        else
        {
            // 애니메이터 재개
            if (anim.speed != 1f)
            {
                anim.speed = 1f;
            }
        }
    }

    public void InitBomb()
    {
        if (co != null) co = null;
        
        indicator = null;
        GameObject damageIndicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        indicator = damageIndicator.GetComponent<DamageIndicator>();
        indicator.Init(radius, transform.position);
        
        // ⭐ 카운트다운 초기화
        remainingTime = waitingTime + UnityEngine.Random.Range(-.5f, .5f);
        isCountingDown = true;
        
        StartCoroutine(ExplodeCo());
    }

    // ⭐ 시간 정지를 고려한 카운트다운
    IEnumerator ExplodeCo()
    {
        while (remainingTime > 0f)
        {
            // 시간 정지 중이 아닐 때만 시간 감소
            if (fieldItemEffect == null || !fieldItemEffect.IsStopedWithStopwatch())
            {
                remainingTime -= Time.deltaTime;
            }
            
            yield return null;
        }

        isCountingDown = false;
        anim.SetTrigger("Trigger");
    }

    public void CastDamage()
    {
        GenEffects();
        DeactivateIndicator();
        
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, radius - .5f, targetLayer);
        if (playerInRange != null)
        {
            Character character = GameManager.instance.character;
            if (character != null)
            {
                character.TakeDamage(projectileDamage, EnemyType.Melee);
            }
        }
        
        gameObject.SetActive(false);
    }

    void GenEffects()
    {
        GameObject explosion = GameManager.instance.poolManager.GetMisc(explosionEffectPrefab);
        explosion.GetComponent<ExplosionEffect>().Init(radius, transform.position);
        
        GameObject shockWave = GameManager.instance.poolManager.GetMisc(shockWavePrefab);
        shockWave.GetComponent<Shockwave>().Init(0, radius, LayerMask.GetMask("Player"), transform.position);
        
        if (explosionSound != null) SoundManager.instance.Play(explosionSound);
        
        CameraShake.instance.Shake();
    }

    void DeactivateIndicator()
    {
        indicator.gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        // Gizmos 코드...
    }

    public void InitProjectileDamage(int damage)
    {
        projectileDamage = damage;
    }
}