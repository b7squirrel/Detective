using UnityEngine;

public class BowlingProjectile : ProjectileBase
{
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private AudioClip hitSound; //뽑뽑뽑 하는 느낌으로 적들 위로 지나가기
    [SerializeField] private AudioClip reflectSound; // 쾅 하는 느낌으로 벽에 반사
    [SerializeField] private AudioClip bowlingStrikeSouind; // 볼링공이 사라질 때
    
    private Rigidbody2D rb;
    private Animator anim;
    private TrailRenderer trailRenderer;
    private int currentReflections = 0;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }
    
    private void OnEnable()
    {
        currentReflections = 0;
    }
    
    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        
        // 적과 프랍 - 통과하며 데미지
        if (other.CompareTag("Enemy") || other.CompareTag("Props"))
        {
            HandleDamage(other, hitEffect);
        }
        // 벽과 카메라 - 반사
        else if (other.CompareTag("MainCamera") || other.CompareTag("Wall"))
        {
            HandleWallReflection(other);
        }
    }
    
    private void HandleDamage(Collider2D target, GameObject hitEffect)
    {
        Idamageable damageable = target.GetComponent<Idamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(
                Damage,
                KnockBackChance,
                KnockBackSpeedFactor,
                transform.position,
                hitEffect);
            
            // Enemy에만 데미지 메시지 표시
            if (target.CompareTag("Enemy"))
            {
                PostMessage(Damage, target.transform.position);
            }
            
            // 데미지 기록
            if (!string.IsNullOrEmpty(WeaponName))
            {
                DamageTracker.instance.RecordDamage(WeaponName, Damage);
            }
            
            TriggerHitEffects(hitSound);
        }
    }
    
    private void HandleWallReflection(Collider2D wall)
    {
        // 🎯 ClosestPoint를 사용해 법선 벡터 계산
        Vector2 normalVector = GetWallNormal(wall);
        
        // ✨ ProjectileBase의 HandleReflection 사용
        HandleReflection(normalVector, rb);
        TriggerHitEffects(reflectSound);
        
        // 반사 횟수 체크
        currentReflections++;
        if (currentReflections >= maxReflections)
        {
            DeactivateBall();
        }
    }
    
    // 🔍 벽의 가장 가까운 지점으로부터 법선 벡터 계산
    private Vector2 GetWallNormal(Collider2D wall)
    {
        // 벽 콜라이더의 가장 가까운 지점 찾기
        Vector2 closestPoint = wall.ClosestPoint(transform.position);
        
        // 볼링공 중심에서 가장 가까운 지점으로의 벡터 = 법선 벡터
        Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;
        
        return normal;
    }
    
    private void TriggerHitEffects(AudioClip sound)
    {
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }
        
        if (sound != null)
        {
            SoundManager.instance.PlaySoundWith(sound, 1f, false, 0.034f);
        }
    }
    
    private void DeactivateBall()
    {
        currentReflections = 0;
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
    
    protected override void CastDamage()
    {
        // 트리거로만 처리하므로 비움
    }
}