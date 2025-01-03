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
    protected float DefaultSpeed; // 속도를 외부에서 변경할 때 원래 속도를 저장해 두기 위해
    protected float currentSpeed;
    public bool IsSlowed { get; set; } // 슬로우 스킬을 
    public bool IsBoss { get; set; }
    [SerializeField] protected bool isSubBoss;
    [SerializeField] protected bool isBoss;
    [SerializeField] int numberOfSubBossDrops;
    [SerializeField]
    int numberOfBossDrops;
    public bool IsGrouping { get; set; } // 그룹지어 다니는 적인지 여부
    public Vector2 GroupDir { get; set; } // spawn 할 떄 spawn 포인트 값과 player위치로 결정

    protected EnemyType enemyType; // Melee, Explode 공격만 EnemyBase에서 정의하고 Ranged는 Enemy에서 정의

    protected bool isOffScreen; // 화면 밖에 있을 때 플레이어의 공격을 받지 않기 위한 플래그
    protected float offScreenCoolDown; // 너무 자주 콜라이더가 활성, 비활성 되지 않도록 쿨타임 주기
    [SerializeField] LayerMask screenEdge;

    Coroutine flipCoroutine;
    bool isFlipping; // 더 이상 flip하고 있지 않으면 코루틴을 초기화 시키기위해
    float pastFacingDir, currentFacingDir;

    bool initDone;
    public bool finishedSpawn; // 스폰이 끝나면 적이 이동하도록 하려고

    Vector2 pastPos; // 벽 바깥으로 나가면 다시 되돌리기 위한 변수
    #endregion

    #region Component Variables
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Collider2D colEnemy;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    //[SerializeField] protected Material whiteMaterial;
    [SerializeField] protected GameObject dieEffectPrefeab;
    [SerializeField] protected GameObject dieExplosionPrefeab;
    [SerializeField] protected GameObject knockbackEffect;
    [SerializeField] protected Transform hitEffectPoint;
    [SerializeField] protected float whiteFlashDuration = 0.08f;
    [SerializeField] protected float knockBackSpeed;
    [SerializeField] protected float knockBackDelay;
    protected GameObject enemyProjectile;
    protected float enemyKnockBackSpeedFactor; // TakeDamage 때마다 인자로 넘어오는 knockBackSpeedFactor를 담아 두는 용도
    protected float stunnedDuration = .2f;

    //protected Material initialMat;
    //[SerializeField] protected Material whiteMat;

    [HideInInspector] public Vector2 targetDir;
    protected float stunnedSpeed = 14f;
    Coroutine whiteFlashCoroutine;
    Color enemyColor; // die effect의 색깔을 정하기 위해서.

    [Header("Sounds")]
    [SerializeField] protected AudioClip[] hits;
    [SerializeField] protected AudioClip[] dies;

    [Header("HP Bar")]
    [SerializeField] GameObject HPbarPrefab;
    protected GameObject HPbar;
    [SerializeField] Transform HpBarPos;
    [SerializeField] protected StatusBar hpBar;
    protected int maxHealth;

    [Header("Shock Wave")]
    [SerializeField] protected GameObject shockwave;

    EnemyFinder enemyFinder;
    #endregion

    #region 유니티 콜백
    protected virtual void OnEnable()
    {
        if (initDone == false)
        {
            Target = GameManager.instance.player.GetComponent<Rigidbody2D>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sr = GetComponentInChildren<SpriteRenderer>();
            colEnemy = GetComponent<Collider2D>();

            pastPos = transform.position;

            initDone = true;
        }

        //initialMat = sr.material;
        IsKnockBack = false;
        IsStunned = false;
        isOffScreen = true;

        transform.eulerAngles = Vector3.zero;
        StopFlipCoroutine();
        isFlipping = false;

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
        enemyColor = _enemyToSpawn.enemyColor;
        // 적과 보스 공통으로 사용하기 위해서 virtual로 했음
        // 각자 덮어쓰기 하면 됨
    }

    #endregion

    #region Movement Functions
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

        if (currentFacingDir != pastFacingDir && isFlipping == false)
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

    public virtual void ApplyMovement()
    {
        if (finishedSpawn == false) return; // 스폰이 완료되지 않았다면 이동 금지. 스폰 애니메이션에서 이벤트로 설정
        if (IsKnockBack)
        {
            rb.velocity = knockBackSpeed * targetDir;
            //rb.velocity = knockBackSpeed * enemyKnockBackSpeedFactor * targetDir;
            float randomOffsetX = UnityEngine.Random.Range(-.5f, .5f);
            float randomOffsetY = UnityEngine.Random.Range(-.5f, .5f);
            SpawnManager.instance.SpawnObject(transform.position + new Vector3(randomOffsetX, randomOffsetY, 0), knockbackEffect, false, 0);
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

    // animation events
    // 스폰 애니메이션이 끝나는 지점에 이벤트
    public void TriggerFinishedSpawn()
    {
        finishedSpawn = true;
    }
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
            if (enemyType == EnemyType.Melee && Time.frameCount % 3 == 0) // Melee는 3프레임에 한 번 공격
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
            for (int i = 0; i < dies.Length; i++)
            {
                SoundManager.instance.PlaySoundWith(dies[i], 1f, true, .2f);
            }

            Die();
        }
        else
        {
            for (int i = 0; i < hits.Length; i++)
            {
                SoundManager.instance.PlaySoundWith(hits[i], 1f, true, .2f);
            }

            if (hpBar != null)
            {
                hpBar.SetStatus(Stats.hp, maxHealth);
            }
        }

        WhiteFlash(whiteFlashDuration);
        KnockBack(target, _knockBackDelay, knockBackSpeedFactor);
    }
    public virtual void Die()
    {
        GameObject explosionEffect = GameManager.instance.feedbackManager.GetDieEffect();
        if (explosionEffect != null) explosionEffect.transform.position = transform.position;

        GetComponent<DropOnDestroy>().CheckDrop();

        StopAllCoroutines();

        //sr.material = initialMat;
        IsGrouping = false;
        ResetFlip();

        GameManager.instance.KillManager.UpdateCurrentKills(); // 처치한 적의 수 세기

        Spawner.instance.SubtractEnemyNumber();
        if (enemyFinder == null) enemyFinder = FindObjectOfType<EnemyFinder>();
        //enemyFinder.RemoveEnemyFromList(transform);

        IsSlowed = false;
        finishedSpawn = false;
        DestroyHPbar();

        if (IsBoss)
        {
            FindObjectOfType<BossDieManager>().DieEvent(.1f, 2f);
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
        if (gameObject.activeSelf)
        {
            whiteFlashCoroutine = StartCoroutine(WhiteFlashCo(delayTime));
        }
    }

    protected IEnumerator WhiteFlashCo(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //sr.material = whiteMat;
        yield return new WaitForSeconds(.1f);
        //sr.material = initialMat;
    }
    #endregion

    #region 스킬
    public void CastSlownessToEnemy(float _slownessFactor)
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록
        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;
        //currentSpeed = DefaultSpeed - DefaultSpeed * .2f;
        Debug.Log($"Default Speed = {DefaultSpeed}, Current Speed = {currentSpeed}");
    }
    public void ResetCurrentSpeedToDefault()
    {
        if (currentSpeed == 0) return; // 스톱워치로 시간을 정지시킨 상태에서 작동하지 않도록
        currentSpeed = DefaultSpeed;
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
    #endregion
}