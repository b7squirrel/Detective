using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWeapon : WeaponBase
{
    [SerializeField] GameObject bomb;
    [SerializeField] float verticalVelocity;
    [SerializeField] bool isClean;
    [SerializeField] AudioClip shootSFX;

    Vector3[] targetDir = new Vector3[6]; //폭탄을 던질 지점
    [SerializeField] GameObject dot;
    Vector3 enemyDir;

    #region Attack
    protected override void Attack()
    {
        List<Vector2> _enemyPos = EnemyFinder.instance.GetEnemies(1);
        if (_enemyPos.Count == 0) { return; }

        Vector3 axisVec = Vector3.forward;
        enemyDir = (_enemyPos[0] - (Vector2)transform.position);
        Debug.DrawLine(transform.position, _enemyPos[0], Color.red);

        float _degree = 360 / weaponStats.numberOfAttacks;
        for (int i = 0; i < weaponStats.numberOfAttacks - 1; i++)
        {
            targetDir[i] = Quaternion.AngleAxis((float)(_degree * (i + 1)), axisVec) * enemyDir + transform.position;
            Debug.DrawLine(transform.position, targetDir[i], Color.yellow);
        }

        GenProjectile(_enemyPos[0]);

        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks - 1; i++)
        {
            yield return new WaitForSeconds(.2f);
            GenProjectile(targetDir[i]);
        }
    }
    #endregion

    void GenProjectile(Vector3 targetVec)
    {
        GameObject bombObject = GameManager.instance.poolManager.GetMisc(bomb);
        bombObject.transform.position = transform.position;

        ProjectileBase projectileBase = bombObject.GetComponent<ProjectileBase>();
        projectileBase.Damage = GetDamage(); // projectile마다 각각의 critical damage나 knock back 확률을 가지도록
        projectileBase.KnockBackChance = GetKnockBackChance();
        projectileBase.IsCriticalDamageProj = isCriticalDamage;

        BombProjectile proj = bombObject.GetComponent<BombProjectile>();
        proj.Init(targetVec, weaponStats);
        ProjectileHeight projHeight = bombObject.GetComponent<ProjectileHeight>();
        projHeight.Initialize(verticalVelocity);

        SoundManager.instance.Play(shootSFX);
    }
}
