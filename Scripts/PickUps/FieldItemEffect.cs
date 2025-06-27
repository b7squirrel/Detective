using System.Collections;
using UnityEngine;

public class FieldItemEffect : MonoBehaviour
{
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] GameObject invincibleCounterUIPrefab;
    [SerializeField] int bombDamage;
    [SerializeField] GameObject bombHitEffect;
    [SerializeField] GameObject bombExplosionEffect;
    StageEvenetManager stageEventManager;
    Coroutine coStopWatch, coInvincible;
    bool isStoppedWithStopwatch = false; // 스톱워치로 시간을 멈추었을 때
    #region 시간정지
    public void StopEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        if (coStopWatch != null) StopCoroutine(coStopWatch);
        coStopWatch = StartCoroutine(StopEnemiesCo(allEnemies, stopDuration));
    }
    IEnumerator StopEnemiesCo(EnemyBase[] _allEnemies, float _stopDuration)
    {
        if (stageEventManager == null) stageEventManager = FindObjectOfType<StageEvenetManager>();
        stageEventManager.PasueStageEvent(true);

        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i] != null)
            {
                _allEnemies[i].PauseEnemy();
                //_allEnemies[i].SpeedUpEnemy();
            }
        }

        isStoppedWithStopwatch = true;

        yield return new WaitForSeconds(_stopDuration);

        stageEventManager.PasueStageEvent(false);

        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i].gameObject.activeSelf)
            {
                //enemy.ResumeEnemy();
                _allEnemies[i].ResumeEnemy();
            }
        }

        isStoppedWithStopwatch = false;
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

        GameObject invincibleCounter = Instantiate(invincibleCounterUIPrefab, Player.instance.transform);
        invincibleCounter.transform.position = Player.instance.transform.position;

        InvincibleCounterUI counterUI = invincibleCounter.GetComponent<InvincibleCounterUI>();

        int remainingTime = Mathf.CeilToInt(invincibaleDuration);
        counterUI.SetCountNumber(remainingTime);

        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);  // 1초 기다림
            remainingTime--;
            counterUI.SetCountNumber(remainingTime);
        }

        GameManager.instance.IsPlayerInvincible = false;
        GameManager.instance.IsPlayerItemInvincible = false;
        Destroy(invincibleCounter);

    }
    #endregion
    #region 폭탄
    public void Explode(Vector2 _pos)
    {
        GameObject effect = GameManager.instance.poolManager.GetMisc(bombExplosionEffect);
        effect.transform.position = _pos;

        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allEnemies.Length == 0) return;

        int number = 1;
        int nullNumbers = 0;
        int notActiveNumbers = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            Idamageable enemy = allEnemies[i].GetComponent<Idamageable>();
            GameObject enemyObject = allEnemies[i].gameObject;

            if (enemy == null) nullNumbers++;
            if (enemyObject == null) notActiveNumbers++;

            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(bombDamage, allEnemies[i].transform.position);

                number++;
                enemy.TakeDamage(bombDamage,
                                 0,
                                 0,
                                 _pos,
                                 bombHitEffect);
            }
        }
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
}