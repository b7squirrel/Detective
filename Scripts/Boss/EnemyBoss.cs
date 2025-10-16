using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : EnemyBase, Idamageable
{
    Spawner spawner;
    [field: SerializeField] public float moveSpeedInAir { get; private set; }
    [field: SerializeField] public bool IsInAir { get; set; }

    [Header("공격")]
    [SerializeField] EnemyData[] projectiles; // 날으는 슬라임 적
    [SerializeField] GameObject slimeProjectilePrefab; // 슬라임 점액
    [SerializeField] float slimeProjectileSpeed; // 슬라임 점액 속도
    [SerializeField] AudioClip[] projectileSFX;
    [SerializeField] int numberOfProjectile;
    [SerializeField] int maxProjectile;
    [SerializeField] float timeToAttack;
    [SerializeField] Transform slimeDropPos; // 슬라임 점액을 떨어트리는 위치
    float slimeDropTimer; // 슬라임 점액을 떨어트리는 타이밍 카운터. 주기는 각 상태에서 정함
    SlimeDropManager slimeDropManager;

    #region 상태 액션 이벤트 변수
    public static event Action OnState1Enter; // 첫 번째 상태 Enter
    public static event Action OnState1Update; // 두 번째 상태 Update
    public static event Action OnState1Exit; // 세 번째 상태 Exit
    public static event Action OnState2Enter; // 두 번째 상태 Enter
    public static event Action OnState2Update; // 두 번째 상태 Update
    public static event Action OnState2Exit; // 두 번째 상태 Exit
    public static event Action OnState3Enter;// 세 번째 상태 Enter
    public static event Action OnState3Update;// 세 번째 상태 Update
    public static event Action OnState3Exit;// 세 번째 상태 Exit
    public static event Action OnState2AnticEnter;// 두 번째 상태 anitic Enter
    public static event Action OnState2AnticUpdate;// 두 번째 상태 anitic Update
    public static event Action OnState2AnticExit;// 두 번째 상태 anitic Exit
    public static event Action OnState3AnticEnter;// 세 번째 상태 antic Enter
    public static event Action OnState3AnticUpdate;// 세 번째 상태 antic Update
    public static event Action OnState3AnticExit;// 세 번째 상태 antic Exit
    public static event Action OnState1SettleEnter;
    public static event Action OnState1SettleUpdate;
    public static event Action OnState1SettleExit;
    public static event Action OnState2SettleEnter;
    public static event Action OnState2SettleUpdate;
    public static event Action OnState2SettleExit;
    public static event Action OnState3SettleEnter;
    public static event Action OnState3SettleUpdate;
    public static event Action OnState3SettleExit;

    public Vector2 prevDir; // 뒤로 튕겨나갈 때 필요한 방향 벡터

    List<string> dialogs = new(); // state1, 2, 3 대사
    #endregion

    [SerializeField] Transform ShootPoint;
    [SerializeField] Transform dialogBubblePoint;

    [Header("이펙트")]
    [SerializeField] Transform dustPoint;
    [SerializeField] GameObject dustEffect;
    GameObject dust;
    [SerializeField] int halfWallBouncerNumber;
    GenerateWalls generateWalls;
    float timer; // shoot coolTime counter

    [Header("기타")]
    SpriteRenderer spriteRen;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject deadBody;


    public Coroutine shootCoroutine;

    WallManager wallManager;
    Vector2 currentPosition;

    [Header("이펙트 및 사운드 이펙트")]
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
    [SerializeField] GameObject stateDisplayPrefab; // 상태 표시 프리펩
    GameObject stateDisp; // 상태 표시. walk, dash, shoot 등등
    [SerializeField] bool debugState; // 상태 표시 디스플레이를 표시할지 여부
    BossStateDisp bossStateDisp; // 상태 디스플레이
    [SerializeField] Transform stateDiplayPoint; // 상태 디스플레이를 위치시킬 로케이터
    float debugAlpha;
    [SerializeField] bool debugSetState; // 특정 상태만 계속 나오게 하고 싶다면 체크
    [SerializeField] int desiredStateIndex; // 계속 나오게 하고 싶은 상태 인덱스

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

        //대사 초기화
        this.dialogs.AddRange(_enemyToSpawn.dialogs);

        if (debugState)
        {
            stateDisp = Instantiate(stateDisplayPrefab);
            StartCoroutine(SetStateDispPosToPlayer());
        }
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

    public void SlimeDropTimer(float dropTime)
    {
        if (slimeDropTimer < dropTime)
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
        slimeDropManager.DropObject(slimeDropPos.position); // 이동할 때는 보스의 가운데에서 점액이 나오는 것이 나아보인다
    }
    void DropSlimeOnLanding()
    {
        // 착지 시 슬라임 점액
        if (slimeDropManager == null) slimeDropManager = GetComponent<SlimeDropManager>();
        slimeDropManager.DropObjectOnLanding(dustPoint.position); // 랜딩할 때는 인디케이터와 일치되는 위치에 점액이 떨어지는 것이 좋아 보인다
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
        // Enemy, InAir 등등
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
    public void SetTriggerTo(string trigger)
    {
        anim.SetTrigger(trigger);
    }
    #endregion

    #region 상태 함수, 애니메이션 이벤트
    public void LandingImpact()
    {
        SoundManager.instance.Play(landingSFX);
        Vector2 landingEffectPos = (Vector2)dustPoint.transform.position;

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
    // 애니메이션 이벤트로 쓰기 위해 매개 변수를 bool로 하지 않고 정수로 활성/비활성
    public void ActivateLandingIndicator(int _activate)
    {
        if (LandingIndicator == null)
        {
            LandingIndicator = Instantiate(LandingIndicatorPrefab, transform);
            LandingIndicator.transform.localPosition = Vector2.zero;
            LandingIndicator.transform.localScale = .8f * Vector2.one;
        }
        bool active = _activate == 1 ? true : false;
        LandingIndicator.SetActive(active);
    }
    public void GenTeleportEffect()
    {
        GameObject teleEffect = Instantiate(teleEffectPrefab, transform.position, Quaternion.identity);
    }
    public Transform GetShootPoint()
    {
        return ShootPoint;
    }
    public Transform GetDialogBubblePoint()
    {
        return dialogBubblePoint;
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

    // 랜덤하게 3개의 공격 가운데 하나를 선택하기 위한 인덱스.
    public void SetRandomState()
    {
        string[] states = { "State1", "State2", "State3" };

        if (debugSetState)
        {
            anim.SetTrigger(states[desiredStateIndex]);
            return;
        }

        // 0~99 사이의 난수를 생성
        int rand = UnityEngine.Random.Range(0, 100);

        int stateIndex;
        if (rand < 20)
        {
            stateIndex = 0; // State1: 10%
        }
        else if (rand < 60)
        {
            stateIndex = 1; // State2: 45%
        }
        else
        {
            stateIndex = 2; // State3: 45%
        }

        anim.SetTrigger(states[stateIndex]);
    }
    public void SetState(float state1Probability, float state2Probability, float state3Probability)
{
    string[] states = { "State1", "State2", "State3" };

    if (debugSetState)
    {
        anim.SetTrigger(states[desiredStateIndex]);
        return;
    }

    float total = state1Probability + state2Probability + state3Probability;
    if (total <= 0f)
    {
        Debug.LogWarning("Total probability must be greater than zero.");
        return;
    }

    // 정규화
    float p1 = state1Probability / total;
    float p2 = state2Probability / total;
    float p3 = state3Probability / total;

    float rand = UnityEngine.Random.value; // 0.0f ~ 1.0f

    int stateIndex;
    if (rand < p1)
    {
        stateIndex = 0;
    }
    else if (rand < p1 + p2)
    {
        stateIndex = 1;
    }
    else
    {
        stateIndex = 2;
    }

    anim.SetTrigger(states[stateIndex]);
}

    public void ExecuteState1Enter() => OnState1Enter?.Invoke();
    public void ExecuteState1Update() => OnState1Update?.Invoke();
    public void ExecuteState1Exit() => OnState1Exit?.Invoke();
    public void ExecuteState2Enter() => OnState2Enter?.Invoke();
    public void ExecuteState2Update() => OnState2Update?.Invoke();
    public void ExecuteState2Exit() => OnState2Exit?.Invoke();
    public void ExecuteState3Enter() => OnState3Enter?.Invoke();
    public void ExecuteState3Update() => OnState3Update?.Invoke();
    public void ExecuteState3Exit() => OnState3Exit?.Invoke();
    public void ExecuteState2AnticEnter() => OnState2AnticEnter?.Invoke();
    public void ExecuteState2AnticUpdate() => OnState2AnticUpdate?.Invoke();
    public void ExecuteState2AnticExit() => OnState2AnticExit?.Invoke();
    public void ExecuteState3AnticEnter() => OnState3AnticEnter?.Invoke();
    public void ExecuteState3AnticUpdate() => OnState3AnticUpdate?.Invoke();
    public void ExecuteState3AnticExit() => OnState3AnticExit?.Invoke();
    public void ExecuteState1SettleEnter() => OnState1SettleEnter?.Invoke();
    public void ExecuteState1SettleUpdate() => OnState1SettleUpdate?.Invoke();
    public void ExecuteState1SettleExit() => OnState1SettleExit?.Invoke();
    public void ExecuteState2SettleEnter() => OnState2SettleEnter?.Invoke();
    public void ExecuteState2SettleUpdate() => OnState2SettleUpdate?.Invoke();
    public void ExecuteState2SettleExit() => OnState2SettleExit?.Invoke();
    public void ExecuteState3SettleEnter() => OnState3SettleEnter?.Invoke();
    public void ExecuteState3SettleUpdate() => OnState3SettleUpdate?.Invoke();
    public void ExecuteState3SettleExit() => OnState3SettleExit?.Invoke();

    public string GetDialog(int stateIndex)
    {
        return dialogs[stateIndex];
    }

    public Vector2 GetPrevDir()
    {
        return prevDir;
    }
    public void SetPrevDir(Vector2 previousDirection)
    {
        prevDir = previousDirection;
    }

    // 투사체를 발사할 때 멈춰있고, 플레이어에게 밀려나지 않도록 할 때 사용
    public void SetMovable(bool movable)
    {
        rb.bodyType = movable ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
    }

    IEnumerator SetStateDispPosToPlayer()
    {
        // 디버그 함수를 update에 넣으면 보기에 복잡하니까 코루틴으로 매 프레임마다 따라다니도록 하기
        while (true)
        {
            stateDisp.transform.position = stateDiplayPoint.position;
            yield return null;
        }
    }
    // 상태 스크립트로 진입할 때 enter에서 실행하기
    public void DisplayCurrentState(string currentState)
    {
        if (debugState == false) return;
        if (bossStateDisp == null) bossStateDisp = stateDisp.GetComponent<BossStateDisp>();
        bossStateDisp.SetStateText(currentState);
    }
    #endregion
}
