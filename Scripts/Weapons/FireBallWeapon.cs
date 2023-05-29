using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    protected override void Attack()
    {
        List<Vector2> closestEnemyPosition = FindTarget(1);
        if (closestEnemyPosition[0] == Vector2.zero)
        {
            Debug.Log("No ememies");
            return;
        }

        // StartCoroutine(AttackCo());
        AttackCo();
    }

    void AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            SoundManager.instance.Play(shoot);
            GameObject effect = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            effect.transform.position = EffectPoint.position;
            
            GameObject fireBall = GameManager.instance.poolManager.GetMisc(weapon);
            float index = 0f;
            if(i==0) 
            {
                index = -15f;
            }
            else if(i==1)
            {
                index = 0;
            }
            else if(i ==2)
            {
                index = 15f;
            }
            Vector3 direction = Quaternion.AngleAxis(index, Vector3.forward) * dir;
            // GameObject fireBall = Instantiate(weapon, transform.position, Quaternion.identity);
            fireBall.transform.position = transform.position;
            fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);

            ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
            projectile.Direction = direction;
            projectile.Speed = weaponStats.projectileSpeed;

            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();

            if (isSynergyWeaponActivated)
            {
                AnimShootExtra();
                SoundManager.instance.Play(shoot);
                // GameObject effectEx = GameManager.instance.poolManager.GetMisc(muzzleFlash);
                // effectEx.transform.position = EffectPointExtra.position;

                GameObject fireBallEx = GameManager.instance.poolManager.GetMisc(weapon);
                Vector3 directionExtra = Quaternion.AngleAxis(index, Vector3.forward) * dirExtra;
                // GameObject fireBall = Instantiate(weapon, transform.position, Quaternion.identity);
                fireBallEx.transform.position = transform.position;
                fireBallEx.transform.rotation = Quaternion.FromToRotation(Vector2.up, directionExtra);

                ProjectileBase projectileEx = fireBallEx.GetComponent<ProjectileBase>();
                projectileEx.Direction = directionExtra;
                projectileEx.Speed = weaponStats.projectileSpeed;

                projectileEx.Damage = GetDamage();
                projectileEx.KnockBackChance = GetKnockBackChance();
            }

            // yield return new WaitForSeconds(.3f);
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this);
    }
}
