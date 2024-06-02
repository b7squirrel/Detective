using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection;
    [SerializeField] AudioClip hitSound;
    Rigidbody2D rb;
    Animator anim;
    TrailRenderer trailRenderer;

    private void Awake()
    {
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

        if (other.contacts.Length > 0)
        {
            Vector2 normalVector = other.contacts[0].normal;
            Debug.Log("Normal Vector: " + normalVector);
            HandleCollisionWithNormal(other, normalVector, hitEffect);
        }
        else
        {
            Debug.LogWarning("No contacts available in the collision.");
        }
    }

    private void HandleCollisionWithNormal(Collision2D other, Vector2 normalVector, GameObject hitEffect)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage,
                                                                    KnockBackChance,
                                                                    KnockBackSpeedFactor,
                                                                    transform.position,
                                                                    hitEffect);
            PostMessage(Damage, other.transform.position);

            ReflectDirection(normalVector);
            TriggerHitEffects();
        }
        else if (other.gameObject.CompareTag("MainCamera") || other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Props"))
        {
            if (other.gameObject.CompareTag("Props"))
            {
                other.gameObject.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, KnockBackSpeedFactor, transform.position, hitEffect);
            }

            ReflectDirection(normalVector);
            TriggerHitEffects();
        }
    }

    private void ReflectDirection(Vector2 normalVector)
    {
        Vector2 incomingVector = Direction.normalized;
        Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector).normalized;
        Direction = deflectionVector;
        rb.velocity = Vector2.zero;
    }

    private void TriggerHitEffects()
    {
        anim.SetTrigger("Hit");
        SoundManager.instance.Play(hitSound);
    }

    protected override void CastDamage()
    {
        // do nothing in tennis projectile
    }
}