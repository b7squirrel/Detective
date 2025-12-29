using UnityEngine;

/// <summary>
/// Boss Moss 캐릭터만을 위한 클래스. 점진적으로 사용하지 않고 EnemyBoss를 이용해서 보스를 계속 구현하도록 하자
/// </summary>
public class BossBase : EnemyBase, Idamageable
{
    [Header("Boss Properties")]
    [SerializeField] float timeToChangeState;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject deadBody;
    [SerializeField] int numberOfStates; // 보스가 가진 상태 수

    Spawner spawner;
    float timer; // shoot coolTime counter

    [field: SerializeField] public float moveSpeedInAir { get; private set; }
    [field: SerializeField] public bool IsInAir { get; set; }

    [Header("Shoot")]
    [SerializeField] EnemyData[] projectiles;
    [SerializeField] AudioClip[] projectileSFX;
    [SerializeField] int numberOfProjectile;
    [SerializeField] int maxProjectile;
    [SerializeField] float timeToAttack;
    [SerializeField] protected Transform ShootPoint;
    [SerializeField] protected Transform dustPoint;
    [SerializeField] protected GameObject dustEffect;
    [SerializeField] protected GameObject teleportEffectPrefab;

    public Coroutine shootCoroutine;
    protected GameObject dust;
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

    protected override void Update()
    {
        base.Update();
        ChangeStateTImer();
    }

    public override void InitEnemy(EnemyData _enemyToSpawn)
    {
        // base에서 Stats 계산
        base.InitEnemy(_enemyToSpawn);
        
        spawner = FindObjectOfType<Spawner>(); // 죽었을 때 enemy를 삭제하기 위해서
        col = GetComponent<CapsuleCollider2D>();
        Name = _enemyToSpawn.name;
        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;
        InitHpBar();
    }

    #region 발사 스킬
    public virtual void ShootMultiProjectiles()
    {
    }
    #endregion

    #region 상태 변경
    public void ChangeStateTImer()
    {
        if (timer < timeToChangeState)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0f;
        int stateIndex = UnityEngine.Random.Range(0, numberOfStates);
        anim.SetTrigger(stateIndex.ToString());
    }
    #endregion

    #region TakeDamage Functions
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

    #region 애니메이션 이벤트
    public float GetDefaultSpeed()
    {
        return DefaultSpeed;
    }
    #endregion
}