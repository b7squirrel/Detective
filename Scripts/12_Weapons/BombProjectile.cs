using UnityEngine;

public class BombProjectile : ProjectileBase
{
    public Vector2 GroundVelocity{get; private set;}
    float sizeOfArea;
    [SerializeField] LayerMask target; // 조준해서 던지는 것은 enemy지만 터지면 enemy, prop 둘 다 공격
    [SerializeField] GameObject hitEffect; // 폭발 이펙트
    [SerializeField] GameObject starEffect; // 폭발 이펙트 - 별
    [SerializeField] AudioClip hitSFX;

    TrailRenderer trailRenderer;

    void OnEnable()
    {
        if(trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    protected override void Update()
    {
        ApplyMovement();
        // Debug.DrawLine(transform.position, Player.instance.transform.position, Color.red);
    }
    protected override void ApplyMovement()
    {
        transform.position += (Vector3)GroundVelocity * Time.deltaTime;
    }
    public void Explode()
    {
        GenerateHitEffect();
        SoundManager.instance.Play(hitSFX);
        CastDamage();

        trailRenderer.enabled = false;

        GetComponentInChildren<TrailRenderer>().enabled = false;
        gameObject.SetActive(false);
    }
    protected override void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, sizeOfArea, target);
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enmey = hit[i].GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                if (enmey.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enmey.transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage,
                                                             KnockBackChance,
                                                             KnockBackSpeedFactor,
                                                             transform.position,
                                                             hitEffect);

                // ✨ 데미지 기록 추가
                if (!string.IsNullOrEmpty(WeaponName))
                {
                    DamageTracker.instance.RecordDamage(WeaponName, Damage);
                }

                hitDetected = true;
            }
        }
    }
    public void Init(Vector2 target, WeaponStats weaponStats)
    {
        Speed = weaponStats.projectileSpeed;
        sizeOfArea = weaponStats.sizeOfArea;

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        GroundVelocity = dir * Speed;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
    }
    void GenerateHitEffect()
    {
        GameObject smoke = GameManager.instance.poolManager.GetMisc(hitEffect);
        smoke.transform.position = transform.position;
        GameObject stars = GameManager.instance.poolManager.GetMisc(starEffect);
        stars.transform.position = transform.position;

        smoke.transform.localScale = Vector2.one * sizeOfArea;
        stars.transform.localScale = Vector2.one * sizeOfArea;
    }
}
