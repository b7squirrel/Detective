using System.Collections.Generic;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // ⭐ 런타임에 결정되는 프로젝타일
    GameObject currentWeaponPrefab;

    public override void Init(WeaponStats stats, bool isLead)
{
    base.Init(stats, isLead);

    // ⭐ 디버그: InitialWeapon 상태 확인
    Logger.Log($"[FireBallWeapon] Init 시작 - InitialWeapon: {InitialWeapon}");

    if (InitialWeapon)
    {
        Item equippedItem = GetEssentialEquippedItem();

        // ⭐ 디버그: 반환된 아이템 확인
        Logger.Log($"[FireBallWeapon] GetEssentialEquippedItem 반환값: {(equippedItem == null ? "null" : equippedItem.Name)}");

        if (equippedItem != null)
        {
            // ⭐ 디버그: projectilePrefab 확인
            Logger.Log($"[FireBallWeapon] projectilePrefab: {(equippedItem.projectilePrefab == null ? "null" : equippedItem.projectilePrefab.name)}");
        }

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentWeaponPrefab = equippedItem.projectilePrefab;
            Logger.Log($"[FireBallWeapon] 리드 오리 - 장착 아이템 프로젝타일 사용: {equippedItem.Name}");
        }
        else
        {
            currentWeaponPrefab = weapon;
            Logger.LogWarning("[FireBallWeapon] 리드 오리 - 장착된 프로젝타일이 없어서 기본값 사용");
        }
    }
    else
    {
        currentWeaponPrefab = weapon;
        Logger.Log("[FireBallWeapon] 동료 오리 - 기본 프로젝타일 사용");
    }

    // ⭐ 디버그: 최종 결정된 프로젝타일 확인
    Logger.Log($"[FireBallWeapon] 최종 currentWeaponPrefab: {(currentWeaponPrefab == null ? "null" : currentWeaponPrefab.name)}");
}

    protected override void Attack()
    {
        base.Attack();
        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(1);
        if (closestEnemyPosition == null) return;
        if (closestEnemyPosition[0] == Vector2.zero)
        {
            return;
        }
        AttackCo();
    }

    void AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters(); // 총알마다 크리티컬 확률, 낙백 확률이 다르게 하기 위해
            SoundManager.instance.Play(shoot);
            //GameObject effect = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            //effect.transform.position = EffectPoint.position;

            // ⭐ currentWeaponPrefab 사용 (리드 오리면 장착 아이템, 동료면 기본값)
            GameObject fireBall = GameManager.instance.poolManager.GetMisc(currentWeaponPrefab);
            float index = 0f;
            if(i == 0)
            {
                index = 0;
            }
            else if(i == 1)
            {
                index = -15f;
            }
            else if (i == 2)
            {
                index = 15f;
            }

            if (fireBall != null)
            {
                Vector3 direction = Quaternion.AngleAxis(index, Vector3.forward) * dir;
                fireBall.transform.position = transform.position;
                fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
                ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
                projectile.Direction = direction;
                projectile.Speed = weaponStats.projectileSpeed;
                projectile.Damage = GetDamage();
                projectile.KnockBackChance = GetKnockBackChance();
                projectile.IsCriticalDamageProj = isCriticalDamage;
                // ✨ 투사체에 무기 이름 전달
                projectile.WeaponName = weaponData.DisplayName;
            }

            if (isSynergyWeaponActivated)
            {
                AnimShootExtra();
                SoundManager.instance.Play(shoot);
                // GameObject effectEx = GameManager.instance.poolManager.GetMisc(muzzleFlash);
                // effectEx.transform.position = EffectPointExtra.position;

                // ⭐ 시너지 무기도 currentWeaponPrefab 사용
                GameObject fireBallEx = GameManager.instance.poolManager.GetMisc(currentWeaponPrefab);
                if(fireBallEx != null)
                {
                    Vector3 directionExtra = Quaternion.AngleAxis(index, Vector3.forward) * dirExtra;
                    fireBallEx.transform.position = transform.position;
                    fireBallEx.transform.rotation = Quaternion.FromToRotation(Vector2.up, directionExtra);
                    ProjectileBase projectileEx = fireBallEx.GetComponent<ProjectileBase>();
                    projectileEx.Direction = directionExtra;
                    projectileEx.Speed = weaponStats.projectileSpeed;
                    projectileEx.Damage = GetDamage();
                    projectileEx.KnockBackChance = GetKnockBackChance();
                    projectileEx.IsCriticalDamageProj = isCriticalDamage;
                    // ✨ 시너지 투사체에도 무기 이름 전달
                    projectileEx.WeaponName = weaponData.DisplayName;
                }
            }
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}