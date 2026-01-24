using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection = 3;
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
    
    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        
        if (other.contacts.Length > 0)
        {
            Vector2 normalVector = other.contacts[0].normal;
            HandleCollisionWithNormal(other, normalVector, hitEffect);
        }
    }
    
    private void HandleCollisionWithNormal(Collision2D other, Vector2 normalVector, GameObject hitEffect)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // 데미지 처리
            other.gameObject.GetComponent<Idamageable>().TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            PostMessage(Damage, other.transform.position);

            // ✨ 데미지 기록 추가
            if (!string.IsNullOrEmpty(WeaponName))
            {
                DamageTracker.instance.RecordDamage(WeaponName, Damage);
            }

            // 반사 및 효과
            HandleReflection(normalVector, rb);
            TriggerHitEffects();

            // deflection 체크
            if (ShouldDeactivate(ref deflection))
            {
                DeactivateBall();
            }
        }
        else if (other.gameObject.CompareTag("MainCamera") || 
                 other.gameObject.CompareTag("Wall") || 
                 other.gameObject.CompareTag("Props"))
        {
            // 소품이면 데미지 처리
            if (other.gameObject.CompareTag("Props"))
            {
                other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                    Damage, 
                    KnockBackChance, 
                    KnockBackSpeedFactor, 
                    transform.position, 
                    hitEffect);
            }
            
            // 반사 및 효과
            HandleReflection(normalVector, rb);
            TriggerHitEffects();
            
            // deflection 체크
            if (ShouldDeactivate(ref deflection))
            {
                DeactivateBall();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        
        if (other.gameObject.CompareTag("Props"))
        {
            // 데미지 처리
            other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            
            // 트리거는 단순 반대 방향 반사
            Direction = -Direction.normalized;
            rb.velocity = Vector2.zero;
            TriggerHitEffects();
            
            // deflection 체크
            if (ShouldDeactivate(ref deflection))
            {
                DeactivateBall();
            }
        }
    }
    
    private void TriggerHitEffects()
    {
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }
        
        if (hitSound != null)
        {
            SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .034f);
        }
    }
    
    private void DeactivateBall()
    {
        deflection = 3; // 초기값으로 리셋
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
    
    protected override void CastDamage()
    {
        // do nothing in tennis projectile
    }
}