using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection;
    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    
    // private void OnEnable() {
    //     rb.AddForce(Direction * Speed, ForceMode2D.Impulse);
    // }

    protected override void HitObject()
    {
        deflection--;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        // if(other.gameObject.CompareTag("Enemy"))
        // {
        //     // Destroy(other.gameObject);
            // other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage);
            // PostMessage(Damage, other.transform.position);
        // }


        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage);
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
            rb.AddForce(Direction * Speed, ForceMode2D.Impulse);
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
            rb.AddForce(Direction * Speed, ForceMode2D.Impulse);
        }
    }
    protected override void ApplyMovement()
    {
        if(rb.velocity.magnitude > Speed)
        {
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, Speed);
        }
    }
    protected override void CastDamage()
    {
        // do nothing in tennis projectile
    }
}
