using System.Collections;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    BoxCollider2D boxCol;
    Player player;
    bool canMultiStrike;
    bool multiStrikeDone;

    [Header("Sounds")]
    [SerializeField] AudioClip punch;

    protected override void Awake()
    {
        base.Awake();
        boxCol = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
        weapon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Idamageable enemy = collision.transform.GetComponent<Idamageable>();

        if (enemy != null)
        {

            PostMessage(damage, collision.transform.position);

            GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
            enemy.TakeDamage(damage, 
                             knockback, 
                             knockbackSpeedFactor,
                             collision.ClosestPoint(transform.position), 
                             hitEffect);
        }
    }

    protected override void Attack()
    {
        base.Attack();

        weapon.SetActive(true);
        multiStrikeDone = false;
        
        if (weaponStats.numberOfAttacks < 2)
        {
            canMultiStrike = false;
        }
        else
        {
            canMultiStrike = true;
        }

        if (player.FacingDir < 0)
        {
            anim.SetTrigger("PunchL");
        }
        else
        {
            anim.SetTrigger("PunchR");
        }
        SoundManager.instance.Play(punch);
    }


    IEnumerator AttackCo(float firstAttackDirection)
    {
        yield return new WaitForSeconds(.1f);

        GetAttackParameters();

        if (firstAttackDirection < 0)
        {
            anim.SetTrigger("PunchR");
        }
        else
        {
            anim.SetTrigger("PunchL");
        }

        SoundManager.instance.Play(punch);

        multiStrikeDone = true;
    }

    protected override void FlipWeaponTools()
    {
        // Debug.Log("Whip");
    }

    // animation events
    void BoxColOn() => boxCol.enabled = true;
    void BoxColOff() => boxCol.enabled = false;
    void MultiAttack(float firstAttackDirection)
    {
        if (multiStrikeDone)
            return;
        if (canMultiStrike)
        {
            StartCoroutine(AttackCo(firstAttackDirection));
        }
    }
}
