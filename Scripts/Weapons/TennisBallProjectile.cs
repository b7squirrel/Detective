using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection;
    [SerializeField] AudioClip hitSound;
    Rigidbody2D rb;
    Animator anim;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
    }
    protected override void HitObject()
    {
        deflection--;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, transform.position);
            PostMessage(Damage, other.transform.position);

            // 입사벡터
            Vector2 incomingVector = Direction;
            incomingVector = incomingVector.normalized;

            // 접선벡터
            Vector2 normalVector = other.contacts[0].normal;

            // 반사벡터
            Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector);
            deflectionVector = deflectionVector.normalized;

            Direction = deflectionVector;
            rb.velocity = Vector2.zero;

            anim.SetTrigger("Hit");
            SoundManager.instance.Play(hitSound);
        }

        if (other.gameObject.CompareTag("MainCamera"))
        {
            // 입사벡터
            Vector2 incomingVector = Direction;
            incomingVector = incomingVector.normalized;

            // 접선벡터
            Vector2 normalVector = other.contacts[0].normal;

            // 반사벡터
            Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector);
            deflectionVector = deflectionVector.normalized;

            Direction = deflectionVector;
            rb.velocity = Vector2.zero;

            anim.SetTrigger("Hit");
            SoundManager.instance.Play(hitSound);
        }

        if (other.gameObject.CompareTag("Wall"))
        {
            // 입사벡터
            Vector2 incomingVector = Direction;
            incomingVector = incomingVector.normalized;

            // 접선벡터
            Vector2 normalVector = other.contacts[0].normal;

            // 반사벡터
            Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector);
            deflectionVector = deflectionVector.normalized;

            Direction = deflectionVector;
            rb.velocity = Vector2.zero;

            anim.SetTrigger("Hit");
            SoundManager.instance.Play(hitSound);
        }
    }
    
    protected override void CastDamage()
    {
        // do nothing in tennis projectile
    }
}
