using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : ProjectileBase
{
    public Vector2 GroundVelocity{get; private set;}
    float sizeOfArea;
    [SerializeField] LayerMask target; // 조준해서 던지는 것은 enemy지만 터지면 enemy, prop 둘 다 공격
    [SerializeField] GameObject hitEffect; // 폭발 이펙트
    [SerializeField] AudioClip hitSFX;

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
        // GenerateHitEffect();
        SoundManager.instance.Play(hitSFX);
        CastDamage();

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
                PostMessage(Damage, enmey.transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, transform.position, hitEffect);
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
    }
    void GenerateHitEffect()
    {
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        effect.transform.localScale *= sizeOfArea;
    }
}
