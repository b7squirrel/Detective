using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWeapon : WeaponBase
{
    [SerializeField] GameObject bomb;
    [SerializeField] float verticalVelocity;
    [SerializeField] float duration = .4f;
    Vector2 target; //폭탄을 던질 지점
    [SerializeField] bool isClean;
    [SerializeField] AudioClip shootSFX;

    #region Attack
    protected override void Attack()
    {
        FindLandingPositions(); // number Of Attacks 만큼 target을 잡음

        if (isClean) // 화면 상에 적이 없으면
        {
            isClean = false;
            return;
        }

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            // Vector3 axisVec = new Vector3(0, 0, 90f * (float)i);
            Vector3 axisVec = new Vector3(0,0,1).normalized;
            Vector3 targetDir = Quaternion.AngleAxis(120 * (float)i, axisVec) * target;

            SoundManager.instance.Play(shootSFX);
            GameObject bombObject = Instantiate(bomb, transform.position, Quaternion.identity);

            BombProjectile proj = bombObject.GetComponent<BombProjectile>();
            proj.Init(targetDir, weaponStats, GetDamage());

            ProjectileHeight projHeight = bombObject.GetComponent<ProjectileHeight>();
            projHeight.Initialize(verticalVelocity);
        }

    }
    #endregion

    #region Find Landing Position
    void FindLandingPositions()
    {
        Vector2 center = Player.instance.transform.position;

        Collider2D[] enemies =
                Physics2D.OverlapAreaAll(center - new Vector2(halfWidth * .8f, halfHeight * .8f),
                                            center + new Vector2(halfWidth * .8f, halfHeight * .8f), enemy);

        if (enemies.Length == 0)
        {
            isClean = true;
            return;
        }

        int index = Random.Range(0, enemies.Length);
        target = enemies[index].transform.position;
    }
    #endregion
}
