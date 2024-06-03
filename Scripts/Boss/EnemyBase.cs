using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, Idamageable
{
    #region Variables
    [field : SerializeField] public string Name {get; private set;} // 체력바에 표시할 이름이 필요한 적들만 사용
    [HideInInspector] public bool IsKnockBack{get; set;}
    [HideInInspector] public bool IsStunned{get; set;}
    [HideInInspector] public Rigidbody2D Target{get; set;}
    public EnemyStats Stats {get; set; }
    protected float DefaultSpeed; // 속도를 외부에서 변경할 때 원래 속도를 저장해 두기 위해
    protected float currentSpeed;
    public bool IsSlowed {get; set;} // 슬로우 스킬을 
    public bool IsBoss{get; set;}
    [SerializeField] protected bool isSubBoss;
    [SerializeField] protected bool isBoss;
    [SerializeField] int numberOfSubBossDrops;
    [SerializeField] int numberOfBossDrops;
    public bool IsGrouping { get; set; } // 그룹지어 다니는 적인지 여부
    public Vector2 GroupDir {get; set;} // spawn 할 떄 spawn 포인트 값과 player위치로 결정

    protected bool isOffScreen; // 화면 밖에 있을 때 플레이어의 공격을 받지 않기 위한 플래그
    protected float offScreenCoolDown; // 너무 자주 콜라이더가 활성, 비활성 되지 않도록 쿨타임 주기
    [SerializeField] LayerMask screenEdge;

    Coroutine flipCoroutine;
    bool isFlipping; // 더 이상 flip하고 있지 않으면 코루틴을 초기화 시키기위해
    float pastFacingDir, currentFacingDir;
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
    protected float enemyKnockBackSpeedFactor; // TakeDamage 때마다 인자로 넘어오는 knockBackSpeedFactor를 담아 두는 용도
    protected float stunnedDuration = .2f;

    protected Material initialMat;
    [SerializeField] protected Material whiteMat;

    [HideInInspector] public Vector2 targetDir;
    protected float stunnedSpeed = 14f;
    Coroutine whiteFlashCoroutine;

    [Header("Sounds")]
    [SerializeField] protected AudioClip hit;
    [SerializeField] protected AudioClip die;

    [Header("HP Bar")]
    [SerializeField] GameObject HPbarPrefab;
    protected GameObject HPbar;
    [SerializeField] Transform HpBarPos;
    [SerializeField] protected StatusBar hpBar;
    protected int maxHealth;

    EnemyFinder enemyFinder;
    #endregion

    #region 유니티 콜백
    protected virtual void OnEnable()
    {
        Target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        colEnemy = GetComponent<Collider2D>();

        initialMat = sr.material;
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

        if(Stats != null)
        {
            
        }
    }
    public void CastSlownessToEnemy(float _slownessFactor)
    {
        currentSpeed = DefaultSpeed - DefaultSpeed * _slownessFactor;
    }
    public void ResetCurrentSpeedToDefault()
    {
        currentSpeed = DefaultSpeed;
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
        //isOffScreen = !(sr.isVisible);
        //if (isOffScreen)
        //{
        //    colEnemy.enabled = false;
        //    Debug.Log("Col disabled");
        //}
        //else
        //{
        //    colEnemy.enabled = true;
        //    Debug.Log("Col enabled");
        //}
        //colEnemy.enabled = isOffScreen;
    }
    #endregion

    #region Movement Functions
    public virtual void Flip()
    {
        if (gameObject.activeSelf == false) return;
        
        if (anim.speed == 0) return; // 스탑워치가 작동하고 있다면 플립되지 않음
        if(isFlipping == false)
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
        while(index < 3) // 120도부터는 20도씩 3번 회전
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
        if(IsGrouping)
        {
            // dirVec = groupDir;
            //rb.velocity = Stats.speed * GroupDir;
            rb.velocity = currentSpeed * GroupDir;
            return;
        }
        //Debug.Log("Target Position = " + GameManager.instance.player.transform.position + " dir Vec = " + dirVec);
        //Vector2 nextVec = dirVec.normalized * Stats.speed * Time.fixedDeltaTime;
        Vector2 nextVec = currentSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region 닿으면 player HP 감소
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if(IsGrouping && collision.gameObject.CompareTag("Wall"))
        {
            GroupDir = (Player.instance.transform.position - transform.position).normalized;
        }

        if (GameManager.instance.player == null)
            return;
        if (collision.gameObject == Target.gameObject)
        {
            Attack();
        }
    }
    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (GameManager.instance.player == null)
            return;
        if (collision.gameObject == Target.gameObject)
        {
            anim.SetBool("Attack", false);
            Debug.Log("Exit Attack");
        }
    }

    protected void Attack()
    {
        if (Target.gameObject == null)
            return;

        anim.SetBool("Attack", true);
        Target.gameObject.GetComponent<Character>().TakeDamage(Stats.damage);
    }
    #endregion

    #region Take Damage
    protected void CheckOffScreen()
    {
        isOffScreen = !(sr.isVisible);
    }

    public virtual void TakeDamage(int damage, float knockBackChance, float knockBackSpeedFactor, Vector2 target, GameObject hitEffect)
    {
        CheckOffScreen();
        if(isOffScreen)
            return;

        // anim.SetTrigger("Hit");

        Stats.hp -= damage;
        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        if (effect != null) effect.transform.position = hitEffectPoint.position;
        SoundManager.instance.Play(hit);

        enemyKnockBackSpeedFactor = knockBackSpeedFactor;

        float _knockBackDelay = 0f;
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance < knockBackChance && knockBackChance != 0)
        {
            _knockBackDelay = this.knockBackDelay;
        }

        if (Stats.hp < 1)
        {
            //Player.instance.transform.GetComponent<Kills>().Add(1);
            Die();
        }
        else
        {
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
        if (dieEffectPrefeab != null)
        {
            GameObject dieEffect = GameManager.instance.poolManager.GetMisc(dieEffectPrefeab);
            if (dieEffect != null) dieEffect.transform.position = transform.position;
        }
        if (dieExplosionPrefeab != null)
        {
            GameObject explosionEffect = GameManager.instance.poolManager.GetMisc(dieExplosionPrefeab);
            if (explosionEffect != null) explosionEffect.transform.position = transform.position;
        }

        if (isSubBoss)
        {
            GetComponent<DropOnDestroy>().DropMultipleObjects();
        }
        if (isBoss)
        {
            GetComponent<DropOnDestroy>().DropMultipleBossObjects();
        }

        GetComponent<DropOnDestroy>().CheckDrop();

        StopAllCoroutines();

        sr.material = initialMat;
        IsGrouping = false;
        ResetFlip();

        GameManager.instance.KillManager.UpdateCurrentKills(); // 처치한 적의 수 세기

        Spawner.instance.SubtractEnemyNumber();
        if(enemyFinder == null) enemyFinder = FindObjectOfType<EnemyFinder>();
        enemyFinder.RemoveEnemyFromList(transform);
        
        IsSlowed = false;
        DestroyHPbar();
        gameObject.SetActive(false);
    }
    public virtual void Deactivate() // 화면 밖으로 사라지는 그룹 적들 경우 아무것도 드롭하지 않고 그냥 사라지도록
    {
        sr.material = initialMat;
        IsGrouping = false;
        IsSlowed = false;

        gameObject.SetActive(false);
    }
    public virtual void DieWithoutDrop()
    {
        if (whiteFlashCoroutine != null)
            StopCoroutine(whiteFlashCoroutine);

        sr.material = initialMat;
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
        sr.material = whiteMat;
        yield return new WaitForSeconds(.1f);
        sr.material = initialMat;
    }
    #endregion

    public void PauseEnemy()
    {
        anim.speed = 0f;
        CastSlownessToEnemy(1f);
        //rb.bodyType = RigidbodyType2D.Static;
    }
    public void ResumeEnemy()
    {
        anim.speed = 1;
        ResetCurrentSpeedToDefault();
        //rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
