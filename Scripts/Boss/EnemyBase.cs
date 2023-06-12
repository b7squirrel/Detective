using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, Idamageable
{
    [field : SerializeField] public string Name {get; private set;}
    [HideInInspector] public bool IsKnockBack{get; set;}
    [HideInInspector] public bool IsStunned{get; set;}
    [HideInInspector] public Rigidbody2D Target{get; set;}
    public EnemyStats Stats {get; set;}
    public bool IsBoss{get; set;}
    public bool IsGrouping { get; set; } // 그룹지어 다니는 적인지 여부
    public Vector2 GroupDir {get; set;} // spawn 할 떄 spawn 포인트 값과 player위치로 결정

    protected bool isOffScreen; // 화면 밖에 있을 때 플레이어의 공격을 받지 않기 위한 플래그
    [SerializeField] LayerMask screenEdge;

    Coroutine flipCoroutine;
    bool isFlipping; // 더 이상 flip하고 있지 않으면 코루틴을 초기화 시키기위해
    float pastFacingDir, currentFacingDir;

    #region Component Variables
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    [SerializeField] protected Material whiteMaterial;
    [SerializeField] protected Transform hitEffectPoint;
    [SerializeField] protected float whiteFlashDuration = 0.08f;
    [SerializeField] protected float knockBackSpeed;
    protected float stunnedDuration = .2f;
    protected Material initialMat;
    [HideInInspector] public Vector2 targetDir;
    protected float stunnedSpeed = 14f;
    Coroutine whiteFlashCoroutine;

    [Header("Sounds")]
    [SerializeField] protected AudioClip hit;
    [SerializeField] protected AudioClip die;
    #endregion

    protected virtual void OnEnable()
    {
        Target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        initialMat = sr.material;
        IsKnockBack = false;
        IsStunned = false;
        isOffScreen = true;

        transform.eulerAngles = Vector3.zero;
        StopFlipCoroutine();
        isFlipping = false;

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

        pastFacingDir = currentFacingDir;
    }

    void StopFlipCoroutine()
    {
        if(flipCoroutine != null) StopCoroutine(flipCoroutine);
    }

    #region Movement Functions
    public virtual void Flip()
    {
        if(isFlipping == false)
        {
            if (flipCoroutine != null) StopCoroutine(flipCoroutine);
        }

        if (Target.position.x - rb.position.x > 0) 
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
        while(index < 2) // 120도까지는 60도씩 2번 회전
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

    public virtual void ApplyMovement()
    {
        if (IsKnockBack)
        {
            rb.velocity = knockBackSpeed * targetDir;
            return;
        }
        if (IsStunned)
        {
            rb.velocity = stunnedSpeed * targetDir;
            return;
        }

        Vector2 dirVec = Target.position - rb.position;
        if(IsGrouping)
        {
            // dirVec = groupDir;
            rb.velocity = Stats.speed * GroupDir;
            Debug.DrawRay(transform.position, GroupDir * 5f);
            return;
        }

        Vector2 nextVec = dirVec.normalized * Stats.speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region 닿으면 player HP 감소
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (GameManager.instance.player == null)
            return;
        if (collision.gameObject == Target.gameObject)
        {
            Attack();
        }
    }

    protected void Attack()
    {
        if (Target.gameObject == null)
            return;

        Target.gameObject.GetComponent<Character>().TakeDamage(Stats.damage);
    }
    #endregion

    protected void CheckOffScreen()
    {
        RaycastHit2D hitEdge = Physics2D.Linecast(transform.position, Player.instance.transform.position, screenEdge);
        if (hitEdge)
        {
            isOffScreen = true;
            // Debug.DrawLine(transform.position, hitEdge.point, Color.yellow);
            return;
        }

        isOffScreen = false;
        // Debug.DrawLine(transform.position, hitEdge.point, Color.yellow);
        
    }

    #region Take Damage
    public virtual void TakeDamage(int damage, float knockBackChance, Vector2 target, GameObject hitEffect)
    {
        CheckOffScreen();
        Debug.Log("IS offscreen = " + isOffScreen);
        if(isOffScreen)
            return;

        anim.SetTrigger("Hit");

        Stats.hp -= damage;
        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        effect.transform.position = target;
        SoundManager.instance.Play(hit);

        float knockBackDelay = 0f;
        float chance = UnityEngine.Random.Range(0, 100);
        // Debug.Log("chance = " + chance + " knockbackChance = " + knockBackChance);
        if (chance < knockBackChance && knockBackChance != 0)
        {
            knockBackDelay = 0.04f;
        }
        WhiteFlash(whiteFlashDuration);
        KnockBack(target, knockBackDelay);
    }
    public virtual void Die()
    {
        GetComponent<DropOnDestroy>().CheckDrop();
        // if (whiteFlashCoroutine != null)
        //     StopCoroutine(whiteFlashCoroutine);

        StopAllCoroutines();

        sr.material = initialMat;
        IsGrouping = false;
        ResetFlip();
        gameObject.SetActive(false);
    }
    public virtual void Deactivate() // 화면 밖으로 사라지는 그룹 적들 경우 아무것도 드롭하지 않고 그냥 사라지도록
    {
        sr.material = initialMat;
        IsGrouping = false;
        gameObject.SetActive(false);
    }
    public virtual void DieWithoutDrop()
    {
        if (whiteFlashCoroutine != null)
            StopCoroutine(whiteFlashCoroutine);

        sr.material = initialMat;
        gameObject.SetActive(false);
    }

    protected virtual void KnockBack(Vector2 target, float knockBackDelay)
    {
        Vector2 fromPlayer = target - (Vector2)Target.transform.position;
        IsKnockBack = true;
        targetDir = (rb.position - target).normalized;

        // float dirX = fromPlayer.x * rb.position.x;
        // float dirY = fromPlayer.y * rb.position.y;

        // float xDir = 0;
        // float yDir = 0;
        // if (dirX < 0)
        //     xDir = -1f;

        // if (dirY < 0)
        //     yDir = -1f;

        // targetDir = new Vector2(xDir * targetDir.x, yDir * targetDir.y);
        if (this.gameObject.activeSelf)
        {
            StartCoroutine(KnockBackDone(knockBackDelay));
        }
    }
    IEnumerator KnockBackDone(float knockBackDelay)
    {
        yield return new WaitForSeconds(knockBackDelay);
        IsKnockBack = false;

        if (Stats.hp < 1)
        {
            Player.instance.transform.GetComponent<Kills>().Add(1);
            Die();
        }
    }

    public virtual void Stunned(Vector2 target)
    {
        IsStunned = true;
        targetDir = (rb.position - target).normalized;
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
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.02f);
        sr.material = initialMat;
    }
    #endregion
}
