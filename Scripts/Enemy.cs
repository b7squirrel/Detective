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
    Vector2 currentPosition; // 현재 위치를 기록해서 박으로 튀어나갔을 때, 위치를 되돌리기 위해서

    // 범위 공격 관련 변수
    float attackInterval;
    float nextAttackTime;
    float distanceToPlayer;

    [Header("Flying Enemies")]
    [SerializeField] float flyingSpeed;
    public bool IsFlying { get; set; }
    public Vector2 LandingTarget { get; set; }
    float flyingTimeThreshold = 4f;
    float flyingTimeCounter;

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

        if (isSubBoss || isBoss) // 보스나 서브보스는 매프레임마다 벽 안쪽에 있는지 체크
        {
            if (IsOutOfRange())
            {
                Debug.Log(gameObject.name + " is out of range");
                transform.position = currentPosition;
                return;
            }
            currentPosition = transform.position;
        }
        else // 일반 적이라면 10초마다 한번씩 체크해서 벽 바깥이면 비활성화
        {
            if (Time.time > nextOutOfRangeCheckingTime)
            {
                nextOutOfRangeCheckingTime += 10f;
                return;
            }

            if (IsOutOfRange())
            {
                Debug.Log("Out of Area");
                Die();
            }
        }

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
        Debug.Log("Melee Damage = " + Stats.damage);
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
}
