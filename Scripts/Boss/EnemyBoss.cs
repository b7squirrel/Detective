using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [SerializeField] EnemyData[] projectiles;
    [SerializeField] int numberOfProjectile;
    [SerializeField] float timeToAttack;
    float timer;

    public Coroutine shootCoroutine;

    bool wallCreated;

    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
        spawner = FindObjectOfType<Spawner>();
    }
    public void ShootTimer()
    {
        if (timer < timeToAttack)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0f;
        anim.SetTrigger("Shoot");
    }

    #region Shooting Functions
    public void ShootProjectile()
    {
        StartCoroutine(ShootProjectileCo());
    }
    IEnumerator ShootProjectileCo()
    {
        Debug.Log("Here");
        for (int i = 0; i < numberOfProjectile; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, projectiles.Length);
            spawner.SpawnEnemiesToShoot(projectiles[randomNum], (int)SpawnItem.enemy, transform.position, Target.position);
            yield return new WaitForSeconds(.1f);
        }
    }
    public void StopShooting()
    {
        if (shootCoroutine == null)
            return;
        StopCoroutine(shootCoroutine);
    }
    #endregion

    public override void TakeDamage(int damage, float knockBackChance)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            anim.SetTrigger("Hit");
        }
        base.TakeDamage(damage, knockBackChance);
    }
}
