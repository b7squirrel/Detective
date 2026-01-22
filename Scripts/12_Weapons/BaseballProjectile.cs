using UnityEngine;

public class BaseballProjectile : ProjectileBase
{
    [SerializeField] AudioClip hitSound;
    [SerializeField] GameObject deadProjectile;
    [SerializeField] float deadVerticalSpeed;
    [SerializeField] SpriteRenderer deadSprite;
    
    [Header("Rotation Settings")]
    [SerializeField] bool rotateToDirection = true; // 회전 활성화 여부
    [SerializeField] float rotationOffset = 0f;     // 각도 보정값 (필요시)
    
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
    
    // ★ 회전 업데이트 추가
    protected override void Update()
    {
        base.Update(); // ProjectileBase의 Update 실행
        
        if (rotateToDirection && !hasHitOnce)
        {
            RotateToDirection();
        }
    }

    // ★ 방향으로 회전
    void RotateToDirection()
    {
        if (Direction != Vector3.zero && deadSprite != null)
        {
            float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;

            // ★ 스프라이트만 회전
            deadSprite.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitOnce) return;
        
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        
        // Normal Vector 계산
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
        deadPr.transform.position = transform.position;
        deadPr.GetComponent<ShadowHeightDeadProjectile>().Initialize(groundVel / 8f, verticalVelocity, sprite);
    }
}