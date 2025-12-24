using System.Collections;
using UnityEngine;

public class EnemyProjectileBomb : MonoBehaviour, IEnemyProjectile
{
    [SerializeField] float waitingTime; // 폭탄이 터지기 전까지의 시간
    int projectileDamage;
    [SerializeField] float radius; // 폭발 범위
    [SerializeField] LayerMask targetLayer; // 플레이어를 선택하기
    [SerializeField] Transform projBody; // 실제 투사체 스프라이트 위치에 인디케이터를 위치시키기 위해
    [SerializeField] ShadowHeightProjectile shadowHeightProj;
    Coroutine co;
    [SerializeField] Animator anim;

    [Header("이펙트")]
    [SerializeField] GameObject explosionEffectPrefab;
    [SerializeField] GameObject shockWavePrefab;
    [SerializeField] GameObject damageIndicatorPrefab;
    DamageIndicator indicator; // 나중에 비활성화 시키기 위해

    [Header("사운드")]
    [SerializeField] AudioClip initSound;
    [SerializeField] AudioClip explosionSound;

    [Header("디버그")]
    [SerializeField] bool isDubugMode;
    [SerializeField] GameObject debugCircleForCheckingRadius; // 반경 체크를 위한 원
    void OnEnable()
    {
        // 사운드
        if(initSound != null) SoundManager.instance.Play(initSound);
    }
    public void InitBomb()
    {
        if (co != null) co = null;

        // 범위 인디케이터 생성
        indicator = null;
        GameObject damageIndicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        indicator = damageIndicator.GetComponent<DamageIndicator>();
        indicator.Init(radius, transform.position);

        StartCoroutine(ExplodeCo());
    }

    // 진동하는 애니메이션을 시작하는 것만 실행. 애니메이션의 끝에 애니메이션 이벤트로 Cast Damage 함수 실행
    IEnumerator ExplodeCo()
    {
        yield return new WaitForSeconds(waitingTime);
        anim.SetTrigger("Trigger");
    }

    public void CastDamage()
    {
        GenEffects(); //이펙트 발생
        DeactivateIndicator(); // 인디케이터는 비활성화

        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, radius - .5f, targetLayer);

        if (playerInRange != null)
        {
            Debug.LogError($"투사체 데미지 = {projectileDamage}");
            Character character = GameManager.instance.character;
            if (character != null)
            {
                character.TakeDamage(projectileDamage, EnemyType.Melee);
            }
        }
        gameObject.SetActive(false);
    }

    // 이펙트
    void GenEffects()
    {
        // 폭발 이펙트
        GameObject explosion = GameManager.instance.poolManager.GetMisc(explosionEffectPrefab);
        explosion.GetComponent<ExplosionEffect>().Init(radius, transform.position);

        // 연기 등의 쇼크웨이브
        GameObject shockWave = GameManager.instance.poolManager.GetMisc(shockWavePrefab);
        shockWave.GetComponent<Shockwave>().Init(0, radius, LayerMask.GetMask("Player"), transform.position);

        // 사운드 이펙트
        if (explosionSound != null) SoundManager.instance.Play(explosionSound);
        
        // 카메라 쉐이크
        CameraShake.instance.Shake();
    }
    void DeactivateIndicator()
    {
        indicator.gameObject.SetActive(false);
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인 가능)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawWireSphere(transform.position, radius - .5f);
    }

    // 데미지 초기화
    public void InitProjectileDamage(int damage)
    {
        projectileDamage = damage;
        // Logger.LogError($"Projectile Damage Set to {projectileDamage}");
    }
}
