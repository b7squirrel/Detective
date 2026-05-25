using System;
using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, Idamageable
{
    #region Variables
    // 체력바에 표시할 이름이 필요한 적들만 사용. 일반 적들은 배열 순서로 스폰.
    // enemy, enemyBoss 에서 InitEnemy로 이름을 enemyData에서 받아옴
    [field: SerializeField] public string Name { get; set; }
    [HideInInspector] public bool IsKnockBack { get; set; }
    [HideInInspector] public bool IsStunned { get; set; }
    [HideInInspector] public Rigidbody2D Target { get; set; }
    public EnemyStats Stats { get; set; }
    protected EnemyStatCalculator calculator; // 스탯 계산을 위해
    protected int currentStageNumber; // 스테이지에 따른 스탯 계산을 위해
    public float DefaultSpeed { get; protected set; } // 속도를 외부에서 변경할 때 원래 속도를 저장해 두기 위해
    protected float currentSpeed;
    public bool IsSlowed { get; set; } // 슬로우 스킬을 
    public bool IsBoss { get; set; }
    [SerializeField] protected bool isSubBoss;
    [SerializeField] protected bool isBoss;
    [SerializeField] int numberOfSubBossDrops;
    [SerializeField] int numberOfBossDrops;
    protected BossType bossType;
    public bool IsGrouping { get; set; } // 그룹지어 다니는 적인지 여부
    public Vector2 GroupDir { get; set; } // spawn 할 때 spawn 포인트 값과 player위치로 결정

    protected EnemyVariantHandler variantHandler; // 광기, 헬멧, 광기헬멧, 폭탄의 다양화 핸들러

    protected EnemyType enemyType; // Melee, Explode 공격만 EnemyBase에서 정의하고 Ranged는 Enemy에서 정의
    protected bool isSplitable; // 처치하면 쪼개지는 적인지
    protected EnemyData splitableEnemyData; // 쪼개질 때의 적 데이터
    protected int splitNum; // 몇 개로 쪼개질지
    protected bool isSplited; // 쪼개진 적이라면 킬 카운트 하지 않기
    DropOnDestroy dropOnDestroy; // Die()에서 매번 GetComponent 하지 않도록 캐싱

    protected bool isOffScreen; // 화면 밖에 있을 때 플레이어의 공격을 받지 않기 위한 플래그
    protected float offScreenCoolDown; // 너무 자주 콜라이더가 활성, 비활성 되지 않도록 쿨타임 주기
    [SerializeField] LayerMask screenEdge;

    Coroutine flipCoroutine;
    bool isFlipping; // 더 이상 flip하고 있지 않으면 코루틴을 초기화 시키기위해
    float pastFacingDir, currentFacingDir;

    bool initDone;
    protected bool finishedSpawn; // 스폰이 끝나면 적이 이동하도록 하려고

    Vector2 pastPos; // 벽 바깥으로 나가면 다시 되돌리기 위한 변수

    bool isGrouned; // 점프 중이라면 플립을 하지 않도록 하기 위해
    protected ShadowHeightEnemy shadowHeightEnemy; // CastSlownessToEnemy 등에서 매번 GetComponent 하지 않도록 캐싱

    int goldReward; // 처치 시 지급할 골드

    // static (모든 적이 공유)
    static InfiniteStageManager infiniteStageManager;
    static bool isInfiniteMode = false;
    static bool hasCheckedMode = false;
    public event Action OnDeath; // 다른 클래스에서 죽을 때 실행해야하는 함수들 등록
    #endregion

    #region Component Variables
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Collider2D colEnemy;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    [SerializeField] protected GameObject dieEffectPrefeab;
    [SerializeField] protected GameObject dieExplosionPrefeab;
    [SerializeField] protected GameObject knockbackEffect;
    [SerializeField] protected Transform hitEffectPoint;
    [SerializeField] protected float knockBackSpeed;
    [SerializeField] protected float knockBackDelay;
    [SerializeField] protected SpriteRenderer[] srFlash; // 깜빡이기 위한 스프라이트 렌더러들. 이 부분들의 색깔을 바꿔서 히트를 보여줌
    protected GameObject enemyProjectile;
    protected float enemyKnockBackSpeedFactor; // TakeDamage 때마다 인자로 넘어오는 knockBackSpeedFactor를 담아 두는 용도
    protected float stunnedDuration = .2f;

    protected Material[] initialMat;
    LayerMask wallLayer;       // IsInsideWall에서 매번 GetMask("Wall") 하지 않도록 캐싱
    LayerMask shockwaveEnemyLayer; // Die()의 shockwave에서 매번 GetMask("Enemy") 하지 않도록 캐싱

    [HideInInspector] public Vector2 targetDir;
    protected float stunnedSpeed = 14f;

    // 공격 프레임 간격 (모드별로 다르게 설정)
    protected int attackFrameInterval = 5; // 기본값 3프레임

    [Header("특수 능력")]
    protected EnemyDashAbility dashAbility;
    protected EnemyRangedAttack rangedAttack;
    protected EnemyLaserAbility laserAbility;

    [Header("Sounds")]
    [SerializeField] protected AudioClip[] hits;
    [SerializeField] protected AudioClip[] dies;
    [SerializeField] protected AudioClip subBossAlarm;
    protected AudioClip hitSound;
    protected AudioClip dieSound;

    [Header("HP Bar")]
    [SerializeField] GameObject HPbarPrefab;
    protected GameObject HPbar;
    [SerializeField] Transform HpBarPos;
    [SerializeField] protected StatusBar hpBar;
    protected int maxHealth;

    [Header("Shock Wave")]
    [SerializeField] protected GameObject shockwave;

    static BossDieManager bossDieManager;

    #endregion

    #region 유니티 콜백
    protected virtual void OnEnable()
    {
        if (infiniteStageManager == null)
        {
            infiniteStageManager = FindObjectOfType<InfiniteStageManager>();
            isInfiniteMode = (infiniteStageManager != null);

            if (isInfiniteMode)
            {
                Logger.Log("[EnemyBase] Infinite mode detected");
            }
        }

        if (initDone == false)
        {
            Target = GameManager.instance.player.GetComponent<Rigidbody2D>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sr = GetComponentInChildren<SpriteRenderer>();
            colEnemy = GetComponent<Collider2D>();

            dashAbility = GetComponent<EnemyDashAbility>();
            rangedAttack = GetComponent<EnemyRangedAttack>();
            laserAbility = GetComponent<EnemyLaserAbility>();

            shadowHeightEnemy = GetComponent<ShadowHeightEnemy>();
            dropOnDestroy = GetComponent<DropOnDestroy>();

            pastPos = transform.position;

            initDone = true;
        }

        // 머티리얼 초기화 (오브젝트 풀에서 재사용될 때 필요)
        if (initialMat != null && srFlash != null)
        {
            for (int i = 0; i < initialMat.Length && i < srFlash.Length; i++)
            {
                srFlash[i].material = initialMat[i];
            }
        }

        IsKnockBack = false;
        IsStunned = false;
        isOffScreen = true;

        transform.eulerAngles = Vector3.zero;
        StopFlipCoroutine();
        isFlipping = false;
        isGrouned = true;

        anim.speed = 1f;

        if (Target.position.x - rb.transform.position.x > 0)
        {
            currentFacingDir = 1f;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            currentFacingDir = -1f;
            transform.eulerAngles = new Vector3(0, 180f, 0);
        }

        pastFacingDir = currentFacingDir;

        // 시간 정지 상태 체크. 시간 정지 이후 생성되는 적들에게도 시간 정지 적용
        CheckStopwatchStatus();

        // LayerMask는 한 번만 계산
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Wall");
        if (shockwaveEnemyLayer == 0) shockwaveEnemyLayer = LayerMask.GetMask("Enemy");
    }

    /// <summary>
    /// 적 생성 시 시간 정지 상태 확인하여 즉시 정지
    /// </summary>
    void CheckStopwatchStatus()
    {
        if (FieldItemEffect.instance != null && FieldItemEffect.instance.IsStopedWithStopwatch())
        {
            PauseEnemy();
        }
    }

    protected void InitHpBar()
    {
        if (HPbarPrefab == null) return;
        if (HPbar == null) HPbar = Instantiate(HPbarPrefab, HpBarPos.position, Quaternion.identity);
        HPbar.transform.localScale = HpBarPos.localScale;
        if (hpBar == null) hpBar = HPbar.GetComponentInChildren<StatusBar>();

        maxHealth = Stats.hp;
        hpBar.SetStatus(Stats.hp, maxHealth);
    }

    protected void DestroyHPbar()
    {
        Destroy(HPbar);
    }

    protected virtual void Update()
    {
        if (HPbar != null) HPbar.transform.position = HpBarPos.position;

        if (isSubBoss || isBoss) // 보스나 서브보스는 매프레임마다 벽 안쪽에 있는지 체크
        {
            IsInsideWall();
        }
        else // 일반 적이라면 10초마다 한번씩 체크해서 벽 바깥이면 비활성화
        {
            if (Time.frameCount % 600 != 0) return;

            IsInsideWall();
        }
    }
    #endregion

    #region 초기화
    public virtual void InitEnemy(EnemyData _enemyToSpawn)
    {
        bossType = _enemyToSpawn.bossType;
        goldReward = _enemyToSpawn.goldReward;
        isSubBoss = false;
        isBoss = false;
        if (bossType == BossType.StageBoss || bossType == BossType.QueenBoss)
        {
            isSubBoss = false;
            isBoss = true;
        }
        else if (bossType == BossType.SubBoss)
        {
            isSubBoss = true;
            isBoss = false;
        }

        // 상태 초기화
        IsSlowed = false;

        // 공격 프레임 간격 설정 (모드별 분기)
        if (isInfiniteMode && infiniteStageManager != null)
        {
            attackFrameInterval = 4; // 무한 모드: 4프레임에 한 번
        }
        else
        {
            attackFrameInterval = 3; // 일반 모드: 3프레임에 한 번
        }

        // 무한 모드와 레귤러 모드 분기
        if (isInfiniteMode && infiniteStageManager != null)
        {
            // 무한 모드: 웨이브를 스테이지로 사용 (최대 30)
            int waveNumber = infiniteStageManager.GetCurrentWave();
            currentStageNumber = Mathf.Min(Mathf.RoundToInt(Mathf.Pow(waveNumber, 1.2f)), 30);
        }
        else
        {
            // 레귤러 모드: 현재 스테이지
            if (currentStageNumber == 0)
            {
                currentStageNumber = PlayerDataManager.Instance.GetCurrentStageNumber();
            }
        }

        if (calculator == null)
            calculator = GameManager.instance.enemyStatCalculator;

        if (calculator != null)
        {
            this.Stats = calculator.GetStatsForStage(currentStageNumber, _enemyToSpawn);
        }
        else
        {
            this.Stats = new EnemyStats
            {
                hp = 100,
                speed = 5,
                damage = 10,
                rangedDamage = 10,
                experience_reward = 50
            };
        }

        // 속도 설정
        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;

        // Variant 적용
        if (variantHandler == null)
            variantHandler = GetComponent<EnemyVariantHandler>();

        if (variantHandler != null)
        {
            int variantNumber = currentStageNumber;

            // 무한 모드에서는 변환된 스테이지 번호가 아닌 실제 웨이브 번호 사용
            if (isInfiniteMode && infiniteStageManager != null)
            {
                variantNumber = infiniteStageManager.GetCurrentWave();
            }

            EnemyVariantType variant = EnemyVariantHandler.GetVariantForStage(variantNumber);
            variantHandler.ApplyVariant(variant);
            ApplyVariantEffects(variant);
        }

        // 쪼개지는 적 관련 초기화
        if (_enemyToSpawn.split == null)
        {
            isSplitable = false;
            splitableEnemyData = null;
            splitNum = 0;
        }
        else
        {
            isSplitable = true;
            splitableEnemyData = _enemyToSpawn.split;
            splitNum = _enemyToSpawn.splitNum;
            finishedSpawn = true;
            dieEffectPrefeab = _enemyToSpawn.splitDieEffectPrefab;
        }

        isSplited = false; // 일단 false. 스포너의 spawnSplit에서 InitSplitedEnemy 호출해서 설정

        if (_enemyToSpawn.hitSound != null) hitSound = _enemyToSpawn.hitSound;
        if (_enemyToSpawn.dieSound != null) dieSound = _enemyToSpawn.dieSound;

        // 서브 보스 설정
        // 화이트 플래시를 한 후 원래 재질로 되돌리기 위한 initial mat 초기화
        // 서브 보스 알람
        if (isSubBoss)
        {
            if (srFlash == null) return;
            initialMat = new Material[srFlash.Length];
            for (int i = 0; i < srFlash.Length; i++)
            {
                initialMat[i] = srFlash[i].material;
            }

            if (subBossAlarm != null) SoundManager.instance.Play(subBossAlarm);
        }
    }

    #region Variant 효과
    protected void ApplyVariantEffects(EnemyVariantType variant)
    {
        // 초기화
        Stats.damageReduction = 0f;
        anim.speed = 1f;

        // config 참조
        EnemyScalingConfig config = GameManager.instance.enemyStatCalculator
            .GetScalingConfig();

        switch (variant)
        {
            case EnemyVariantType.Madness:
                attackFrameInterval = config.madnessAttackFrameInterval;
                anim.speed = config.madnessAnimSpeed;
                break;

            case EnemyVariantType.Helmet:
                Stats.damageReduction = config.helmetDamageReduction;
                break;

            case EnemyVariantType.MadnessHelmet:
                attackFrameInterval = config.madnessAttackFrameInterval;
                anim.speed = config.madnessAnimSpeed;
                Stats.damageReduction = config.helmetDamageReduction;
                break;

            case EnemyVariantType.Explosive:
                break;
        }
    }
    #endregion

    public void SetIsSplited(bool splited)
    {
        isSplited = splited;
    }
    #endregion

    #region Movement Functions
    public void SetFlipEnabled(int enable) // 애니메이션 이벤트로 사용
    {
        if (enable == 0)
        {
            // Flip 비활성화 - 진행 중인 플립 코루틴 중지
            StopFlipCoroutine();
            isFlipping = true; // 플립이 일어나지 않도록 막음
        }
        else
        {
            // Flip 활성화
            isFlipping = false; // 플립이 다시 일어날 수 있도록 허용
        }
    }

    public virtual void Flip()
    {
        if (gameObject.activeSelf == false) return;

        if (anim.speed == 0) return; // 스탑워치가 작동하고 있다면 플립되지 않음
        if (isFlipping == false)
        {
            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
        }

        if (Target.position.x - transform.position.x > 0)
        {
            currentFacingDir = 1f;
        }
        else
        {
            currentFacingDir = -1f;
        }

        if (currentFacingDir != pastFacingDir && isFlipping == false && isGrouned)
            flipCoroutine = StartCoroutine(FlipCo());

        if (IsGrouping)
        {
            if (GroupDir.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 180f, 0);
            }
        }

        pastFacingDir = currentFacingDir;
    }

    IEnumerator FlipCo()
    {
        isFlipping = true;
        int index = 0;
        while (index < 2) // 120도까지는 60도씩 2번 회전
        {
            yield return new WaitForSeconds(.03f); // 0.03초 간격으로
            transform.eulerAngles = transform.eulerAngles + (currentFacingDir * new Vector3(0, 60f, 0));
            index++;
            yield return null;
        }
        index = 0;
        while (index < 3) // 120도부터는 20도씩 3번 회전
        {
            yield return new WaitForSeconds(.03f);
            transform.eulerAngles = transform.eulerAngles + (currentFacingDir * new Vector3(0, 20f, 0));
            index++;
            yield return null;
        }

        // 뒤집고 나서는 방향을 플레이어쪽으로 강제 정렬하여
        // 뒤집히다 말거나 반대로 뒤집히는 현상을 방지
        if (Target.position.x - rb.position.x > 0)
        {
            currentFacingDir = 1f;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            currentFacingDir = -1f;
            transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        isFlipping = false;
    }

    void ResetFlip()
    {
        StopFlipCoroutine();
        if (Target.position.x - rb.transform.position.x > 0)
        {
            currentFacingDir = 1f;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            currentFacingDir = -1f;
            transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        isFlipping = false;
    }

    void StopFlipCoroutine()
    {
        if (flipCoroutine != null) StopCoroutine(flipCoroutine);
    }

    #region Apply Movement
    public virtual void ApplyMovement()
    {
        if (finishedSpawn == false) return;

        // 대시 중이면 일반 이동하지 않음
        if (dashAbility != null && dashAbility.IsDashing())
        {
            return; // 대시 컴포넌트가 움직임을 처리함
        }

        // 레이져 발사 중이면 이동하지 않음
        if (laserAbility != null && laserAbility.IsUsingLaser())
        {
            return;
        }

        if (IsKnockBack)
        {
            rb.velocity = knockBackSpeed * targetDir;
            float randomOffsetX = UnityEngine.Random.Range(-.5f, .5f);
            float randomOffsetY = UnityEngine.Random.Range(-.5f, .5f);
            PickupSpawner.Instance.SpawnPickup(transform.position + new Vector3(randomOffsetX, randomOffsetY, 0), knockbackEffect, false, 0);
            return;
        }
        if (IsStunned)
        {
            rb.velocity = stunnedSpeed * targetDir;
            return;
        }

        Vector2 dirVec = Target.position - (Vector2)rb.transform.position;
        if (IsGrouping)
        {
            rb.velocity = currentSpeed * GroupDir;
            return;
        }
        Vector2 nextVec = currentSpeed * EarthquakeManager.EnemySpeedMultiplier
    * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }
    #endregion

    void IsInsideWall()
    {
        RaycastHit2D hit = Physics2D.Linecast(pastPos, transform.position, wallLayer);
        if (hit.collider != null) // 벽 안으로 들어갔다면
        {
            transform.position = pastPos; // 이전 위치로 되돌림
        }
        pastPos = transform.position;
    }

    public void SetGrounded(bool _isGrouinded)
    {
        isGrouned = _isGrouinded;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    #region 애니메이션 이벤트
    // 스폰 애니메이션이 끝나는 지점에 이벤트
    public void TriggerFinishedSpawn()
    {
        finishedSpawn = true;

        // 원거리 공격 컴포넌트에 알림
        if (rangedAttack != null)
        {
            rangedAttack.SetFinishedSpawn(true);
        }

        // 대시 컴포넌트에 알림
        if (dashAbility != null)
        {
            dashAbility.SetFinishedSpawn(true);
        }

        // 레이저 컴포넌트에 알림
        if (laserAbility != null)
        {
            laserAbility.SetFinishedSpawn(true);
        }
    }
    #endregion
    #endregion

    #region 닿으면 player HP 감소
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (anim.speed == 0) // 스톱워치로 멈춘 상태라면 
            return;

        // ✅ 추가: 모든 적이 동일 프레임에 처리되지 않도록 분산
        if (Time.frameCount % attackFrameInterval != (GetInstanceID() & 3)) return;

        if (GameManager.instance.player == null)
            return;

        if (collision.gameObject == Target.gameObject)
        {
            if (enemyType == EnemyType.Melee && Time.frameCount % attackFrameInterval == 0) // Melee는 attackFrameInterval 프레임에 한 번 공격
            {
                Attack(EnemyType.Melee);
            }
            else if (enemyType == EnemyType.Ranged)
            {
                Attack(EnemyType.Melee);
            }
            else if (enemyType == EnemyType.Explode)
            {
                Attack(EnemyType.Explode);
            }
            rb.velocity = .5f * Vector2.one;
            rb.angularVelocity = 0f;
        }
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (GameManager.instance.player == null)
            return;

        if (anim.speed == 0) // 스톱워치로 멈춘 상태라면 
            return;

        if (collision.gameObject == Target.gameObject)
        {
            anim.SetBool("Attack", false);
        }
    }

    protected void Attack(EnemyType _enemyType)
    {
        if (Target.gameObject == null)
            return;

        if (anim.speed == 0) // 스톱워치로 멈춘 상태라면 
            return;

        anim.SetBool("Attack", true);

        switch (_enemyType)
        {
            case EnemyType.Melee:
                AttackMelee(Stats.damage);
                break;
            case EnemyType.Ranged:
                AttackRange(Stats.rangedDamage);
                break;
            case EnemyType.Explode:
                AttackExplode(Stats.damage);
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(enemyType), enemyType, "정의되지 않은 enemyType입니다");
        }
    }

    protected virtual void AttackMelee(int _damage)
    {
        // Enemy에서 오버라이드
    }
    protected virtual void AttackRange(int _damage)
    {
        // Enemy에서 오버라이드
    }
    protected virtual void AttackExplode(int _damage)
    {
        // Enemy에서 오버라이드
    }

    public StatusBar GetHpBar()
    {
        return hpBar;
    }
    #endregion

    #region Take Damage
    protected void CheckOffScreen()
    {
        isOffScreen = !(sr.isVisible);
    }

    public virtual void TakeDamage(int damage, float knockBackChance, float knockBackSpeedFactor, Vector2 target, GameObject hitEffect)
    {
        // 스테이지/웨이브 기반 회피 확률 적용
        float dodgeRoll = UnityEngine.Random.Range(0f, 1f);
        if (dodgeRoll < Stats.dodgeChance)
        {
            return;
        }

        // 화면 밖에 있으면 데미지 입지 않기
        CheckOffScreen();
        if (isOffScreen)
            return;

        // Hurt 애니메이션이 재생중이면 또 재생하지 않기
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("SlimeLV1Hurt"))
        {
            anim.SetTrigger("Hit");
            // 느림 상태라면 Hypnotized도 유지
            if (IsSlowed)
            {
                anim.SetBool("Hypnotized", true);
            }
        }

        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        if (effect != null) effect.transform.position = target;

        // 넉백
        enemyKnockBackSpeedFactor = knockBackSpeedFactor;

        float _knockBackDelay = 0f;
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance < knockBackChance && knockBackChance != 0)
            _knockBackDelay = this.knockBackDelay;

        // 헬멧 방어율 적용. 최소 1 데미지 보장
        int finalDamage = Mathf.RoundToInt(damage * (1f - Stats.damageReduction));
        finalDamage = Mathf.Max(1, finalDamage);
        Stats.hp -= finalDamage;

        if (Stats.hp < 1)
        {
            if (dieSound != null)
            {
                SoundManager.instance.PlaySoundWith(dieSound, 1f, true, .2f);
            }
            else
            {
                for (int i = 0; i < dies.Length; i++)
                {
                    SoundManager.instance.PlaySoundWith(dies[i], 1f, true, .2f);
                }
            }

            Die();
        }
        else
        {
            if (hitSound != null)
            {
                SoundManager.instance.PlaySoundWith(hitSound, 1f, true, .2f);
            }
            else
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    SoundManager.instance.PlaySoundWith(hits[i], 1f, true, .2f);
                }
            }

            if (hpBar != null)
            {
                hpBar.SetStatus(Stats.hp, maxHealth);
            }
        }

        KnockBack(target, _knockBackDelay, knockBackSpeedFactor);
    }

    public virtual void Die()
    {
        dropOnDestroy?.CheckDrop();

        StopAllCoroutines();

        IsGrouping = false;
        ResetFlip();

        GameManager.instance.KillManager.UpdateCurrentKills(enemyType, isSubBoss, isBoss);
        GoldRewardManager.Instance.AddKillGold(goldReward);

        // 쪼개진 적(슬라임 조각 등)은 킬 카운트 제외, 무한 모드와 일반 모드 분리
        if (!isSplited && AchievementManager.Instance != null)
        {
            if (PlayerDataManager.Instance.GetGameMode() == GameMode.Infinite)
                AchievementManager.Instance.AddProgressInfinite(AchievementType.KILL, 1);
            else
                AchievementManager.Instance.AddProgress(AchievementType.KILL, 1);
        }

        Spawner.instance.SubtractEnemyNumber();

        IsSlowed = false;
        finishedSpawn = false;
        DestroyHPbar();

        if (IsBoss && PlayerDataManager.Instance.GetGameMode() == GameMode.Regular)
        {
            BossDieManager.instance.SetIsBossDead(true);
            BossDieManager.instance.DieEvent(.1f, 2f);
        }
        else if (IsBoss && PlayerDataManager.Instance.GetGameMode() == GameMode.Infinite)
        {
            BossDieManager.instance.DieEventInfinite(.1f, 2f); // deadBody 연출만, IsBossDead 없음
        }

        if (isSubBoss)
        {
            CameraShake.instance.Shake();
        }

        if (shockwave != null)
        {
            GameObject wave = GameManager.instance.poolManager.GetMisc(shockwave);
            wave.GetComponent<Shockwave>().Init(0, 10f, shockwaveEnemyLayer, transform.position);

            BossDieManager.instance.SlowMo(.5f, .5f);
        }

        if (isSplitable)
        {
            for (int i = 0; i < splitNum; i++)
            {
                Spawner.instance.SpawnSplit(splitableEnemyData, 0, true, transform.position);
            }
            GameObject explosionEffect = GameManager.instance.poolManager.GetMisc(dieEffectPrefeab);
            if (explosionEffect != null) explosionEffect.transform.position = transform.position;
        }
        else
        {
            GameObject explosionEffect = GameManager.instance.feedbackManager.GetDieEffect();
            if (explosionEffect != null) explosionEffect.transform.position = transform.position;
        }

        // 무한 모드 킬 카운트. 쪼개진 조각 적은 제외
        if (isInfiniteMode && infiniteStageManager != null && isSplited == false)
        {
            infiniteStageManager.OnEnemyKilled();
        }

        OnDeath?.Invoke();

        gameObject.SetActive(false);
    }

    // 보스가 등장할 때 적들을 모두 제거할 때 사용
    // ✅ DieOnBossEvent
    public void DieOnBossEvent()
    {
        if (isBoss) return; // 서브 보스도 제거
        GameObject explosionEffect = GameManager.instance.feedbackManager.GetDieEffect();
        if (explosionEffect != null) explosionEffect.transform.position = transform.position;

        IsSlowed = false;
        finishedSpawn = false;
        Spawner.instance.SubtractEnemyNumber(); // ⭐ 추가
        gameObject.SetActive(false);
    }

    // ✅ Deactivate
    public virtual void Deactivate()
    {
        IsGrouping = false;
        IsSlowed = false;
        Spawner.instance.SubtractEnemyNumber(); // ⭐ 추가
        gameObject.SetActive(false);
    }

    // ✅ DieWithoutDrop
    public virtual void DieWithoutDrop()
    {
        IsSlowed = false;
        Spawner.instance.SubtractEnemyNumber(); // ⭐ 추가
        gameObject.SetActive(false);
    }

    protected virtual void KnockBack(Vector2 target, float knockBackDelay, float knockBackSpeedFactor)
    {
        if (anim.speed == 0) return;
        // knockbackDelay를 0으로 설정해 두었다면 넉백이 일어나지 않음
        if (knockBackDelay != 0)
        {
            IsKnockBack = true;
            targetDir = ((Vector2)transform.position - target).normalized;
        }

        if (this.gameObject.activeSelf)
        {
            StartCoroutine(KnockBackDone(knockBackDelay * knockBackSpeedFactor));
        }
    }

    IEnumerator KnockBackDone(float knockBackDelay)
    {
        yield return new WaitForSeconds(knockBackDelay);
        IsKnockBack = false;
    }

    public virtual void Stunned(Vector2 target)
    {
        IsStunned = true;
        targetDir = ((Vector2)transform.position - target).normalized;
        anim.SetTrigger("Hit");
        StartCoroutine(StunnedCo());
    }

    IEnumerator StunnedCo()
    {
        yield return new WaitForSeconds(stunnedDuration);
        IsStunned = false;
    }
    #endregion

    #region 스킬
    public void CastSlownessToEnemy(float _slownessFactor)
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록

        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;

        // 최대 속도 제한
        if (currentSpeed >= 15f) currentSpeed = 15f;

        // 최소 속도 제한 (너무 느려지지 않도록)
        if (currentSpeed < 1f) currentSpeed = 1f;

        anim.SetBool("Hypnotized", true);

        // 애니메이터 속도 감소 (느린 모션 효과)
        anim.speed = 1f - _slownessFactor;

        // 점프 느림 효과
        if (shadowHeightEnemy != null)
        {
            shadowHeightEnemy.ApplySlowToJump(_slownessFactor);
        }
    }

    // 점프 중 속도를 조정하기 위해 ShadowHeightEnemy에서 호출
    // 최면 애니메이션을 재생하지 않음
    public void SpeedUpOnJump(float _slownessFactor)
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록

        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;

        // 최대 속도 제한
        if (currentSpeed >= 15f) currentSpeed = 15f;

        // 최소 속도 제한
        if (currentSpeed < 1f) currentSpeed = 1f;
    }

    public void ResetCurrentSpeedToDefault()
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록
        currentSpeed = DefaultSpeed;

        anim.SetBool("Hypnotized", false);

        // 애니메이터 속도 복구
        anim.speed = 1f;

        // 점프 느림 효과 해제
        if (shadowHeightEnemy != null)
        {
            shadowHeightEnemy.ReleaseSlowFromJump();
        }
    }

    public void SpeedUpEnemy()
    {
        float speed = 2f;
        anim.speed = speed;
        CastSlownessToEnemy(-speed);
    }

    public void PauseEnemy()
    {
        anim.speed = 0f;
        currentSpeed = 0f; // sluggish slumber와 겹치지 않기 위해 CastSlowness 함수를 사용하지 않음
    }

    public void ResumeEnemy()
    {
        anim.speed = 1;
        currentSpeed = DefaultSpeed; // sluggish slumber와 겹치지 않기 위해 ResetCurrentSpeed 함수를 사용하지 않음
    }

    public bool isTimeStopped()
    {
        return currentSpeed == 0f;
    }

    // 보스들의 경우 너무 한 점에만 공격이 그래픽적으로 집중되지 않도록. Hit effect가 한 곳에만 생기지 않도록.
    public Vector2 GetRandomBodyPoint()
    {
        if (isSubBoss || isBoss)
        {
            return (Vector2)transform.position + new Vector2(
                UnityEngine.Random.Range(-0.8f, 0.8f),
                UnityEngine.Random.Range(1f, 3f));
        }

        // 일반 적도 작은 오프셋 적용
        return (Vector2)transform.position + new Vector2(
            UnityEngine.Random.Range(-0.2f, 0.2f),
            UnityEngine.Random.Range(.3f, .8f));
    }
    #endregion
}