using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [SerializeField] EnemyData[] projectiles;
    [SerializeField] int numberOfProjectile;
    [SerializeField] int maxProjectile;
    [SerializeField] float timeToAttack;

    [SerializeField] Transform ShootPoint;
    [SerializeField] Transform dustPoint;
    [SerializeField] GameObject dustEffect;
    GenerateWalls generateWalls;
    float timer;

    public Coroutine shootCoroutine;
    bool wallCreated;

    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
        spawner = FindObjectOfType<Spawner>();
        generateWalls = GetComponent<GenerateWalls>();

        float randomX = UnityEngine.Random.Range(Target.position.x - 5f, Target.position.x + 5f);
        float randomY = UnityEngine.Random.Range(Target.position.y - 10f, Target.position.y + 10f);
        transform.position = new Vector2(randomX, randomY);
    }
    public void ShootTimer()
    {
        if (timer < timeToAttack)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0f;
        anim.SetBool("ShootFinished", false);
        anim.SetTrigger("Shoot");
    }

    #region Shooting Functions
    public void ShootProjectile()
    {
        StartCoroutine(ShootProjectileCo());
    }
    IEnumerator ShootProjectileCo()
    {
        for (int i = 0; i < numberOfProjectile; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, projectiles.Length);
            spawner.SpawnEnemiesToShoot(projectiles[randomNum], (int)SpawnItem.enemy, ShootPoint.position, Target.position);
            yield return new WaitForSeconds(.1f);
        }

        numberOfProjectile += 5;
        if (numberOfProjectile > maxProjectile)
        {
            numberOfProjectile = maxProjectile;
        }

        StartCoroutine(ShootFinishedCo());
    }

    IEnumerator ShootFinishedCo()
    {
        yield return new WaitForSeconds(.5f);
        anim.SetBool("ShootFinished", true);
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
    //animation events
    public void GenerateSpawnDust()
    {
        Instantiate(dustEffect, dustPoint.position, Quaternion.identity);
    }
    public void TriggerWallGenerator()
    {
        generateWalls.GenWalls();
    }
}
