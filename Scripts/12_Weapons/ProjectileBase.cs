using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [field : SerializeField] public Vector3 Direction { get; set; }
    [field : SerializeField] public float Speed {get; set;}
    protected bool hitDetected = false;
    public int Damage { get; set; } = 5;
    public float KnockBackChance {get; set;}
    [field: SerializeField] public float KnockBackSpeedFactor { get; set;}
    public bool IsCriticalDamageProj {get; set;}
    [field : SerializeField] public float TimeToLive {get; set;} = 3f;

    // ✨ 투사체를 발사한 무기 이름
    public string WeaponName { get; set; }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
            return;
        ApplyMovement();
        CastDamage();
        
        AttackCoolTimer();
    }

    protected virtual void AttackCoolTimer()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
        {
            DieProjectile();
        }
    }

    protected virtual void ApplyMovement()
    {
        transform.position += Speed * Time.deltaTime * Direction.normalized;
    }

    protected virtual void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enmey = hit[i].GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                if (enmey.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enmey.transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, KnockBackSpeedFactor, transform.position, hitEffect);
                hitDetected = true;

                // ✨ 무기 이름과 함께 데미지 기록
                if (!string.IsNullOrEmpty(WeaponName))
                {
                    DamageTracker.instance.RecordDamage(WeaponName, Damage);
                }

                break;
            }
        }
        if (hitDetected == true)
        {
            HitObject();
            hitDetected = false;
        }
    }

    protected virtual void HitObject()
    {
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }

    protected virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    protected virtual void DieProjectile()
    {
        TimeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }

    #region 반사 로직
    protected void HandleReflection(Vector2 normalVector, Rigidbody2D rb)
    {
        Vector2 incomingVector = Direction.normalized;
        Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector).normalized;
        Direction = deflectionVector;
        rb.velocity = Vector2.zero;
    }
    
    // 공통 메서드: deflection 감소 및 비활성화 체크
    protected bool ShouldDeactivate(ref int deflection)
    {
        deflection--;
        return deflection < 0;
    }
    #endregion
}
