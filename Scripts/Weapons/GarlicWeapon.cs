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

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                Debug.Log("hit effect name = " + hitEffect.name);
                enemy.TakeDamage(damage, knockback, item.transform.position, hitEffect);
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
