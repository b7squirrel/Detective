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
    [SerializeField] GameObject hitEffectPrefab;
    FireballIndicator indicator;

    [Header("폭발 이펙트")]
    [SerializeField] GameObject explosionEffectPrefab;

    [Header("사운드")]
    [SerializeField] AudioClip fallSound;
    [SerializeField] AudioClip explosionSound;
    [SerializeField][Range(0f, 1f)] float fallSoundVolume = 0.6f;
    [SerializeField][Range(0f, 1f)] float explosionSoundVolume = 0.8f;

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

        // 낙하 사운드 재생
        if (fallSound != null)
            SoundManager.instance.PlaySoundWith(fallSound, fallSoundVolume, false, 0.3f);

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
        // indicator 크기를 폭발 범위로 사용
        float range = explosionRange; // 기본값
        if (indicator != null)
            range = indicator.transform.localScale.x * 0.5f; // indicator 반지름

        if (indicator != null)
        {
            indicator.Hide();
            indicator = null;
        }

        // 폭발 사운드
        if (explosionSound != null)
            SoundManager.instance.PlaySoundWith(explosionSound, explosionSoundVolume, false, 0.1f);

        // 카메라 쉐이크
        CameraShake.instance.Shake();

        // 폭발 이펙트
        if (explosionEffectPrefab != null)
        {
            GameObject effect = GameManager.instance.poolManager.GetMisc(explosionEffectPrefab);
            if (effect != null)
            {
                effect.transform.position = transform.position;
                StartCoroutine(DeactivateEffectCo(effect, 2f));
            }
        }

        // 적 데미지 + 넉백
        Collider2D[] enemyHits = Physics2D.OverlapCircleAll(
            transform.position, range, LayerMask.GetMask("Enemy"));

        foreach (var hit in enemyHits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(enemyDamage, knockBackForce, knockBackDuration,
                    transform.position, hitEffectPrefab);
        }

        // 플레이어 데미지 + 넉백
        Collider2D[] playerHits = Physics2D.OverlapCircleAll(
            transform.position, range, LayerMask.GetMask("Player"));

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

    IEnumerator DeactivateEffectCo(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
    }
}