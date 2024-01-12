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
        for (int i = 0; i < colliders.Length; i++)
        {
            Idamageable enemy = colliders[i].transform.GetComponent<Idamageable>();

            if (enemy != null)
            {
                PostMessage(damage, colliders[i].transform.position);

                Vector2 enemyDir = colliders[i].transform.position - transform.position;
                Vector2 offsetDir = -(enemyDir.normalized);
                Vector2 hitPoint = (Vector2)colliders[i].transform.position + (offsetDir * 2f); // 대략 적 콜라이더의 반정도

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enemy.TakeDamage(damage, knockback, knockbackSpeedFactor, hitPoint, hitEffect);
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
