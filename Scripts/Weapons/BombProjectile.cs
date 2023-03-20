using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : ProjectileBase
{
    public Vector2 GroundVelocity{get; private set;}
    float sizeOfArea;
    [SerializeField] LayerMask target; // 조준해서 던지는 것은 enemy지만 터지면 enemy, prop 둘 다 공격

    protected override void Update()
    {
        ApplyMovement();
    }
    protected override void ApplyMovement()
    {
        transform.position += (Vector3)GroundVelocity * Time.deltaTime;
    }
    public void Explode()
    {
        CastDamage();
    }
    protected override void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, sizeOfArea, target);
        foreach (var item in hit)
        {
            Transform enmey = item.GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                PostMessage(Damage, enmey.transform.position);
                enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, transform.position);
                hitDetected = true;
            }
        }
    }
    public void Init(Vector2 target, WeaponStats weaponStats, int damage)
    {
        Speed = weaponStats.projectileSpeed;
        sizeOfArea = weaponStats.sizeOfArea;
        Damage = damage;

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        GroundVelocity = dir * Speed;
    }
}
