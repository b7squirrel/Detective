using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("데미지")]
    [SerializeField] int playerDamage = 80;
    [SerializeField] int enemyDamage = 150;

    [Header("범위")]
    [SerializeField] float explosionRange = 2.5f;

    [Header("넉백")]
    [SerializeField] float knockBackForce = 12f;
    [SerializeField] float knockBackDuration = 0.3f;

    [Header("낙하 속도")]
    [SerializeField] float fallDuration = 0.4f; // 위→착지 시간

    [Header("이펙트")]
    [SerializeField] GameObject hitEffect;
    FireballIndicator indicator;

    void OnEnable()
    {
        // 풀에서 꺼낼 때 초기 상태로 리셋
        indicator = null;
        StopAllCoroutines();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Init(Vector2 targetPos, FireballIndicator _indicator) // 매개변수 추가
    {
        indicator = _indicator;
        // transform.localScale = Vector3.one * 3f;
        StartCoroutine(FallCo(targetPos));
    }

    IEnumerator FallCo(Vector2 targetPos)
    {
        // 카메라 상단 밖에서 시작
        float cameraHeight = Camera.main.orthographicSize;
        Vector2 startPos = new Vector2(targetPos.x, targetPos.y + cameraHeight * 2f);
        transform.position = startPos;

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            if (!GameManager.instance.IsPaused)
            {
                float progress = elapsed / fallDuration;
                float easedProgress = progress * progress;
                transform.position = Vector2.Lerp(startPos, targetPos, easedProgress);
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        transform.position = targetPos;
        Explode();
    }

    void Explode()
    {
        if (indicator != null)
        {
            indicator.Hide();
            indicator = null;
        }

        // 적 데미지 + 넉백
        Collider2D[] enemyHits = Physics2D.OverlapCircleAll(
            transform.position, explosionRange, LayerMask.GetMask("Enemy"));

        foreach (var hit in enemyHits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(enemyDamage, knockBackForce, knockBackDuration,
                    transform.position, hitEffect);
        }

        Collider2D[] playerHits = Physics2D.OverlapCircleAll(
            transform.position, explosionRange, LayerMask.GetMask("Player"));

        foreach (var hit in playerHits)
        {
            Character ch = hit.GetComponent<Character>();
            if (ch != null)
                ch.TakeDamage(playerDamage, EnemyType.Ranged);

            Player pl = hit.GetComponent<Player>();
            if (pl != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position
                    - (Vector2)transform.position).normalized;
                pl.GetBounced(knockBackForce, dir, knockBackDuration);
            }
        }

        gameObject.SetActive(false);
    }
}