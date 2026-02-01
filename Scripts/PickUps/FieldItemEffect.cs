using System.Collections;
using UnityEngine;

public class FieldItemEffect : MonoBehaviour
{
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] CountdownTimer stopCounterUI;
    [SerializeField] InvincibleCounterUI invincibleCounterUI;
    [Header("폭탄 설정")]
    [SerializeField] int bombDamage;
    [SerializeField] float bombRadius = 5f; // 폭탄 폭발 범위
    [SerializeField] LayerMask enemyLayer; // 적 레이어
    [SerializeField] GameObject bombHitEffect;
    [SerializeField] GameObject bombExplosionEffect;
    [SerializeField] GameObject damageIndicatorPrefab; // 디버그용 인디케이터
    [SerializeField] float indicatorDisplayTime = 0.5f; // 인디케이터 표시 시간
    [SerializeField] GameObject itemDieEffect; // 상자, 보석 등이 사라질 때의 이펙트
    ISpawnController spawnController;

    Coroutine coStopWatch, coInvincible;
    bool isStoppedWithStopwatch = false; // 스톱워치로 시간을 멈추었을 때

    void Start()
    {
        // 어떤 스폰 컨트롤러든 찾기
        spawnController = FindObjectOfType<StageEvenetManager>() as ISpawnController;

        if (spawnController == null)
        {
            spawnController = FindObjectOfType<InfiniteStageManager>() as ISpawnController;
        }

        if (spawnController == null)
        {
            Logger.LogWarning("[FieldItemEffect] No spawn controller found!");
        }

        stopCounterUI.gameObject.SetActive(false);
    }
    #region 시간정지
    public void StopEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        if (coStopWatch != null) StopCoroutine(coStopWatch);
        coStopWatch = StartCoroutine(StopEnemiesCo(allEnemies, stopDuration));
        stopCounterUI.StartTimer(stopDuration);

    }
    IEnumerator StopEnemiesCo(EnemyBase[] _allEnemies, float _stopDuration)
    {
        // 스폰 컨트롤러가 있으면 일시정지
        if (spawnController != null)
        {
            spawnController.PauseSpawn(true);
            Logger.Log("[FieldItemEffect] 스폰이 정지되었습니다.");
        }
        else
        {
            Logger.LogWarning("[FieldItemEffect] 어떤 종류의 Spawn Controller도 없습니다.");
        }

        stopCounterUI.gameObject.SetActive(true);

        // 적들 일시정지
        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i] != null)
            {
                _allEnemies[i].PauseEnemy();
            }
        }

        isStoppedWithStopwatch = true;

        yield return new WaitForSeconds(_stopDuration);

        // 스폰 재개
        if (spawnController != null)
        {
            spawnController.PauseSpawn(false);
            Logger.Log("[FieldItemEffect] Spawn resumed");
        }

        // ⭐ 수정: 시간 정지 종료 시점에 모든 적을 다시 찾아서 Resume
        EnemyBase[] allCurrentEnemies = FindObjectsOfType<EnemyBase>();
        for (int i = 0; i < allCurrentEnemies.Length; i++)
        {
            if (allCurrentEnemies[i] != null && allCurrentEnemies[i].gameObject.activeSelf)
            {
                allCurrentEnemies[i].ResumeEnemy();
            }
        }

        isStoppedWithStopwatch = false;
        stopCounterUI.gameObject.SetActive(false);
    }
    public bool IsStopedWithStopwatch()
    {
        // 스톱워치로 시간이 멈추었는지
        return isStoppedWithStopwatch;
    }
    #endregion
    #region 무적
    public void SetPlayerInvincible()
    {
        if (coInvincible != null) StopCoroutine(coInvincible);

        coInvincible = StartCoroutine(PlayerInvincibleCo());
    }
    IEnumerator PlayerInvincibleCo()
    {
        GameManager.instance.IsPlayerInvincible = true;
        GameManager.instance.IsPlayerItemInvincible = true;

        invincibleCounterUI.gameObject.SetActive(true);

        Animator counterAnim = invincibleCounterUI.GetComponent<Animator>();

        int remainingTime = Mathf.CeilToInt(invincibaleDuration);
        invincibleCounterUI.SetCountNumber(remainingTime);

        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);  // 1초 기다림
            remainingTime--;
            invincibleCounterUI.SetCountNumber(remainingTime);
            counterAnim.SetTrigger("Pop");
        }

        GameManager.instance.IsPlayerInvincible = false;
        GameManager.instance.IsPlayerItemInvincible = false;
        invincibleCounterUI.gameObject.SetActive(false);
    }
    #endregion
    #region 폭탄
    public void Explode(Vector2 _pos)
    {
        // 디버그 인디케이터 표시
        if (damageIndicatorPrefab != null)
        {
            StartCoroutine(ShowBombIndicator(_pos));
        }

        // 폭발 이펙트
        GameObject effect = GameManager.instance.poolManager.GetMisc(bombExplosionEffect);
        effect.transform.position = _pos;

        // 범위 내의 적만 찾기
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(_pos, bombRadius, enemyLayer);

        if (enemiesInRange.Length == 0)
        {
            Logger.Log("[FieldItemEffect] 폭탄 범위 내에 적이 없습니다.");
            return;
        }

        int damagedEnemies = 0;
        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            Idamageable enemy = enemiesInRange[i].GetComponent<Idamageable>();
            GameObject enemyObject = enemiesInRange[i].gameObject;

            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(bombDamage, enemiesInRange[i].transform.position);

                enemy.TakeDamage(bombDamage,
                                 0,
                                 0,
                                 _pos,
                                 bombHitEffect);
                damagedEnemies++;
            }
        }

        Logger.Log($"[FieldItemEffect] 폭탄으로 {damagedEnemies}마리의 적에게 데미지를 입혔습니다.");
    }

    IEnumerator ShowBombIndicator(Vector2 _pos)
    {
        // 인디케이터 생성
        GameObject indicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        DamageIndicator damageIndicator = indicator.GetComponent<DamageIndicator>();

        if (damageIndicator != null)
        {
            damageIndicator.Init(bombRadius, _pos);
        }

        // 지정된 시간만큼 표시
        yield return new WaitForSeconds(indicatorDisplayTime);

        // 인디케이터 비활성화
        indicator.SetActive(false);
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, false);
    }
    #endregion
    #region 모든 적 제거
    public void RemoveAllEnemy()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        foreach (var item in allEnemies)
        {
            item.DieOnBossEvent();
        }
    }
    #endregion

    #region 모든 보석 제거
    public void RemoveAllGems()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 200f);
        foreach (var item in hits)
        {
            Collectable collectable = item.GetComponent<Collectable>();
            if (collectable != null)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
                if (effect != null) effect.transform.position = collectable.transform.position;
                collectable.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 모든 상자 제거
    public void RemoveAllChests()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 200f);
        foreach (var item in hits)
        {
            DestructableObject DestructableObject = item.GetComponent<DestructableObject>();
            if (DestructableObject != null)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
                if (effect != null) effect.transform.position = DestructableObject.transform.position;
                DestructableObject.gameObject.SetActive(false);
            }
        }
    }
    #endregion
}