using UnityEngine;

public class BaseballProjectile : ProjectileBase
{
    [SerializeField] AudioClip hitSound;
    [SerializeField] GameObject deadProjectile;
    [SerializeField] float deadVerticalSpeed;
    [SerializeField] SpriteRenderer deadSprite;
    
    Animator anim;
    TrailRenderer trailRenderer;
    
    bool hasHitOnce = false;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }
    
    private void OnEnable()
    {
        hasHitOnce = false;
    }
    
    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    
    // ★ Trigger만 사용
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitOnce) return;
        
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;

        // ★ Normal Vector 계산
        Vector2 hitPosition = transform.position;
        Vector2 otherPosition = other.transform.position;
        Vector2 normalVector = (hitPosition - otherPosition).normalized;

        GenDeadProjectile(normalVector * Speed, deadVerticalSpeed, deadSprite.sprite);
        
        // Enemy에 충돌
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Idamageable>()?.TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            PostMessage(Damage, other.transform.position);
            
            TriggerHitEffects();
            hasHitOnce = true;
            gameObject.SetActive(false);
        }
        // Props에 충돌
        else if (other.CompareTag("Props"))
        {
            other.GetComponent<Idamageable>()?.TakeDamage(
                Damage, 
                KnockBackChance, 
                KnockBackSpeedFactor, 
                transform.position, 
                hitEffect);
            
            TriggerHitEffects();
            hasHitOnce = true;
            gameObject.SetActive(false);
        }
        // 벽이나 경계에 충돌
        else if (other.CompareTag("Wall") || other.CompareTag("MainCamera"))
        {
            TriggerHitEffects();
            hasHitOnce = true;
            gameObject.SetActive(false);
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
    
    protected override void CastDamage()
    {
        // do nothing
    }

    void GenDeadProjectile(Vector2 groundVel, float verticalVelocity, Sprite sprite)
    {
        GameObject deadPr = GameManager.instance.poolManager.GetMisc(deadProjectile);

        // ★ 위치 설정 추가!
        deadPr.transform.position = transform.position;

        deadPr.GetComponent<ShadowHeightDeadProjectile>().Initialize(groundVel, verticalVelocity, sprite);
    }
}