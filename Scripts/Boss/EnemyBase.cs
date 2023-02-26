using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public bool IsKnockBack{get; set;}
    public Rigidbody2D Target{get; set;}
    public EnemyStats Stats {get; set;}

    #region Component Variables
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    #endregion

    #region FeedBack Variables
    [Header("Effect")]
    [SerializeField] protected Material whiteMaterial;
    protected float whiteFlashDuration = 0.08f;
    protected Material initialMat;
    protected Vector2 targetDir;
    protected float knockBackSpeed = 8f;

    [Header("Sounds")]
    [SerializeField] protected AudioClip hit;
    [SerializeField] protected AudioClip die;
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
    public void TakeDamage(int damage, float knockBackChance)
    {
        Stats.hp -= damage;
        EffectManager.instance.GenerateEffect(0, this.transform);
        SoundManager.instance.Play(hit);
        float chance = UnityEngine.Random.Range(0, 100);

        WhiteFlash(whiteFlashDuration);
        if (chance > knockBackChance || knockBackChance == 0)
        {
            IsKnockBack = false;
            return;
        }
        KnockBack();
    }

    protected void KnockBack()
    {
        anim.SetTrigger("Hit");
        Vector2 playerPos = Target.transform.position;
        IsKnockBack = true;
        targetDir = (rb.position - Target.position).normalized;
        
    }
    public void WhiteFlash(float delayTime)
    {
        StartCoroutine(WhiteFlashCo(delayTime));
    }

    protected IEnumerator WhiteFlashCo(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.05f);
        sr.material = initialMat;

        IsKnockBack = false;
        if (Stats.hp < 1)
        {
            //target.GetComponent<Level>().AddExperience(experienceReward);
            GetComponent<DropOnDestroy>().CheckDrop();
            gameObject.SetActive(false);
        }
    }
    #endregion
}
