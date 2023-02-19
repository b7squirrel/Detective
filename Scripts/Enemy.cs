using System;
using System.Collections;
using Unity.VisualScripting;
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
        this.experience_reward= stats.experience_reward;
    }
}

public class Enemy : MonoBehaviour, Idamageable
{
    public int ExperienceReward {get; private set;}
    [SerializeField] Rigidbody2D target;

    [Header("Effect")]
    [SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    Material initialMat;
    bool isKncokBack;
    Vector2 targetDir;
    float knockBackSpeed = 8f;

    [Header("Sounds")]
    [SerializeField] AudioClip hit;
    [SerializeField] AudioClip die;

    bool isLive;

    public EnemyStats stats;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        isKncokBack= false;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        initialMat = sr.material;
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
    }

    void Flip()
    {
        if (target.position.x < rb.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    void ApplyMovement()
    {
        if (isKncokBack)
        {
            rb.velocity = knockBackSpeed * targetDir;
            return;
        }
        Vector2 dirVec = target.position - rb.position;
        Vector2 nextVec = dirVec.normalized * stats.speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    public void Init(EnemyData data)
    {
        anim.runtimeAnimatorController = data.animController;
        this.stats = new EnemyStats(data.stats);
        ExperienceReward = this.stats.experience_reward;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (GameManager.instance.player == null)
            return;
        if (collision.gameObject == target.gameObject)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (target.gameObject == null)
            return;

        target.gameObject.GetComponent<Character>().TakeDamage(stats.damage);
    }

    #region Take Damage
    public void TakeDamage(int damage)
    {
        stats.hp -= damage;
        EffectManager.instance.GenerateEffect(0, this.transform);
        SoundManager.instance.Play(hit);
        KnockBack();
    }

    void KnockBack()
    {
        anim.SetTrigger("Hit");
        Vector2 playerPos = target.transform.position;
        isKncokBack = true;
        targetDir = (rb.position - target.position).normalized;
        WhiteFlash(whiteFlashDuration);
    }
    public void WhiteFlash(float delayTime)
    {
        StartCoroutine(WhiteFlashCo(delayTime));
    }

    IEnumerator WhiteFlashCo(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.05f);
        sr.material = initialMat;

        isKncokBack = false;
        if (stats.hp < 1)
        {
            //target.GetComponent<Level>().AddExperience(experienceReward);
            GetComponent<DropOnDestroy>().CheckDrop();
            gameObject.SetActive(false);
        }
    }
    #endregion
}
