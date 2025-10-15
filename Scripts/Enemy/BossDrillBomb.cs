using System.Collections;
using UnityEngine;

public class BossDrillBomb : MonoBehaviour
{
    [SerializeField] float waitingTime; // 폭탄이 터지기 전까지의 시간
    [SerializeField] int damage;
    [SerializeField] float radius; // 폭발 범위
    [SerializeField] LayerMask targetLayer; // 플레이어를 선택하기
    [SerializeField] Transform damageIndicator; // 데미지 인디케이터 스케일 조절
    Coroutine co;
    Animator anim;

    [Header("디버그")]
    [SerializeField] bool isDubugMode; 
    [SerializeField] GameObject debugCircleForCheckingRadius; // 반경 체크를 위한 원

    void OnEnable()
    {
        if (co != null) co = null;
        StartCoroutine(ExplodeCo());

        if (damageIndicator != null)
        {
            damageIndicator.gameObject.SetActive(false);
            damageIndicator.localScale = radius * Vector2.one;
        }
    }
    IEnumerator ExplodeCo()
    {
        yield return new WaitForSeconds(waitingTime);
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetTrigger("Trigger");

        if (damageIndicator != null)
        {
            damageIndicator.gameObject.SetActive(true);
            damageIndicator.position = transform.position;
        }

        // yield return new WaitForSeconds(1f); // 폭탄 터지기 직전 애니메이션 길이 1초

        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, radius, targetLayer);

        if (isDubugMode)
        {
            GameObject circle = Instantiate(debugCircleForCheckingRadius, transform.position, Quaternion.identity);
            circle.transform.position = transform.position;
            circle.transform.localScale = radius * Vector2.one;
        }

        if (playerInRange != null)
        {
            if (playerInRange.GetComponent<Character>() != null)
            {
                playerInRange.GetComponent<Character>().TakeDamage(damage, EnemyType.Melee);
            }
        }

        if (damageIndicator != null) damageIndicator.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인 가능)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
