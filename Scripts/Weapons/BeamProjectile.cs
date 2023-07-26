using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 재활용해서 계속 사용하는 projectile의 경우 active여부를 weapon단계에서 관리하도록 한다
public class BeamProjectile : ProjectileBase
{
    Animator[] anim;

    void OnEnable()
    {
        if(anim == null) anim = GetComponentsInChildren<Animator>();

        foreach (var item in anim)
        {
            item.Play("LV1");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CastDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        CastDamage(other);
    }

    void CastDamage(Collider2D other)
    {
        if (Time.frameCount % 3 != 0) // 3프레임 간격으로 데미지를 입힘
            return;

        if (other.GetComponent<Idamageable>() == null)
            return;

        Transform enmey = other.GetComponent<Transform>();
        PostMessage(Damage, enmey.position);
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        hitEffect.transform.position = enmey.position;
        enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, other.ClosestPoint(transform.position), hitEffect);
        hitDetected = true;
    }

    protected override void DieProjectile()
    {
        // do nothing
    }
    protected override void Update()
    {
        // do nothing
    }
}