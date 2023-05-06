using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    
    [field : SerializeField]public Vector3 Direction { get; set; }
    [field : SerializeField] public float Speed {get; set;}
    protected bool hitDetected = false;
    public int Damage { get; set; } = 5;
    [field : SerializeField] public float TimeToLive {get; set;} = 3f;
    public float KnockBackChance {get; set;}

    protected virtual void Update()
    {
        ApplyMovement();
        CastDamage();
        
        AttackCoolTimer();
    }

    protected virtual void AttackCoolTimer()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
        {
            DieProjectile();
        }
    }

    protected virtual void ApplyMovement()
    {
        transform.position += Speed * Time.deltaTime * Direction.normalized;
    }

    protected virtual void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
        foreach (var item in hit)
        {
            Transform enmey = item.GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                PostMessage(Damage, enmey.transform.position);
                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, transform.position, hitEffect);
                hitDetected = true;
                break;
            }
        }
        if (hitDetected == true)
        {
            HitObject();
            hitDetected = false;
        }
    }

    protected virtual void HitObject()
    {
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }

    protected virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition);
    }

    protected virtual void DieProjectile()
    {
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
}
