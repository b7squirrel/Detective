using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [field : SerializeField] public float moveSpeedInAir {get; private set;}
    [SerializeField] EnemyData[] projectiles;
    [SerializeField] AudioClip[] projectileSFX;
    [SerializeField] int numberOfProjectile;
    [SerializeField] int maxProjectile;
    [SerializeField] float timeToAttack;

    [SerializeField] Transform ShootPoint;
    [SerializeField] Transform dustPoint;
    [SerializeField] GameObject dustEffect;
    GameObject dust;
    [SerializeField] GameObject teleportEffect;
    [SerializeField] int halfWallBouncerNumber;
    GenerateWalls generateWalls;
    float timer; // shoot coolTime counter

    [SerializeField] Collider2D col;
    [SerializeField] GameObject rewards;
    [SerializeField] GameObject defeatedBossPrefab;

    public Coroutine shootCoroutine;
    bool wallCreated;

    [SerializeField] BossHealthBar bossHealthBar;

    [SerializeField] float landingImpactSize;
    [SerializeField] float landingImpactForce;
    [SerializeField] LayerMask landingHit;
    [SerializeField] GameObject landingEffect;
    [SerializeField] AudioClip spawnSFX;
    [SerializeField] AudioClip landingSFX;
    [SerializeField] AudioClip shootAnticSFX;
    [SerializeField] AudioClip jumpupSFX;
    [SerializeField] AudioClip fallDownSFX;
    [SerializeField] AudioClip dieSFX;

    public void Init(EnemyData data)
    {
        IsBoss = true;
        this.Stats = new EnemyStats(data.stats);
        spawner = FindObjectOfType<Spawner>();
        generateWalls = GetComponent<GenerateWalls>();
        col = GetComponent<Collider2D>();

        bossHealthBar = FindObjectOfType<BossHealthBar>();
        bossHealthBar.InitHealthBar(Stats.hp, Name);

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

        SoundManager.instance.Play(spawnSFX);
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
    public void PlayShootAnticSound()
    {
        SoundManager.instance.Play(shootAnticSFX);
    }
    public void PlayJumpUpSound()
    {
        SoundManager.instance.Play(jumpupSFX);
    }
    public void PlayFallDownSound()
    {
        SoundManager.instance.Play(fallDownSFX);
    }
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
            SoundManager.instance.Play(projectileSFX[randomNum]);
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

    public override void TakeDamage(int damage, float knockBackChance, Vector2 target)
    {
        base.TakeDamage(damage, knockBackChance, target);
        bossHealthBar.UpdateBossHealthSlider(Stats.hp);
    }
    public override void Die()
    {
        GameObject reward = Instantiate(rewards, transform.position, Quaternion.identity);
        reward.GetComponent<DropCoins>().Init();
        GetComponent<DropOnDestroy>().CheckDrop();

        SoundManager.instance.Play(dieSFX);
        anim.SetTrigger("Die");

        Instantiate(defeatedBossPrefab,transform.position, transform.rotation);
        MusicManager.instance.Stop();
        GameManager.instance.GetComponent<BossHealthBarManager>().DeActivateBossHealthBar();
        
        
        gameObject.SetActive(false);
                                                                                  
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
        if (generateWalls == null)
            return;
        generateWalls.GenWalls(halfWallBouncerNumber);
    }
    public void SetLayer(string layer)
    {
        gameObject.layer = LayerMask.NameToLayer(layer);
    }
    public void EnableCollider()
    {
        col.enabled = true;
    }
    public void DisableCollier()
    {
        col.enabled = false;
    }
    public void CamShake()
    {
        CameraShake.insstance.Shake();
    }
    public void StartLanding()
    {
        anim.SetTrigger("Land");
    }
    public void LandingImpact()
    {
        GameObject effect = Instantiate(landingEffect);
        SoundManager.instance.Play(landingSFX);
        effect.transform.position = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, landingImpactSize, landingHit);
        
        foreach (Collider2D item in hits)
        {
            EnemyBase hit = item.GetComponent<EnemyBase>();

            if (hit != null && !hit.IsBoss) // 보스 랜딩 공격에 자신까지 포함시키지 않기
            {
                if (item.CompareTag("Enemy"))
                {
                    hit.Stunned(transform.position);
                }
            }
        }
    }
    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, landingImpactSize);
    }
}
