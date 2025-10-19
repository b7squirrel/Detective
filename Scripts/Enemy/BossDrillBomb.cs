using System;
using System.Collections;
using UnityEngine;

public class BossDrillBomb : MonoBehaviour
{
    [SerializeField] float waitingTime; // 폭탄이 터지기 전까지의 시간
    [SerializeField] int damage;
    [SerializeField] Transform center; // 폭발의 중심
    float radius; // 폭발 범위
    [SerializeField] LayerMask targetLayer; // 플레이어를 선택하기
    [SerializeField] GameObject damageIndicatorPrefab; // 데미지 인디케이터 프리펩
    [SerializeField] GameObject shockWavePrefab;
    Coroutine co;
    Animator anim;
    Action onDie; // 폭탄이 사라질 때 이벤트

    [Header("디버그")]
    [SerializeField] bool isDubugMode;
    [SerializeField] GameObject debugCircleForCheckingRadius; // 반경 체크를 위한 원

    public void SetDamageRadius(float radius)
    {
        this.radius = radius;
    }
    public void Explode()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ExplodeCo());
    }
    IEnumerator ExplodeCo()
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetTrigger("Trigger");
        ShowIndicator();

        yield return new WaitForSeconds(waitingTime);

        Collider2D playerInRange = Physics2D.OverlapCircle(center.position, radius, targetLayer);

        if (isDubugMode)
        {
            GameObject circle = Instantiate(debugCircleForCheckingRadius, center.position, Quaternion.identity);
            circle.transform.position = center.position;
            circle.transform.localScale = radius * Vector2.one;
        }

        if (playerInRange != null)
        {
            if (playerInRange.GetComponent<Character>() != null)
            {
                playerInRange.GetComponent<Character>().TakeDamage(damage, EnemyType.Melee);
            }
        }

        GameObject shockWave = GameManager.instance.poolManager.GetMisc(shockWavePrefab);
        shockWave.GetComponent<Shockwave>().Init(0, radius, LayerMask.GetMask("Player"), center.position);

        onDie?.Invoke(); // 연결되어 있던 인디케이터를 비활성화
        gameObject.SetActive(false);
    }


    void ShowIndicator()
    {
        GameObject damageIndicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        damageIndicator.transform.position = center.position;
        damageIndicator.transform.localScale = radius * Vector2.one;

        DamageIndicator indicator = damageIndicator.GetComponent<DamageIndicator>();
        onDie += indicator.DeactivateIndicator; // BossDrillBomb에서 직접 등록
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인 가능)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
