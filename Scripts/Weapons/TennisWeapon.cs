using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisWeapon : WeaponBase
{
    [SerializeField] GameObject weaponTennisBall;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] Transform muzzleFlash;

    protected override void Attack()
    {
          Vector2 closestEnemyPosition = FindTarget();
        if (closestEnemyPosition == Vector2.zero)
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
            Transform muzzleEffect =
                Instantiate(muzzleFlash, EffectPoint.position, Quaternion.identity);
            GameObject tennisBall = Instantiate(weaponTennisBall);
            tennisBall.transform.position = ShootPoint.position;

            ProjectileBase projectile = tennisBall.GetComponent<ProjectileBase>();
            projectile.Direction = dir;
            projectile.GetComponent<Rigidbody2D>().AddForce(projectile.Direction * weaponStats.projectileSpeed, ForceMode2D.Impulse);
            projectile.Damage = GetDamage();

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
