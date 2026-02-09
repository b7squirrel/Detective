using System.Collections.Generic;
using UnityEngine;

public class ZapWeapon : WeaponBase
{
    [SerializeField] GameObject zapProjectile;
    [SerializeField] List<Transform> projectiles;
    bool isProjectileActive;
    
    [Header("Zap Settings")]
    [SerializeField] float duration; // projectile 지속 시간
    [SerializeField] float normalDuration = 2f;
    [SerializeField] float synergyDuration = 3f;

    [SerializeField] GameObject muzzleFlash;
    GameObject muzzle;
    [SerializeField] AudioClip zapShoot;
    public AudioClip ZapShootSound => zapShoot; // ← 추가: ZapProjectile이 접근할 수 있도록

    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
    }

    protected override void Update()
    {
        base.Update();

        // 레이저가 비활성화 된 상태에서만 쿨타임이 돌아감
        if (isProjectileActive == false)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                Attack();
                timer = weaponStats.timeToAttack;
            }
            return;
        }

        // 레이저가 활성화된 상태라면 duration이 돌아감
        if (duration > 0)
        {
            duration -= Time.deltaTime;
        }
        else if (duration <= 0)
        {
            DestroyProjectiles();
        }

        // 업그레이드 되면 프로젝타일을 중단시키고 다시 시작
        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        if (numberOfProjectilesToGen == 0)
            return;
        if (isProjectileActive == false)
            return;

        timer = 0;
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

        // 🔍 디버그 로그 추가
        Logger.Log($"[ZapWeapon] numberOfAttacks: {weaponStats.numberOfAttacks}, Current Count: {projectiles.Count}, To Generate: {numberOfProjectilesToGen}");

        // 생성
        for (int i = 0; i < numberOfProjectilesToGen; i++)
        {
            Transform zapObject = Instantiate(zapProjectile, transform.position, Quaternion.identity).transform;
            zapObject.parent = transform;
            projectiles.Add(zapObject);
        }

        // 시너지 설정
        if (isSynergyWeaponActivated)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].GetComponent<ZapProjectile>().SetAnimToSynergy();
            }
        }

        // 활성화 및 stat 설정
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = damage;
            projectile.KnockBackChance = knockback;
            projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
            projectile.IsCriticalDamageProj = isCriticalDamage;
            projectile.WeaponName = weaponData.DisplayName;
        }

        // 눈 반짝
        AnimShoot();

        // muzzle flash
        if (muzzle == null)
        {
            muzzle = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            if (muzzle != null)
            {
                muzzle.transform.parent = ShootPoint;
                muzzle.transform.position = ShootPoint.position;
            }
        }
        if (muzzle != null)
        {
            muzzle.gameObject.SetActive(true);
        }

        isProjectileActive = true;
        duration = isSynergyWeaponActivated ? synergyDuration : normalDuration;

        // sound
        // SoundManager.instance.Play(zapShoot);

        Logger.Log($"[ZapWeapon] Total projectiles after generation: {projectiles.Count}"); // 🔍
    }

    void DestroyProjectiles()
    {
        foreach (Transform proj in projectiles)
        {
            proj.gameObject.SetActive(false);
        }

        isProjectileActive = false;
        timer = weaponStats.timeToAttack;

        if (muzzle != null)
        {
            muzzle.gameObject.SetActive(false);
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        duration = synergyDuration;
    }
}