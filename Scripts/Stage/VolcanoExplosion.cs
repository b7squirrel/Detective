using System.Collections;
using UnityEngine;

public class VolcanoExplosion : MonoBehaviour
{
    [Header("데미지")]
    [SerializeField] int enemyDamage = 100;
    [SerializeField] int playerDamage = 50;

    [Header("범위")]
    [SerializeField] float explosionRange = 3f;

    [Header("넉백")]
    [SerializeField] float knockBackForce = 15f;
    [SerializeField] float knockBackDuration = 0.3f;

    [Header("이펙트")]
    [SerializeField] GameObject hitEffect;

    // animation event로 폭발 타이밍 제어
    public void Explode()
    {
        StartCoroutine(ExplodeCo());
    }

    IEnumerator ExplodeCo()
    {
        yield return null;

        // 적에게 데미지 + 넉백
        Collider2D[] enemyHits = Physics2D.OverlapCircleAll(
            transform.position, explosionRange, LayerMask.GetMask("Enemy"));

        for (int i = 0; i < enemyHits.Length; i++)
        {
            EnemyBase enemy = enemyHits[i].GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(enemyDamage, knockBackForce, knockBackDuration,
                    transform.position, hitEffect);
        }

        // 플레이어에게 데미지 + 넉백
        Collider2D[] playerHits = Physics2D.OverlapCircleAll(
            transform.position, explosionRange, LayerMask.GetMask("Player"));

        for (int i = 0; i < playerHits.Length; i++)
        {
            Character ch = playerHits[i].GetComponent<Character>();
            if (ch != null)
                ch.TakeDamage(playerDamage, EnemyType.Ranged);

            Player pl = playerHits[i].GetComponent<Player>();
            if (pl != null)
            {
                // 폭발 중심에서 플레이어 방향으로 넉백
                Vector2 dir = ((Vector2)playerHits[i].transform.position
                    - (Vector2)transform.position).normalized;
                pl.GetBounced(knockBackForce, dir, knockBackDuration);
            }
        }
    }

    // 애니메이션 이벤트로 비활성화
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}