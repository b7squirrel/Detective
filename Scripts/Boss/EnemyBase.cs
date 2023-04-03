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

    #region Component Variables
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    [SerializeField] protected Material whiteMaterial;
    [SerializeField] protected Transform hitEffectPoint;
    protected float whiteFlashDuration = 0.08f;
    protected float stunnedDuration = .2f;
    protected Material initialMat;
    [HideInInspector] public Vector2 targetDir;
    protected float knockBackSpeed = 8f;
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
    }

    #region Movement Functions
    public void Flip()
    {
        if (Target.position.x < rb.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        if(IsGrouping)
        {
            if(GroupDir.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 180f, 0);
            }
        }
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

    #region Take Damage
    public virtual void TakeDamage(int damage, float knockBackChance, Vector2 target)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            anim.SetTrigger("Hit");
            Debug.Log("Hit Anim");
        }

        Stats.hp -= damage;
        EffectManager.instance.GenerateEffect(0, hitEffectPoint);
        SoundManager.instance.Play(hit);
        float chance = UnityEngine.Random.Range(0, 100);

        if (Stats.hp < 1)
        {
            Player.instance.transform.GetComponent<Kills>().Add(1);
            Die();
            return;
        }

        WhiteFlash(whiteFlashDuration);
        if (chance > knockBackChance || knockBackChance == 0)
        {
            IsKnockBack = false;
            return;
        }
        KnockBack();
    }
    public virtual void Die()
    {
        //target.GetComponent<Level>().AddExperience(experienceReward);
        GetComponent<DropOnDestroy>().CheckDrop();
        if (whiteFlashCoroutine != null)
            StopCoroutine(whiteFlashCoroutine);

        sr.material = initialMat;
        IsGrouping = false;
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

    protected virtual void KnockBack()
    {
        Vector2 playerPos = Target.transform.position;
        IsKnockBack = true;
        targetDir = (rb.position - Target.position).normalized;
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
        whiteFlashCoroutine = StartCoroutine(WhiteFlashCo(delayTime));
    }

    protected IEnumerator WhiteFlashCo(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.05f);
        sr.material = initialMat;

        IsKnockBack = false;
    }
    #endregion
}
