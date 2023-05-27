using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisWeapon : WeaponBase
{
    [SerializeField] GameObject weaponTennisBall;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    protected override void Attack()
    {
          List<Vector2> closestEnemyPosition = FindTarget(1);
        if (closestEnemyPosition[0] == Vector2.zero)
        {
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
            Transform muzzleEffect =
                GameManager.instance.poolManager.GetMisc(muzzleFlash).transform;
            muzzleEffect.transform.position = ShootPoint.position;
            GameObject tennisBall = GameManager.instance.poolManager.GetMisc(weaponTennisBall);
            tennisBall.transform.position = ShootPoint.position;

            ProjectileBase projectile = tennisBall.GetComponent<ProjectileBase>();
            projectile.Direction = dir;
            projectile.Speed = weaponStats.projectileSpeed;
            projectile.Direction = direction;
            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();

            Debug.Log("Projectile Speed = " + weaponStats.projectileSpeed);

            yield return new WaitForSeconds(.3f);
        }
    }

    protected override void FlipWeaponTools()
    {
        if(weaponTools == null)
        return;

        if(flip)
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}
