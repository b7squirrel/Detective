using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : ProjectileBase
{
    Vector2 targetPos;
    float sizeOfArea;
    [SerializeField] LayerMask target; // 조준해서 던지는 것은 enemy지만 터지면 enemy, prop 둘 다 공격

    protected override void Update()
    {
        ApplyMovement();
    }
    protected override void ApplyMovement()
    {
        transform.position =
            Vector2.MoveTowards(transform.position, targetPos, Speed * Time.deltaTime);
        float distance = Vector2.Distance(transform.position, targetPos);

        if (distance < .1f)
        {
            CastDamage();
            Destroy(gameObject);
        }
    }
    protected override void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position,sizeOfArea, target);
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
    public void SetTargetPos(Vector2 target)
    {
        targetPos = target;
    }
    public void SetStats(WeaponStats weaponStats, int damage)
    {
        Speed = weaponStats.projectileSpeed;
        sizeOfArea = weaponStats.sizeOfArea;
        Damage = damage;
        Debug.Log("Speed = " + Speed);
        Debug.Log("Size = " + sizeOfArea);
        Debug.Log("Damage = " + Damage);
    }
}
