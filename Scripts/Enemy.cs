using System;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    public int hp = 999;
    public float speed = 5;
    public int damage = 1;
    public int rangedDamage = 1;
    public int experience_reward = 0;

    public EnemyStats(EnemyStats stats)
    {
        this.hp = stats.hp;
        this.speed = stats.speed;
        this.damage = stats.damage;
        this.rangedDamage = stats.rangedDamage;
        this.speed = stats.speed;
        this.experience_reward = stats.experience_reward;
    }
}

public class Enemy : EnemyBase
{
    #region Variables
    public int ExperienceReward { get; private set; }
    bool isLive;
    Vector2 currentPosition; // to store current position for reverting if enemies move outside walls

    // 범위 공격 관련 변수
    float attackInterval;
    float nextAttackTime;
    float distanceToPlayer;

    // 점프
    bool canJump;

    [Header("Flying Enemies")]
    [SerializeField] float flyingSpeed;
    public bool IsFlying { get; set; }
    public Vector2 LandingTarget { get; set; }
    float flyingTimeThreshold = 4f;
    float flyingTimeCounter;
    ShadowHeightEnemy shadowHeightEnemy;

    [SerializeField] LayerMask playerLayer;

    WallManager wallManager;
    float nextOutOfRangeCheckingTime;
    #endregion

    #region 유니티 콜백 함수
    protected override void OnEnable()
    {
        base.OnEnable();
        isLive = true;
        SetWalking(); // 날으는 상태로 소환되지 않도록
    }
    void FixedUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;

        // col.enabled = sr.isVisible;

        ApplyMovement();
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;

        Flip();

        // 벽에 끼거나 해서 walking으로 돌아오지 못하면 빠져나오도록
        if (flyingTimeCounter > 0 && IsFlying)
        {
            flyingTimeCounter -= Time.deltaTime;
        }
        else if(flyingTimeCounter < 0 && IsFlying)
        {
            SetWalking();
        }

        // 범위 공격
        if (enemyType == EnemyType.Ranged) AttackCoolDown();
    }
    #endregion

    #region 초기화
    public override void InitEnemy(EnemyData _data)
    {
        base.InitEnemy(_data);
        anim.runtimeAnimatorController = _data.animController;
        this.Stats = new EnemyStats(_data.stats);
        ExperienceReward = this.Stats.experience_reward;

        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;

        //범위 공격 변수 초기화
        attackInterval = _data.attackInterval;
        distanceToPlayer = _data.distanceToPlayer;

        InitHpBar();

        enemyType = _data.enemyType;
        enemyProjectile = _data.projectilePrefab;
        dieEffectPrefeab = _data.dieEffectPrefab; // 자폭 죽음과 일반 죽음을 구별하기 위해서 

        Name = _data.Name;

        canJump = UnityEngine.Random.Range(0, 1f) <= _data.jumpRate? true : false; // 점프 여부
        if(shadowHeightEnemy == null) shadowHeightEnemy = GetComponent<ShadowHeightEnemy>();
        shadowHeightEnemy.SetIsJumper(canJump, _data.jumpInterval);
    }
    public void SetFlying(Vector2 target)
    {
        IsFlying = true;
        LandingTarget = target;
        gameObject.layer = LayerMask.NameToLayer("InAir");
        sr.sortingLayerName = "InAir";
        sr.sortingOrder = 100;

        flyingTimeCounter = flyingTimeThreshold;
    }
    void SetWalking()
    {
        IsFlying = false;
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        sr.sortingLayerName = "Enemy";
    }
    #endregion

    public override void ApplyMovement()
    {
        if (IsFlying)
        {
            transform.position = Vector2.MoveTowards(transform.position, LandingTarget, flyingSpeed * Time.deltaTime);
            if (Vector2.Distance((Vector2)transform.position, LandingTarget) < 1f)
            {
                SetWalking();
            }
            return;
        }
        base.ApplyMovement();
    }

    bool IsOutOfRange()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();

        return new Equation().IsOutOfRange(transform.position, spawnConst);
    }

    protected override void AttackMelee(int _damage)
    {
        Target.gameObject.GetComponent<Character>().TakeDamage(Stats.damage, EnemyType.Melee);
    }
    protected override void AttackRange(int _damage)
    {
        if (enemyProjectile == null) return;
        GameObject projectile = GameManager.instance.poolManager.GetMisc(enemyProjectile);
        if(projectile == null) return; // pooling key에서 개수 제한에 걸려서 더 이상 생성되지 않았다면
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
    void AttackCoolDown()
    {
        if (Time.time >= nextAttackTime)
        {
            if (DetectingPlayer())
            {
                Attack(EnemyType.Ranged);
            }
            nextAttackTime = Time.time + attackInterval;
        }
    }
    bool DetectingPlayer()
    {
        float squDist = (Target.position - (Vector2)transform.position).sqrMagnitude;
        if (squDist < MathF.Pow(distanceToPlayer,2f))
            return true;
        return false;
    }


    #region Shadow Height에서 사용하기 위한 함수들
    public void ActivateCollider(bool activateCol)
    {
        colEnemy.enabled= activateCol;
    }

    // 점프를 위해 매프레임마다 수평 이동 속도가 필요하다
    public Vector2 GetNextVec()
    {
        Vector2 dirVec = Target.position - (Vector2)rb.transform.position;
        Vector2 nextVec = currentSpeed * Time.fixedDeltaTime * dirVec.normalized;
        return nextVec;
    }
    #endregion
}
