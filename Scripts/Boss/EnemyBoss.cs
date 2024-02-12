using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [field: SerializeField] public float moveSpeedInAir { get; private set; }
    [field: SerializeField] public bool IsInAir { get; set; }

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

    SpriteRenderer spriteRen;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject deadBody;


    public Coroutine shootCoroutine;
    bool wallCreated;

    [SerializeField] BossHealthBar bossHealthBar;

    [SerializeField] float landingImpactSize;
    [SerializeField] float landingImpactForce;
    [SerializeField] LayerMask landingHit;
    [SerializeField] GameObject SpawnTeleportEffect;
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
        col = GetComponent<CapsuleCollider2D>();
        spriteRen = GetComponentInChildren<SpriteRenderer>();

        bossHealthBar = FindObjectOfType<BossHealthBar>();
        bossHealthBar.InitHealthBar(Stats.hp, Name);

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
        yield return new WaitForSeconds(1f);
        anim.SetBool("ShootFinished", true);
    }
    public void StopShooting()
    {
        if (shootCoroutine == null)
            return;
        StopCoroutine(shootCoroutine);
    }
    #endregion

    #region TakeDamage Funcitons
    public override void TakeDamage(int damage, float knockBackChance, float knockBackSpeedFactor, Vector2 target, GameObject hitEffect)
    {
        if (IsInAir)
            return;
        base.TakeDamage(damage, knockBackChance, knockBackSpeedFactor, target, hitEffect);
        bossHealthBar.UpdateBossHealthSlider(Stats.hp);
    }
    public override void Die()
    {
        base.Die();

        //GetComponent<DropOnDestroy>().CheckDrop();

        SoundManager.instance.Play(dieSFX);
        anim.SetTrigger("Die");

        BossDieManager.instance.Init(deadBody, transform, 25);
        gameObject.SetActive(false);
    }
    #endregion

    #region Animation Events
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
    #endregion

    #region State Functions
    public void LandingImpact()
    {
        GameObject effect = Instantiate(landingEffect);
        SoundManager.instance.Play(landingSFX);
        effect.transform.position = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, landingImpactSize, landingHit);

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyBase hit = hits[i].GetComponent<EnemyBase>();

            if (hit != null && !hit.IsBoss) // 보스 랜딩 공격에 자신까지 포함시키지 않기
            {
                if (hits[i].CompareTag("Enemy"))
                {
                    hit.Stunned(transform.position);
                }
            }
        }
    }
    #endregion
}
