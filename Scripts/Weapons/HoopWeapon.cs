using System.Collections.Generic;
using UnityEngine;

// projectile 체력은 모두가 공유한다. 체력이 0이 되면 사라진다.
// timeToAttack으로 쿨타임이 돌면 다시 재생된다.
// sizeOfArea를 체력으로 사용하자
public class HoopWeapon : WeaponBase
{
    [SerializeField] GameObject hoopProjectile;
    [SerializeField] List<Transform> projectiles;
    float projectileHealth;
    bool isProjectileActive;
    float duration;

    protected override void Update()
    {
        base.Update();
        transform.Rotate(Vector3.forward * weaponStats.projectileSpeed * Time.deltaTime);

        // duration -= Time.deltaTime;
        // if (duration < 0 && isProjectileActive)
        // {
        //     DestroyProjectiles();
        //     // duration은 projectiles가 생성되면서 초기화 된다
        // }


        // 업그레이드 되면 프로젝타일을 중단시키고 다시 시작해서 갯수를 weaponStats.numberOfAttacks에 맞춰줌
        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        if (numberOfProjectilesToGen == 0)
            return;
        if (isProjectileActive == false) // 프로젝타일이 활성화 되어 있을 때만 Destroy 실행
            return;

        timer = 0; // 곧바로 업그레이드 된 갯수로 업데이트 하기위해
        DestroyProjectiles();
    }

    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
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
            Transform hoopObject = Instantiate(hoopProjectile, transform.position, Quaternion.identity).transform;
            hoopObject.parent = transform;
            projectiles.Add(hoopObject);
        }

        //배치, stat 리셋
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);

            projectiles[i].localPosition = Vector3.zero;
            projectiles[i].localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * i / weaponStats.numberOfAttacks;
            projectiles[i].Rotate(rotVec);
            projectiles[i].Translate(projectiles[i].up * 4.5f, Space.World);
            // Debug.Log("Rot Vec = " +rotVec);

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();
        }
        isProjectileActive = true;
        duration = 5f;
    }

    public void TakeDamageProjectile()
    {
        projectileHealth--;
        if(projectileHealth < 0)
        {
            DestroyProjectiles();
        }
    }
    void DestroyProjectiles()
    {
        foreach(Transform proj in projectiles)
        {
            proj.gameObject.SetActive(false);
        }
        
        isProjectileActive = false;
        timer = weaponStats.timeToAttack;
        projectileHealth = weaponStats.sizeOfArea;
    }
}
