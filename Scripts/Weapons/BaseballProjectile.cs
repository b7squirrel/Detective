using UnityEngine;

public class BaseballProjectile : ProjectileBase
{
    [SerializeField] AudioClip hitSound;
    
    Animator anim;
    TrailRenderer trailRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // 다시 활성화 될 때 트레일이 이상하게 시작되는 문제 해결
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
        
        // 적에게 충돌
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            
            PostMessage(Damage, other.transform.position);
            TriggerHitEffects();
            DeactivateBall(); // 즉시 비활성화
        }
        // 벽이나 소품에 충돌
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
            
            TriggerHitEffects();
            DeactivateBall(); // 즉시 비활성화
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        
        if (other.gameObject.CompareTag("Props"))
        {
            other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            
            TriggerHitEffects();
            DeactivateBall(); // 즉시 비활성화
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
        // 오브젝트 풀링을 위해 비활성화
        TimeToLive = 3f; // 리셋
        transform.localScale = new Vector3(1, 1, 1); // 리셋
        gameObject.SetActive(false);
    }

    // ProjectileBase의 CastDamage를 비활성화
    // (OnCollisionEnter2D로만 데미지 처리)
    protected override void CastDamage()
    {
        // do nothing - 충돌로만 데미지 처리
    }
}