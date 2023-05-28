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

        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            SoundManager.instance.Play(shoot);
            GameObject effect = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            effect.transform.position = EffectPoint.position;
            
            GameObject fireBall = GameManager.instance.poolManager.GetMisc(weapon);
            // GameObject fireBall = Instantiate(weapon, transform.position, Quaternion.identity);
            fireBall.transform.position = transform.position;
            fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, dir);

            ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
            projectile.Direction = dir;
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
                // GameObject fireBall = Instantiate(weapon, transform.position, Quaternion.identity);
                fireBallEx.transform.position = transform.position;
                fireBallEx.transform.rotation = Quaternion.FromToRotation(Vector2.up, dirExtra);

                ProjectileBase projectileEx = fireBallEx.GetComponent<ProjectileBase>();
                projectileEx.Direction = dirExtra;
                projectileEx.Speed = weaponStats.projectileSpeed;

                projectileEx.Damage = GetDamage();
                projectileEx.KnockBackChance = GetKnockBackChance();
            }

            yield return new WaitForSeconds(.3f);
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this);
    }
}
