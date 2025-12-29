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

    // public EnemyStats(EnemyStats stats)
    // {
    //     this.hp = stats.hp;
    //     this.speed = stats.speed;
    //     this.damage = stats.damage;
    //     this.rangedDamage = stats.rangedDamage;
    //     this.speed = stats.speed;
    //     this.experience_reward = stats.experience_reward;
    // }
}

public class Enemy : EnemyBase
{
    #region Variables
    public int ExperienceReward { get; private set; }
    bool isLive;
    Vector2 currentPosition;

    // ⭐ 원거리 공격 관련 변수 제거됨 (컴포넌트로 이동)
    // float attackInterval;
    // float nextAttackTime;
    // float distanceToPlayer;

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

    // ⭐ 원거리 공격 컴포넌트 참조 추가
    EnemyRangedAttack rangedAttack;
    #endregion

    #region 유니티 콜백 함수
    protected override void OnEnable()
    {
        base.OnEnable();
        isLive = true;
        SetWalking();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;

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

        // ⭐ 원거리 공격 쿨다운 제거 (컴포넌트가 처리)
        // if (enemyType == EnemyType.Ranged) RangedAttackCoolDown();
    }
    #endregion

    #region 초기화
    public override void InitEnemy(EnemyData _data)
    {
        base.InitEnemy(_data);
        
        anim.runtimeAnimatorController = _data.animController;
        
        ExperienceReward = this.Stats.experience_reward;
        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;

        // ⭐ 원거리 공격 초기화 제거 (컴포넌트로 이동)
        // attackInterval = _data.attackInterval;
        // attackInterval += UnityEngine.Random.Range(0, 6f);
        // distanceToPlayer = _data.distanceToPlayer;

        InitHpBar();

        enemyType = _data.enemyType;
        enemyProjectile = _data.projectilePrefab;

        Name = _data.Name;

        canJump = _data.isJumper;
        if (shadowHeightEnemy == null)
            shadowHeightEnemy = GetComponent<ShadowHeightEnemy>();
        shadowHeightEnemy.SetIsJumper(canJump, _data.jumpInterval);

        // 모든 특수 능력 컴포넌트 초기화
        InitSpecialAbilities(_data);
    }

    // 새로 추가: 특수 능력 컴포넌트 관리
    void InitSpecialAbilities(EnemyData _data)
    {
        // 원거리 공격
        if (rangedAttack == null)
            rangedAttack = GetComponent<EnemyRangedAttack>();

        if (rangedAttack != null)
        {
            rangedAttack.InitRangedAttack(_data);
        }

        // 대시 능력 추가
        EnemyDashAbility dashAbility = GetComponent<EnemyDashAbility>();
        if (dashAbility != null)
        {
            dashAbility.InitDash(_data);
        }

        // 앞으로 추가될 다른 능력들도 여기서 초기화
        // EnemyDashAbility dashAbility = GetComponent<EnemyDashAbility>();
        // if (dashAbility != null)
        // {
        //     dashAbility.InitDash(_data);
        // }
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

    #region 공격
    protected override void AttackMelee(int _damage)
    {
        Target.gameObject.GetComponent<Character>().TakeDamage(Stats.damage, EnemyType.Melee);
    }

    protected override void AttackRange(int _damage)
    {
        // ⭐ 이 함수는 이제 EnemyBase의 OnCollisionStay에서만 호출됨
        // 원거리 쿨다운 공격은 EnemyRangedAttack 컴포넌트가 처리
        if (enemyProjectile == null) return;
        GameObject cannonBall = Instantiate(enemyProjectile, transform.position, Quaternion.identity);
        cannonBall.GetComponentInChildren<IEnemyProjectile>().InitProjectileDamage(Stats.rangedDamage);
        anim.SetBool("Attack", true);
    }

    protected override void AttackExplode(int _damage)
    {
        AttackMelee(_damage);
        Die();
    }

    // ⭐ RangedAttackCoolDown 함수 제거 (컴포넌트로 이동)
    #endregion

    // ⭐ DetectingPlayer 함수 제거 (컴포넌트로 이동)

    #region 애니메이션 이벤트
    public void SetAttackAnimDone()
    {
        anim.SetBool("Attack", false);
    }
    #endregion

    #region Shadow Height에서 사용하기 위한 함수들
    public void ActivateCollider(bool activateCol)
    {
        colEnemy.enabled = activateCol;
    }

    public Vector2 GetNextVec()
    {
        Vector2 dirVec = Target.position - (Vector2)rb.transform.position;
        Vector2 nextVec = currentSpeed * Time.fixedDeltaTime * dirVec.normalized;
        return nextVec;
    }
    #endregion
}