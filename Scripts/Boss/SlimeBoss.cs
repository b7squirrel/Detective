using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BossStats
{
    public int hp = 999;
    public float speed = 5;
    public int damage = 1;
    public int experience_reward = 0;

    public BossStats(EnemyStats stats)
    {
        this.hp = stats.hp;
        this.speed = stats.speed;
        this.damage = stats.damage;
        this.speed = stats.speed;
        this.experience_reward= stats.experience_reward;
    }
}

public class SlimeBoss : MonoBehaviour, Idamageable
{
    Rigidbody2D target;

    #region FeedBack Variables
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
    #endregion

    #region Components Variables
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    BossStats stats;
    #endregion

    #region Unity CallBack Functions
    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isKncokBack= false;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        initialMat = sr.material;
    }
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        
    }
    #endregion

    #region 닿으면 플레이어의 HP 감소
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
    #endregion

    #region Take Damage
    public void TakeDamage(int damage, float knockBackChance)
    {
        stats.hp -= damage;
        EffectManager.instance.GenerateEffect(0, this.transform);
        SoundManager.instance.Play(hit);
        float chance = UnityEngine.Random.Range(0, 100);

        WhiteFlash(whiteFlashDuration);
        if (chance > knockBackChance || knockBackChance == 0)
        {
            isKncokBack = false;
            return;
        }
        KnockBack();
    }

    void KnockBack()
    {
        anim.SetTrigger("Hit");
        Vector2 playerPos = target.transform.position;
        isKncokBack = true;
        targetDir = (rb.position - target.position).normalized;
        
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
