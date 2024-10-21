using System.Collections;
using UnityEngine;

public class FieldItemEffect : MonoBehaviour
{
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] int bombDamage;
    [SerializeField] GameObject bombHitEffect;
    [SerializeField] GameObject bombExplosionEffect;
    StageEvenetManager stageEventManager;
    Coroutine coStopWatch, coInvincible;
    #region �����ġ
    public void StopEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        if(coStopWatch != null) StopCoroutine(coStopWatch);
        coStopWatch = StartCoroutine(StopEnemiesCo(allEnemies, stopDuration));
    }
    IEnumerator StopEnemiesCo(EnemyBase[] _allEnemies, float _stopDuration)
    {
        if(stageEventManager == null) stageEventManager = FindObjectOfType<StageEvenetManager>();
        stageEventManager.PasueStageEvent(true);

        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i] != null)
            {
                _allEnemies[i].PauseEnemy();
                //_allEnemies[i].SpeedUpEnemy();
            }
        }
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
    }
    #endregion
    #region ����
    public void SetPlayerInvincible()
    {
        if (coInvincible != null) StopCoroutine(coInvincible);

        coInvincible = StartCoroutine(PlayerInvincibleCo());
    }   
    IEnumerator PlayerInvincibleCo()
    {
        GameManager.instance.IsPlayerInvincible = true;
        GameManager.instance.IsPlayerItemInvincible = true;
        yield return new WaitForSeconds(invincibaleDuration);
        GameManager.instance.IsPlayerInvincible = false;
        GameManager.instance.IsPlayerItemInvincible = false;
    }
    #endregion
    #region ��ź
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
}