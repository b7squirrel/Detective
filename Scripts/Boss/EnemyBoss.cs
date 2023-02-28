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
    GameObject dust;
    [SerializeField] GameObject teleportEffect;
    GenerateWalls generateWalls;
    float timer;

    public Coroutine shootCoroutine;
    bool wallCreated;

    public void Init(EnemyData data)
    {
        this.Stats = new EnemyStats(data.stats);
        spawner = FindObjectOfType<Spawner>();
        generateWalls = GetComponent<GenerateWalls>();

        // 플레이어 중심으로 X(-3, 3) Y(-10, 10) 가장자리에 보스 생성
        float f = UnityEngine.Random.value > .5f ? -1f : 1f;
        float randomX = 0f;
        float randomY = 0f;
        if (UnityEngine.Random.value > .5f)
        {
            randomX = UnityEngine.Random.Range(Target.position.x - 3f, Target.position.x + 3f);
            randomY = Target.position.y + (f * 10f);
        }
        else
        {
            randomY = UnityEngine.Random.Range(Target.position.y - 5f, Target.position.y + 5f);
            randomX = Target.position.x + (f * 3f);
        }
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
            Vector2 offsetPos = Target.position + new Vector2(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-4f, 4f));
            spawner.SpawnEnemiesToShoot(projectiles[randomNum], (int)SpawnItem.enemy, ShootPoint.position, offsetPos);
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
        dust = Instantiate(dustEffect, dustPoint.position, Quaternion.identity);
    }
    public void DestroySpawnDust()
    {
        Destroy(dust);
    }
    public void TriggerWallGenerator()
    {
        if(generateWalls == null)
            return;
        generateWalls.GenWalls();
    }
}
