using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItemEffect : MonoBehaviour
{
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] int bombDamage;
    [SerializeField] GameObject bombHitEffect;
    [SerializeField] GameObject bombExplosionEffect;
    StageEvenetManager stageEventManager;
    #region 胶砰况摹
    public void StopEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        Debug.Log("ALL ENEMY NUM = " + allEnemies.Length);
        if (allEnemies == null) return;
        StartCoroutine(StopEnemiesCo(allEnemies, stopDuration));
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
            }
        }
        yield return new WaitForSeconds(_stopDuration);

        stageEventManager.PasueStageEvent(false);

        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i].gameObject.activeSelf)
            {
                //enemy.ResumeEnemy();
                Debug.Log("ENEMY " + i);
                _allEnemies[i].ResumeEnemy();
            }
        }
    }
    #endregion
    #region 公利
    public void SetPlayerInvincible()
    {
        StartCoroutine(PlayerInvincibleCo());
    }   
    IEnumerator PlayerInvincibleCo()
    {
        GameManager.instance.IsPlayerInvincible = true;
        yield return new WaitForSeconds(invincibaleDuration);
        GameManager.instance.IsPlayerInvincible = false;
    }
    #endregion
    #region 气藕
    public void Explode(Vector2 _pos)
    {
        GameObject effect = GameManager.instance.poolManager.GetMisc(bombExplosionEffect);
        effect.transform.position = _pos;

        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        Debug.Log("Enmey number = " + allEnemies.Length);
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
        Debug.Log("Number = " + number);
        Debug.Log("null Numbers = " + nullNumbers);
        Debug.Log("not aCtive = " + notActiveNumbers);
    }
    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, false);
    }
    #endregion
}