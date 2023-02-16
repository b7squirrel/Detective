using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    
    [field : SerializeField] public Vector3 Direction { get; set; }
    [field : SerializeField] public float Speed {get; private set;}
    protected bool hitDetected = false;
    public int Damage { get; set; } = 5;
    [field : SerializeField] public float TimeToLive {get; set;} = 6f;


    protected virtual void Update()
    {
        ApplyMovement();
        CastDamage();
        
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void ApplyMovement()
    {
        transform.position += Speed * Time.deltaTime * Direction.normalized;
    }

    protected virtual void CastDamage()
    {
        if (Time.frameCount % 3 == 0)
        {
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
            foreach (var item in hit)
            {
                Transform enmey = item.GetComponent<Transform>(); 
                if (enmey.GetComponent<Idamageable>() != null)
                {
                    PostMessage(Damage, enmey.transform.position);
                    enmey.GetComponent<Idamageable>().TakeDamage(Damage);
                    hitDetected = true;
                    break;
                }
            }
            if (hitDetected == true)
            {
                HitObject();
            }
        }
    }

    protected virtual void HitObject()
    {
        Destroy(gameObject);
    }

    protected virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition);
    }
}
