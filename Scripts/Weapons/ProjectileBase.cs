using System.Collections;
using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    
    [field : SerializeField]public Vector3 Direction { get; set; }
    [field : SerializeField] public float Speed {get; set;}
    protected bool hitDetected = false;
    public int Damage { get; set; } = 5;
    public float KnockBackChance {get; set;}
    public bool IsCriticalDamageProj {get; set;}
    [field : SerializeField] public float TimeToLive {get; set;} = 3f;

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
            return;
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
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enmey = hit[i].GetComponent<Transform>();
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
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    protected virtual void DieProjectile()
    {
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
}
