using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// sizeOfArea를 지속 시간으로 사용하자
/// </summary>
public class BeamWeapon : WeaponBase
{
    [SerializeField] GameObject laserProjectile;
    [SerializeField] List<Transform> projectiles;
    bool isProjectileActive;
    float duration; // projectile 지속 시간
    float normalDuration = .5f;
    float synergyDuration = 1f;

    [SerializeField] GameObject muzzleFlash;
    GameObject muzzle; // muzzleFlash를 생성해서 담아두는 곳
    [SerializeField] AudioClip laserShoot;
    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
    }

    protected override void Update()
    {
        base.Update();

        // 회전판 회전
        transform.Rotate(Vector3.forward * weaponStats.projectileSpeed * Time.deltaTime);

        // 레이져가 비활성화 된 상태에서만 쿨타임이 돌아감
        if (isProjectileActive == false)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                Attack(); // attack 파라미터를 얻어옴. 
                timer = weaponStats.timeToAttack;
            }
            return;
        }

        // 레이져가 활성화된 상태라면 duration이 돌아감
        if (duration > 0)
        {
            duration -= Time.deltaTime;
        }
        else if (duration <= 0)
        {
            DestroyProjectiles();
        }

        // 업그레이드 되면 프로젝타일을 중단시키고 다시 시작해서 갯수를 weaponStats.numberOfAttacks에 맞춰줌
        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        if (numberOfProjectilesToGen == 0)
            return;
        if (isProjectileActive == false) // 프로젝타일이 활성화 되어 있을 때만 Destroy 실행
            return;

        timer = 0; // 곧바로 업그레이드 된 갯수로 업데이트 하기위해
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
            //GameObject laserObject = GameManager.instance.poolManager.GetMisc(laserProjectile);
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
            projectiles[i].Rotate(rotVec);
            // projectiles[i].Translate(projectiles[i].right * 30f, Space.World); // laser projectile 길이는 60

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = damage;
            projectile.KnockBackChance = knockback;;
            projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
            projectile.IsCriticalDamageProj = isCriticalDamage;
        }

        // muzzle flash
        if(muzzle == null)
        {
            muzzle = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            if(muzzle != null)
            {
                muzzle.transform.parent = ShootPoint; // 편의상 적당히 child를 따라다닐 오브젝트에 페어런트
                muzzle.transform.position = ShootPoint.position;
            }
        }
        muzzle.gameObject.SetActive(true);

        isProjectileActive = true;

        duration = isSynergyWeaponActivated ? synergyDuration : normalDuration;
        Debug.Log("Duration = " + duration);

        // sound
        SoundManager.instance.Play(laserShoot);
    }
    void DestroyProjectiles()
    {
        foreach (Transform proj in projectiles)
        {
            proj.gameObject.SetActive(false);
        }

        isProjectileActive = false;
        timer = weaponStats.timeToAttack;

        muzzle.gameObject.SetActive(false);
    }
    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].GetComponent<BeamProjectile>().SetAnimToSynergy();
        }
        duration = synergyDuration; // 레이져 애니메이션 길이
    }
}