using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection;
    [SerializeField] AudioClip hitSound;
    Rigidbody2D rb;
    Animator anim;
    TrailRenderer trailRenderer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        
    }
    // 다시 활성화 될 때 트레일이 이상하게 시작되는 문제를 해결하기 위해서
    private void OnDisable()
    {
        trailRenderer.Clear();
    }

    protected override void HitObject()
    {
        deflection--;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;

        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage, 
                                                                    KnockBackChance, 
                                                                    KnockBackSpeedFactor,
                                                                    transform.position, 
                                                                    hitEffect);
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

        if (other.gameObject.CompareTag("MainCamera") || other.gameObject.CompareTag("Wall"))
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

        if (other.gameObject.CompareTag("Props"))
        {
            other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, KnockBackSpeedFactor, transform.position, hitEffect);
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
