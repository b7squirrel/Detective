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
    public Vector2 GroupDir { get; set; } // spawn 할 떄 spawn 포인트 값과 player위치로 결정

    protected EnemyType enemyType; // Melee, Explode 공격만 EnemyBase에서 정의하고 Ranged는 Enemy에서 정의
    protected bool isSplitable; // 처치하면 쪼개지는 적인지
    protected EnemyData splitableEnemyData; // 쪼개질 때의 적 데이터
    protected int splitNum; // 몇 개로 쪼개질지
    protected bool isSplited; // 쪼개진 적이라면 킬 카운트 하지 않기

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
    [SerializeField] protected float whiteFlashDuration = 0.08f;
    [SerializeField] protected float knockBackSpeed;
    [SerializeField] protected float knockBackDelay;
    [SerializeField] protected SpriteRenderer[] srFlash; // 깜빡이기 위한 스프라이트 렌더러들. 이 부분들의 색깔을 바꿔서 히트를 보여줌
    protected GameObject enemyProjectile;
    protected float enemyKnockBackSpeedFactor; // TakeDamage 때마다 인자로 넘어오는 knockBackSpeedFactor를 담아 두는 용도
    protected float stunnedDuration = .2f;

    protected Material[] initialMat;
    [SerializeField] protected Material whiteMat;

    [HideInInspector] public Vector2 targetDir;
    protected float stunnedSpeed = 14f;
    Coroutine whiteFlashCoroutine;
    Color enemyColor; // die effect의 색깔을 정하기 위해서.

    // 공격 프레임 간격 (모드별로 다르게 설정)
    protected int attackFrameInterval = 3; // 기본값 3프레임

    [Header("특수 능력")]
    protected EnemyDashAbility dashAbility;
    protected EnemyRangedAttack rangedAttack;
    protected EnemyLaserAbility laserAbility;

    [Header("Sounds")]
    [SerializeField] protected AudioClip[] hits;
    [SerializeField] protected AudioClip[] dies;
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

    EnemyFinder enemyFinder;
    FieldItemEffect fieldItemEffect;
    #endregion

    // 시각적 효과 (느림보 최면술)
    Color originalColor = Color.white;
    bool isColorChanged = false;

    #region 유니티 콜백
    protected virtual void OnEnable()
    {
        // ⭐ 수정: hasCheckedMode 제거하고 매번 null 체크
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
        whiteFlashCoroutine = null; // 코루틴 참조도 초기화

        //initialMat = sr.material;
        IsKnockBack = false;
        IsStunned = false;
        isOffScreen = true;

        transform.eulerAngles = Vector3.zero;
        StopFlipCoroutine();
        isFlipping = false;
        isGrouned = true;

        anim.speed = 1f;
        //rb.bodyType = RigidbodyType2D.Dynamic;

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

        if (Stats != null)
        {

        }

        // 시간 정지 상태 체크. 시간 정지 이후 생성되는 적들에게도 시간 정지 적용
        CheckStopwatchStatus();
    }
    /// <summary>
    /// 적 생성 시 시간 정지 상태 확인하여 즉시 정지
    /// </summary>
    void CheckStopwatchStatus()
    {
        if (fieldItemEffect == null) fieldItemEffect = FindObjectOfType<FieldItemEffect>();
        if (fieldItemEffect != null && fieldItemEffect.IsStopedWithStopwatch())
        {
            PauseEnemy();
            // Logger.Log($"[EnemyBase] {gameObject.name} - 생성 시 즉시 정지됨 (스톱워치 활성화 중)");
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

        enemyColor = _enemyToSpawn.enemyColor;

        // 상태 초기화
        ResetTintColor(); // ⭐ 색상 초기화 추가
        IsSlowed = false; // 느림 상태도 초기화

        // ⭐ 공격 프레임 간격 설정 (모드별 분기)
        if (isInfiniteMode && infiniteStageManager != null)
        {
            attackFrameInterval = 4; // 무한 모드: 4프레임에 한 번
        }
        else
        {
            attackFrameInterval = 3; // 일반 모드: 3프레임에 한 번
        }

        // ⭐ 무한 모드와 레귤러 모드 분기
        if (isInfiniteMode && infiniteStageManager != null)
        {
            // 무한 모드: 웨이브를 스테이지로 사용 (최대 30)
            int waveNumber = infiniteStageManager.GetCurrentWave();
            currentStageNumber = Mathf.Min(Mathf.RoundToInt(Mathf.Pow(waveNumber, 1.2f)), 30);

            // Logger.Log($"[Enemy] Infinite mode - Wave {waveNumber} → Stage {currentStageNumber}");
        }
        else
        {
            // 레귤러 모드: 현재 스테이지
            if (currentStageNumber == 0)
            {
                currentStageNumber = PlayerDataManager.Instance.GetCurrentStageNumber();
            }
            // Logger.Log($"[Enemy] Regular mode - Stage {currentStageNumber}");
        }

        if (calculator == null)
            calculator = GameManager.instance.enemyStatCalculator;

        if (calculator != null)
        {
            this.Stats = calculator.GetStatsForStage(currentStageNumber, _enemyToSpawn);
            // Logger.Log($"[Enemy] Stats - Speed: {Stats.speed}, HP: {Stats.hp}, Damage: {Stats.damage}");
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
            // Logger.LogWarning($"{_enemyToSpawn.Name}: EnemyStatCalculator를 찾을 수 없습니다.");
        }

        if(bossType == BossType.StageBoss || bossType == BossType.QueenBoss)
        {
            Logger.LogError($"[enemyBase] 보스 체력 : {this.Stats.hp}");
        }

        // 속도 설정
        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;

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

        // 화이트 플래시를 한 후 원래 재질로 되돌리기 위한 initial mat 초기화
        if (isSubBoss)
        {
            if (srFlash == null) return;
            initialMat = new Material[srFlash.Length];
            for (int i = 0; i < srFlash.Length; i++)
            {
                initialMat[i] = srFlash[i].material;
            }
        }
    }

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

        // 모르겠다. 일단 뒤집고 나서는 방향을 플레이어쪽으로 바꿔줘서 
        // 뒤집히거나 뒤집히다 말거나 하는 현상을 없애자
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
        Vector2 nextVec = currentSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }
    #endregion

    void IsInsideWall()
    {
        Vector2 dir = (pastPos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Linecast(pastPos, transform.position, LayerMask.GetMask("Wall"));
        if (hit.collider != null) // 벽 안으로 들어갔다면
        {
            transform.position = pastPos;
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

        // 레이저 컴포넌트에 알림 추가
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

        if (IsGrouping && collision.gameObject.CompareTag("Wall"))
        {
            GroupDir = (Player.instance.transform.position - transform.position).normalized;
        }

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
        //if (enemyType != EnemyType.Melee) return; // 근접 공격 적들만 플레이어에서 벗어났을 때 공격모션을 해제한다

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
        // ⭐ 스테이지/웨이브 기반 회피 확률 적용
        float dodgeRoll = UnityEngine.Random.Range(0f, 1f);
        if (dodgeRoll < Stats.dodgeChance)
        {
            // 회피 성공 - 선택적으로 회피 이펙트나 사운드 추가 가능
            // Logger.Log($"[Enemy] Dodged attack! (chance: {Stats.dodgeChance:P0})");
            return;
        }

        // 화면 밖에 있으면 데미지 입지 않기
        CheckOffScreen();
        if (isOffScreen)
            return;

        // Hurt 애니메이션이 재생중이면 또 재생하지 않기
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("SlimeLV1Hurt"))
            anim.SetTrigger("Hit");

        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        if (effect != null) effect.transform.position = hitEffectPoint.position;

        // 넉백
        enemyKnockBackSpeedFactor = knockBackSpeedFactor;

        float _knockBackDelay = 0f;
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance < knockBackChance && knockBackChance != 0)
            _knockBackDelay = this.knockBackDelay;

        // 체력이 0 이하이면 죽음
        Stats.hp -= damage;
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

        // WhiteFlash(whiteFlashDuration);
        // if (isBoss || isSubBoss) SpriteFlash(whiteFlashDuration);
        KnockBack(target, _knockBackDelay, knockBackSpeedFactor);
    }
    public virtual void Die()
    {
        GetComponent<DropOnDestroy>().CheckDrop();

        StopAllCoroutines();

        IsGrouping = false;
        ResetFlip();

        GameManager.instance.KillManager.UpdateCurrentKills(enemyType, isSubBoss, isBoss);

        Spawner.instance.SubtractEnemyNumber();
        if (enemyFinder == null) enemyFinder = FindObjectOfType<EnemyFinder>();

        IsSlowed = false;
        finishedSpawn = false;
        DestroyHPbar();

        // 일반 모드 보스일 떄만
        if (IsBoss && PlayerDataManager.Instance.GetGameMode() == GameMode.Regular)
        {
            BossDieManager bossDieManager = FindObjectOfType<BossDieManager>();
            bossDieManager.SetIsBossDead(true);
            bossDieManager.DieEvent(.1f, 2f);
        }

        if (isSubBoss)
        {
            CameraShake.instance.Shake();
        }

        if (shockwave != null)
        {
            GameObject wave = GameManager.instance.poolManager.GetMisc(shockwave);
            wave.GetComponent<Shockwave>().Init(0, 10f, LayerMask.GetMask("Enemy"), transform.position);

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

        // ⭐ 무한 모드 카운트, 쪼개진 조그만 적이 아닐 때만 카운트
        if (isInfiniteMode && infiniteStageManager != null && isSplited == false)
        {
            infiniteStageManager.OnEnemyKilled();
        }

        OnDeath?.Invoke();

        gameObject.SetActive(false);
    }

    // 보스가 등장할 때 적들은 모두 없어지도록 할 때 쓰는 Die
    // 아무것도 드롭하지 않도록 Drop on Destroy를 포함하지 않음
    // 플레이어의 kill수에도 포함시키지 않음
    public void DieOnBossEvent()
    {
        if (isBoss || isSubBoss) return; // 보스이거나 서브보스라면 없애지 않음
        GameObject explosionEffect = GameManager.instance.feedbackManager.GetDieEffect();
        if (explosionEffect != null) explosionEffect.transform.position = transform.position;

        IsSlowed = false;
        finishedSpawn = false;
        gameObject.SetActive(false);
    }
    public virtual void Deactivate() // 화면 밖으로 사라지는 그룹 적들 경우 아무것도 드롭하지 않고 그냥 사라지도록
    {
        //sr.material = initialMat;
        IsGrouping = false;
        IsSlowed = false;

        gameObject.SetActive(false);
    }
    public virtual void DieWithoutDrop()
    {
        if (whiteFlashCoroutine != null)
            StopCoroutine(whiteFlashCoroutine);

        //sr.material = initialMat;
        IsSlowed = false;
        gameObject.SetActive(false);
    }

    protected virtual void KnockBack(Vector2 target, float knockBackDelay, float knockBackSpeedFactor)
    {
        if (anim.speed == 0) return;
        // knockbackDelay를 0으로 설정해 두었다면 낙백이 일어나지 않음
        if (knockBackDelay != 0) // 낙백이 일어나지 않게. 낵백이 끝나야 kill이 진행된다
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

    public void WhiteFlash(float delayTime)
    {
        // if (gameObject.activeSelf)
        // {
        //     whiteFlashCoroutine = StartCoroutine(WhiteFlashCo(delayTime));
        // }
    }
    void SpriteFlash(float delayTime)
    {
        if (srFlash == null) return;
        if (gameObject.activeSelf)
        {
            if (whiteFlashCoroutine != null) return; // 플래시가 아직 끝나지 않았다면 또 다른 플래시를 실행하지 않음
            whiteFlashCoroutine = StartCoroutine(WhiteFlashCo(delayTime));
        }
    }
    protected IEnumerator SpriteFlashCo(float delayTime)
    {
        // yield return new WaitForSeconds(delayTime);
        foreach (var item in srFlash)
        {
            item.color = new Color(.5f, 0, 0, 1); // 빨강으로 바꾸기
        }

        yield return new WaitForSeconds(.1f);
        foreach (var item in srFlash)
        {
            item.color = new Color(1, 1, 1, 1); // 다시 하얀색으로 되돌려서 원래 색으로 바꾸기
        }
    }

    protected IEnumerator WhiteFlashCo(float delayTime)
    {
        // yield return new WaitForSeconds(delayTime);
        foreach (var item in srFlash)
        {
            item.material = whiteMat;
        }
        yield return new WaitForSeconds(.07f);
        for (int i = 0; i < initialMat.Length; i++)
        {
            srFlash[i].material = initialMat[i];
        }
        yield return new WaitForSeconds(.07f); // 연속 공격을 당할 때 캐릭터가 아예 하얀색으로 보이지 않도록 딜레이를 주기

        whiteFlashCoroutine = null;
    }
    #endregion

    #region 스킬
    public void CastSlownessToEnemy(float _slownessFactor)
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록

        float previousSpeed = currentSpeed;
        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;

        // 최대 속도 제한
        if (currentSpeed >= 15f) currentSpeed = 15f;

        // 최소 속도 제한 추가 (너무 느려지지 않도록)
        if (currentSpeed < 1f) currentSpeed = 1f;

        anim.SetBool("Hypnotized", true);

        // ⭐ 애니메이터 속도 감소 (느린 모션 효과)
        anim.speed = 1f - _slownessFactor;

        // ⭐ 점프 느림 효과 추가
        ShadowHeightEnemy shadowHeight = GetComponent<ShadowHeightEnemy>();
        if (shadowHeight != null)
        {
            shadowHeight.ApplySlowToJump(_slownessFactor);
        }
    }
    // 점프 중 속도를 증가시키기 위해 Shadow Height Enemy에서 호출
    // 최면 애니메이션을 재생하지 않음
    public void SpeedUpOnJump(float _slownessFactor)
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록

        float previousSpeed = currentSpeed;
        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;

        // 최대 속도 제한
        if (currentSpeed >= 15f) currentSpeed = 15f;

        // 최소 속도 제한 추가 (너무 느려지지 않도록)
        if (currentSpeed < 1f) currentSpeed = 1f;
    }

    public void ResetCurrentSpeedToDefault()
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록
        currentSpeed = DefaultSpeed;

        anim.SetBool("Hypnotized", false);

        // ⭐ 애니메이터 속도 복구
        anim.speed = 1f;

        // ⭐ 점프 느림 효과 해제
        ShadowHeightEnemy shadowHeight = GetComponent<ShadowHeightEnemy>();
        if (shadowHeight != null)
        {
            shadowHeight.ReleaseSlowFromJump();
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
        currentSpeed = 0f; // sluggish slumber와 겹치지 않기 위해 CastSlowness 함수를 사용하지 않음.
        //rb.bodyType = RigidbodyType2D.Static;
    }
    public void ResumeEnemy()
    {
        anim.speed = 1;
        currentSpeed = DefaultSpeed; // sluggish slumber와 겹치지 않기 위해 ResetCurrentSpeed 함수를 사용하지 않음.
        //rb.bodyType = RigidbodyType2D.Dynamic;
    }
    public bool isTimeStopped()
    {
        return currentSpeed == 0f ? true : false;
    }

    /// <summary>
    /// 느림 효과 등 상태 이상 색상 적용
    /// </summary>
    public void SetTintColor(Color tintColor)
    {
        // 첫 색상 변경 시 원래 색상 저장
        if (!isColorChanged)
        {
            if (sr != null)
            {
                originalColor = sr.color;
            }
            isColorChanged = true;
        }

        // 메인 스프라이트
        if (sr != null)
        {
            sr.color = tintColor;
        }

        // 서브보스/보스의 경우 여러 스프라이트
        if (srFlash != null && srFlash.Length > 0)
        {
            foreach (var sprite in srFlash)
            {
                if (sprite != null)
                {
                    sprite.color = tintColor;
                }
            }
        }
    }

    /// <summary>
    /// 원래 색상으로 복구 (InitEnemy에서도 호출)
    /// </summary>
    public void ResetTintColor()
    {
        // 색상 상태 초기화
        isColorChanged = false;
        originalColor = Color.white;

        // 메인 스프라이트
        if (sr != null)
        {
            sr.color = Color.white;
        }

        // 서브보스/보스의 경우 여러 스프라이트
        if (srFlash != null && srFlash.Length > 0)
        {
            foreach (var sprite in srFlash)
            {
                if (sprite != null)
                {
                    sprite.color = Color.white;
                }
            }
        }
    }
    #endregion
}