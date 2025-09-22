using System.Collections;
using UnityEngine;

public class EnemyProjectileBomb : MonoBehaviour, IEnemyProjectile
{
    [SerializeField] float waitingTime; // 폭탄이 터지기 전까지의 시간
    int projectileDamage;
    [SerializeField] float radius; // 폭발 범위
    [SerializeField] LayerMask targetLayer; // 플레이어를 선택하기
    [SerializeField] Transform damageIndicator; // 데미지 인디케이터 스케일 조절
    [SerializeField] Transform projBody; // 실제 투사체 스프라이트 위치에 인디케이터를 위치시키기 위해
    [SerializeField] ShadowHeightProjectile shadowHeightProj;
    Coroutine co;
    [SerializeField] Animator anim;
    bool initDone; // Init Bomb을 한 번만 실행하도록 하기 위해

    [Header("디버그")]
    [SerializeField] bool isDubugMode; 
    [SerializeField] GameObject debugCircleForCheckingRadius; // 반경 체크를 위한 원

    void OnEnable()
    {
        damageIndicator.gameObject.SetActive(false);
    }

    // 지면에 닿아서 움직이지 않게 되면 
    void Update()
    {   if (initDone) return;
        if (shadowHeightProj.GetIsDone())
        {
            InitBomb();
            initDone = true;
        }
    }
    void InitBomb()
    {
        if (co != null) co = null;
        damageIndicator.gameObject.SetActive(false);
        damageIndicator.localScale = radius * Vector2.one;

        StartCoroutine(ExplodeCo());
    }

    // 진동하는 애니메이션을 시작하는 것만 실행. 애니메이션의 끝에 애니메이션 이벤트로 Cast Damage 함수 실행
    IEnumerator ExplodeCo()
    {
        damageIndicator.gameObject.SetActive(true);
        damageIndicator.position = projBody.position;

        yield return new WaitForSeconds(waitingTime);

        // if (anim == null) anim = GetComponent<Animator>();
        anim.SetTrigger("Trigger");

    }

    public void CastDamage()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, radius, targetLayer);

        // if (isDubugMode)
        // {
        //     GameObject circle = Instantiate(debugCircleForCheckingRadius, transform.position, Quaternion.identity);
        //     circle.transform.position = transform.position;
        //     circle.transform.localScale = radius * Vector2.one;
        // }

        if (playerInRange != null)
        {
            if (playerInRange.GetComponent<Character>() != null)
            {
                playerInRange.GetComponent<Character>().TakeDamage(projectileDamage, EnemyType.Melee);
            }
        }

        damageIndicator.gameObject.SetActive(false);

        initDone = false;
        gameObject.SetActive(false);
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인 가능)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    // 데미지 초기화
    public void InitProjectileDamage(int damage)
    {
        projectileDamage = damage;
    }
}
