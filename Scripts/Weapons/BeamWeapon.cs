using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// sizeOfArea를 지속 시간으로 사용하자
public class BeamWeapon : WeaponBase
{
    [SerializeField] GameObject laserProjectile;
    [SerializeField] List<Transform> projectiles;
    bool isProjectileActive;
    float duration; // projectile 지속 시간

    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
    }

    protected override void Update()
    {
        base.Update();
        transform.Rotate(Vector3.forward * weaponStats.projectileSpeed * Time.deltaTime);

        // 업그레이드 되면 프로젝타일을 중단시키고 다시 시작해서 갯수를 weaponStats.numberOfAttacks에 맞춰줌
        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        if (numberOfProjectilesToGen == 0)
            return;
        if (isProjectileActive == false) // 프로젝타일이 활성화 되어 있을 때만 Destroy 실행
            return;

        timer = 0; // 곧바로 업그레이드 된 갯수로 업데이트 하기위해
        Debug.Log("Destroy");
        DestroyProjectiles();
    }

    void GenProjectile()
    {
        if (isProjectileActive)
            return;

        // 초기화
        if (projectiles == null)
        {
            projectiles = new List<Transform>();
        }

        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;

        // 생성
        for (int i = 0; i < numberOfProjectilesToGen; i++)
        {
            Transform laserObject = Instantiate(laserProjectile, transform.position, Quaternion.identity).transform;
            laserObject.parent = transform;
            projectiles.Add(laserObject);
        }

        //배치, stat 리셋
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);

            projectiles[i].localPosition = Vector3.zero;
            projectiles[i].localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * i / weaponStats.numberOfAttacks;
            projectiles[i].Translate(projectiles[i].right * 30f, Space.World); // laser projectile 길이는 60
            projectiles[i].Rotate(rotVec);

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();
            // projectile.TimeToLive = weaponStats.sizeOfArea; // size of area를 time to live로 하기
        }
        isProjectileActive = true;
        duration = 5f;
    }
    void DestroyProjectiles()
        {
            foreach (Transform proj in projectiles)
            {
                proj.gameObject.SetActive(false);
            }

            isProjectileActive = false;
            timer = weaponStats.timeToAttack;
        }
}
