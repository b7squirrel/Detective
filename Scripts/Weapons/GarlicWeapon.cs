using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    ParticleSystem note;
    float effectRadius;
    [SerializeField] float[] effectArea = new float[4];

    protected override void Awake()
    {
        base.Awake();
        note = GetComponentInChildren<ParticleSystem>();
        
    }
    protected override void Attack()
    {
        base.Attack();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, effectArea[(int)weaponStats.sizeOfArea]);

        effectRadius = weaponStats.sizeOfArea;

        note.GetComponent<Animator>().SetTrigger((weaponStats.sizeOfArea).ToString());
        note.Play();

        ApplyDamage(colliders);
    }

    private void ApplyDamage(Collider2D[] colliders)
    {
        foreach (var item in colliders)
        {
            Idamageable enemy = item.transform.GetComponent<Idamageable>();

            if (enemy != null)
            {
                PostMessage(damage, item.transform.position);

                Vector2 enemyDir = item.transform.position - transform.position;
                Vector2 offsetDir = -(enemyDir.normalized);
                Vector2 hitPoint = (Vector2)item.transform.position + (offsetDir * 2f); // 대략 적 콜라이더의 반정도

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enemy.TakeDamage(damage, knockback, hitPoint, hitEffect);
            }
        }
    }

    void SetNoteParticle()
    {
        
    }

    protected override void FlipWeaponTools()
    {
        // Debug.Log("Garlic");
    }


}
