using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [field: SerializeField] public float moveSpeedInAir { get; private set; }
    [field: SerializeField] public bool IsInAir { get; set; }

    [SerializeField] EnemyData[] projectiles; // 날으는 슬라임 적
    [SerializeField] GameObject slimeProjectilePrefab; // 슬라임 점액
    [SerializeField] float slimeProjectileSpeed; // 슬라임 점액 속도
    [SerializeField] AudioClip[] projectileSFX;
    [SerializeField] int numberOfProjectile;
    [SerializeField] int maxProjectile;
    [SerializeField] float timeToAttack;
    [SerializeField] float timeToDropSlime;

    [SerializeField] Transform ShootPoint;
    [SerializeField] Transform dustPoint;
    [SerializeField] GameObject dustEffect;
    GameObject dust;
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] int halfWallBouncerNumber;
    GenerateWalls generateWalls;
    float timer; // shoot coolTime counter
    float slimeDropTimer; // 슬라임 점액을 떨어트리는 타이밍 카운터
    SlimeDropManager slimeDropManager;

    SpriteRenderer spriteRen;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject deadBody;


    public Coroutine shootCoroutine;
    bool wallCreated;

    WallManager wallManager;
    Vector2 currentPosition;

    [SerializeField] float landingImpactSize;
    [SerializeField] float landingImpactForce;
    [SerializeField] LayerMask landingHit;
    [SerializeField] GameObject LandingIndicatorPrefab;
    [SerializeField] GameObject LandingIndicator;
    [SerializeField] GameObject landingEffect;
    [SerializeField] GameObject teleEffectPrefab;
    [SerializeField] AudioClip spawnSFX;
    [SerializeField] AudioClip landingSFX;
    [SerializeField] AudioClip shootAnticSFX;
    [SerializeField] AudioClip jumpupSFX;
    [SerializeField] AudioClip fallDownSFX;
    [SerializeField] AudioClip dieSFX;

    [Header("Debug")]
    [SerializeField] GameObject dot;
    float debugAlpha;

    #region Init/Shoot Cooldown
    public override void InitEnemy(EnemyData _enemyToSpawn)
    {
        this.Stats = new EnemyStats(_enemyToSpawn.stats);
        spawner = FindObjectOfType<Spawner>(); // 입에서 enemy를 발사하기 위해서
        generateWalls = GetComponent<GenerateWalls>();
        col = GetComponent<CapsuleCollider2D>();
        spriteRen = GetComponentInChildren<SpriteRenderer>();

        Name = _enemyToSpawn.name;

        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;
        InitHpBar();
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

    public void SlimeDropTimer()
    {
        if (slimeDropTimer < timeToDropSlime)
        {
            slimeDropTimer += Time.deltaTime;
            return;
        }
        DropSlime();
        slimeDropTimer = 0f;

    }
    #endregion
    void DropSlime()
    {
        // 이동 할 때 슬라임 점액
        if (slimeDropManager == null) slimeDropManager = GetComponent<SlimeDropManager>();
        slimeDropManager.DropSlimeDrop(ShootPoint.position);
    }
    void DropSlimeOnLanding()
    {
        // 착지 시 슬라임 점액
        if (slimeDropManager == null) slimeDropManager = GetComponent<SlimeDropManager>();
        slimeDropManager.DropSlimeDropOnLanding(ShootPoint.position);
    }

    #region 닿으면 Player HP 감소
    protected override void AttackMelee(int _damage)
    {
        Target.gameObject.GetComponent<Character>().TakeDamage(Stats.damage, EnemyType.Melee);
    }
    protected override void AttackRange(int _damage)
    {
        if (enemyProjectile == null) return;
        GameObject projectile = GameManager.instance.poolManager.GetMisc(enemyProjectile);
        if (projectile == null) return; // pooling key에서 개수 제한에 걸려서 더 이상 생성되지 않았다면
        projectile.transform.position = transform.position;

        // enemyProjectile의 damage값을 _damage 값으로 초기화 시키기
        EnemyProjectile proj = projectile.GetComponent<EnemyProjectile>();
        if (proj == null) return;
        proj.Init(_damage, Vector3.zero);
    }
    protected override void AttackExplode(int _damage)
    {
        AttackMelee(_damage);
        Die();
    }
    #endregion

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
        // StartCoroutine(ShootProjectileCo());
        StartCoroutine(ShootSlimeCo());
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

    IEnumerator ShootSlimeCo()
    {
        for (int i = 0; i < numberOfProjectile; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, projectiles.Length);
            Vector2 offsetPos = Target.position + new Vector2(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-4f, 4f));
            GameObject slimeDrop = Instantiate(slimeProjectilePrefab);
            slimeDrop.GetComponent<SlimeDropProjectile>().InitProjectile(transform.position, offsetPos, slimeProjectileSpeed);
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
        hpBar.SetStatus(Stats.hp, maxHealth);
    }
    public override void Die()
    {
        Debug.Log("Enemy Boss");
        base.Die();

        //GetComponent<DropOnDestroy>().CheckDrop();

        SoundManager.instance.Play(dieSFX);
        anim.SetTrigger("Die");

        BossDieManager.instance.InitDeadBody(deadBody, transform, 25);
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
    public void EnbleHPbar()
    {
        HPbar.SetActive(true);
    }
    public void DisableHPbar()
    {
        HPbar.SetActive(false);
    }
    public void CamShake()
    {
        CameraShake.instance.Shake();
    }
    public void StartLanding()
    {
        anim.SetTrigger("Land");
    }
    #endregion

    #region State Functions
    public void LandingImpact()
    {
        SoundManager.instance.Play(landingSFX);
        Vector2 landingEffectPos = (Vector2)transform.position + new Vector2(0, 3f);

        if (shockwave != null)
        {
            GameObject wave = GameManager.instance.poolManager.GetMisc(shockwave);
            wave.GetComponent<Shockwave>().Init(1200, 10f, LayerMask.GetMask("Player"), landingEffectPos);

            DropSlimeOnLanding();
        }
    }
    public void ActivateLandingIndicator(bool _activate)
    {
        if (LandingIndicator == null)
        {
            LandingIndicator = Instantiate(LandingIndicatorPrefab, transform);
            LandingIndicator.transform.localPosition = Vector2.zero;
            LandingIndicator.transform.localScale = .8f * Vector2.one;
        }
        LandingIndicator.SetActive(_activate);
    }
    public void GenTeleportEffect()
    {
        GameObject teleEffect = Instantiate(teleEffectPrefab, transform.position, Quaternion.identity);
    }

    bool IsOutOfRange()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();

        return new Equation().IsOutOfRange(transform.position, spawnConst);
    }

    public void RePosition()
    {
        if (IsOutOfRange())
        {
            Debug.Log(gameObject.name + " is out of range");
            transform.position = currentPosition;
            return;
        }
        Debug.Log("Yes, I am inside the wall");
        currentPosition = transform.position;
    }
    #endregion
}
