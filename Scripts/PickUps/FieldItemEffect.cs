using System.Collections;
using UnityEngine;

public class FieldItemEffect : MonoBehaviour
{
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] int bombDamage;
    [SerializeField] GameObject bombHitEffect;
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
        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i] != null)
            {
                _allEnemies[i].PauseEnemy();
            }
        }
        yield return new WaitForSeconds(_stopDuration);

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
        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allEnemies.Length == 0) return;
        

        for (int i = 0; i < allEnemies.Length; i++)
        {
            Idamageable enemy = allEnemies[i].transform.GetComponent<Idamageable>();
            GameObject enemyObject = allEnemies[i].gameObject;

            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(bombDamage, allEnemies[i].transform.position);

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