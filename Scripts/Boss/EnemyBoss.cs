using System.Collections;
using UnityEngine;

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
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] int halfWallBouncerNumber;
    GenerateWalls generateWalls;
    float timer; // shoot coolTime counter

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
        hpBar.SetStatus(Stats.hp, maxHealth);
    }
    public override void Die()
    {
        base.Die();

        //GetComponent<DropOnDestroy>().CheckDrop();

        SoundManager.instance.Play(dieSFX);
        anim.SetTrigger("Die");

        

        BossDieManager.instance.InitDeadBody(deadBody, transform, 25);
        BossDieManager.instance.DieEvent(.1f, 2f);
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
    //public GameObject GenLandingIndicator()
    //{
    //    GameObject landingIndicaotr = Instantiate(SpawnTeleportIndicator, transform.position, Quaternion.identity);
    //    return landingIndicaotr;
    //}
    //void OnDrawGizmos()
    //{
    //    // Gizmos.color를 통해 원의 색상을 설정합니다.
    //    Gizmos.color = Color.red;
    //    // Gizmos.DrawWireSphere를 사용하여 원의 외곽선을 그립니다.
    //    Gizmos.DrawWireSphere(transform.position, 15f / 2f);
    //}
    public void LandingImpact()
    {
        //GameObject effect = Instantiate(landingEffect, transform.position, Quaternion.identity);
        SoundManager.instance.Play(landingSFX);
        //effect.transform.position = transform.position;
        Vector2 landingEffectPos = (Vector2)transform.position + new Vector2(0, 3f);
        //Collider2D[] hits = Physics2D.OverlapCircleAll(landingEffectPos, landingImpactSize, landingHit);
        Collider2D hit = Physics2D.OverlapCircle(landingEffectPos, 15f/2f, landingHit);
        //GameObject debugGo = Instantiate(dot, landingEffectPos, Quaternion.identity);
        //debugGo.transform.localScale = 15f * Vector2.one;

        if (hit != null)
        {
            Character character = GameManager.instance.character;
            GameManager.instance.character.TakeDamage(1200, EnemyType.Ranged); // 플레이어가 무조건 데미지를 입도록 임시로 ranged
        }
        //for (int i = 0; i < hits.Length; i++)
        //{
        //    EnemyBase hit = hits[i].GetComponent<EnemyBase>();
        //    Character character = hits[i].GetComponent<Character>();

        //    if (hit != null && !hit.IsBoss) // 보스 랜딩 공격에 자신까지 포함시키지 않기
        //    {
        //        if (hits[i].CompareTag("Enemy"))
        //        {
        //            Debug.Log("Enemy on boss landing " + hits[i].name);
        //            hit.Stunned(transform.position);
        //        }
        //        if (hits[i].CompareTag("Player")) 
        //        {
        //            Debug.Log("Hit Player with Landing Attack");
        //            character.TakeDamage(Stats.damage);
        //        }
        //    }
        //}
    }
    public void ActivateLandingIndicator(bool _activate)
    {
        if (LandingIndicator == null)
        {
            LandingIndicator = Instantiate(LandingIndicatorPrefab, transform);
            LandingIndicator.transform.localPosition = Vector2.zero;
            LandingIndicator.transform.localScale = .8f * Vector2.one;
        }
        Debug.Log("Indicator");
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
