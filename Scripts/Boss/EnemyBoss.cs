using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBoss : MonoBehaviour, Idamageable
{
    [field : SerializeField] public string BossName{get; private set;}

    #region Variables Anim States Refer to
    public EnemyStats stats;
    public bool isKncokBack;
    #endregion

    #region Component Variables
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Rigidbody2D target;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    [SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    Material initialMat;
    Vector2 targetDir;
    float knockBackSpeed = 8f;

    [Header("Sounds")]
    [SerializeField] AudioClip hit;
    [SerializeField] AudioClip die;
    #endregion
    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        initialMat = sr.material;
        isKncokBack= false;
    }

    public void Init(EnemyData data)
    {
        this.stats = new EnemyStats(data.stats);
    }

    #region 닿으면 player HP 감소
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
