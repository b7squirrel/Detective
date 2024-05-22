using System;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    public int hp = 999;
    public float speed = 5;
    public int damage = 1;
    public int experience_reward = 0;

    public EnemyStats(EnemyStats stats)
    {
        this.hp = stats.hp;
        this.speed = stats.speed;
        this.damage = stats.damage;
        this.speed = stats.speed;
        this.experience_reward = stats.experience_reward;
    }
}

public class Enemy : EnemyBase
{
    public int ExperienceReward { get; private set; }
    bool isLive;
    Vector2 currentPosition; // 현재 위치를 기록해서 박으로 튀어나갔을 때, 위치를 되돌리기 위해서
    
    public bool IsFlying { get; set; }
    public Vector2 LandingTarget { get; set; }
    [SerializeField] float flyingSpeed;
    float flyingTimeThreshold = 4f;
    float flyingTimeCounter;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] bool isDetectingPlayer;

    WallManager wallManager;
    float nextOutOfRangeCheckingTime;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        isLive = true;
        SetWalking(); // 날으는 상태로 소환되지 않도록
        isDetectingPlayer = false;

    }
    void FixedUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;

        if (isSubBoss || isBoss) // 보스나 서브보스는 매프레임마다 벽 안쪽에 있는지 체크
        {
            if(IsOutOfRange())
            {
                transform.position = currentPosition;
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

            if(IsOutOfRange())
            {
                Debug.Log("Out of Area");
                Die();
            }
        }

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

        isDetectingPlayer = false;

        // 벽에 끼거나 해서 walking으로 돌아오지 못하면 빠져나오도록
        if (flyingTimeCounter > 0 && IsFlying)
        {
            flyingTimeCounter -= Time.deltaTime;
        }
        else if(flyingTimeCounter < 0 && IsFlying)
        {
            SetWalking();
        }
    }
    

    public void Init(EnemyData data)
    {
        anim.runtimeAnimatorController = data.animController;
        this.Stats = new EnemyStats(data.stats);
        ExperienceReward = this.Stats.experience_reward;

        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;

        InitHpBar();
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
        // 벽 안쪽에서 2 unit 더 안쪽에 스폰
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnArea = wallManager.GetSpawnAreaConstant();

        if (transform.position.x > spawnArea || transform.position.x < -spawnArea
            || transform.position.y > spawnArea || transform.position.y < -spawnArea)
        {
            return true;
        }

        return false;
    }
}
