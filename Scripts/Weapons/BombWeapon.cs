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

    Vector3[] targetDir = new Vector3[2];
    [SerializeField] GameObject dot;
    Vector3 enemyDir;

    #region Attack
    protected override void Attack()
    {
        FindLandingPositions(); // number Of Attacks 만큼 target을 잡음

        if (isClean) // 화면 상에 적이 없으면
        {
            isClean = false;
            return;
        }
        Vector3 axisVec = Vector3.forward;
        enemyDir = (target - (Vector2)transform.position);
        Debug.DrawLine(transform.position, target, Color.red);

        for (int i = 0; i < 2; i++)
        {
            targetDir[i] = Quaternion.AngleAxis((float)(120 * (i + 1)), axisVec) * enemyDir + transform.position;
            // Debug.DrawLine(transform.position, targetDir[i], Color.yellow);
            Debug.DrawLine(transform.position, targetDir[i], Color.yellow);
        }

        GenProjectile(target);
        SoundManager.instance.Play(shootSFX);
        for (int i = 0; i < weaponStats.numberOfAttacks -1; i++)
        {
            // targetDir[i] = Quaternion.AngleAxis((float)(120 * (i + 1)), axisVec) * enemyDir + transform.position;
            GenProjectile(targetDir[i]);
        }
    }
    #endregion

    void GenProjectile(Vector3 targetVec)
    {
        GameObject bombObject = Instantiate(bomb, transform.position, Quaternion.identity);
        BombProjectile proj = bombObject.GetComponent<BombProjectile>();
        proj.Init(targetVec, weaponStats, GetDamage());
        ProjectileHeight projHeight = bombObject.GetComponent<ProjectileHeight>();
        projHeight.Initialize(verticalVelocity);
    }

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

        isClean = false;
        int index = Random.Range(0, enemies.Length);
        target = enemies[index].transform.position;
        // Instantiate(dot, target, Quaternion.identity);
    }
    #endregion
}
