using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 반사 레이저 무기 - 적/프롭에 닿으면 데미지를 주고 반사
/// BeamWeapon처럼 회전하며, numberOfAttacks에 따라 레이저 개수 증가
/// </summary>
public class ArcWeapon : WeaponBase
{
    [SerializeField] GameObject arcProjectile;
    [SerializeField] List<Transform> projectiles;
    bool isProjectileActive;

    [Header("Arc Settings")]
    [SerializeField] float duration; // projectile 지속 시간
    [SerializeField] float normalDuration = 0.5f;
    [SerializeField] float synergyDuration = 1f;
    [SerializeField] float angleSpread = 15f; // 레이저 사이각

    [SerializeField] GameObject muzzleFlash;
    GameObject muzzle;
    GameObject muzzle2;
    [SerializeField] AudioClip arcShoot;

    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
    }

    protected override void Update()
    {
        base.Update();

        // 회전판 회전 (BeamWeapon과 동일)
        transform.Rotate(Vector3.forward * weaponStats.projectileSpeed * Time.deltaTime);

        // ✨ 프로젝타일 상태 체크 (1초에 한 번)
        if (Time.frameCount % 60 == 0 && projectiles != null)
        {
            int activeCount = 0;
            int hasLineRendererCount = 0;

            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i] == null) continue;

                bool isActive = projectiles[i].gameObject.activeSelf;
                if (isActive) activeCount++;

                LineRenderer lr = projectiles[i].GetComponent<LineRenderer>();
                if (lr != null) hasLineRendererCount++;
            }
        }

        // 레이저가 비활성화된 상태에서만 쿨타임이 돌아감
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

        // 업그레이드되면 프로젝타일을 중단시키고 다시 시작
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
        {
            Logger.LogWarning($"[ArcWeapon] Already active, skipping GenProjectile");
            return;
        }

        // 초기화
        if (projectiles == null)
        {
            projectiles = new List<Transform>();
        }

        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;

        // 생성
        for (int i = 0; i < numberOfProjectilesToGen; i++)
        {
            Transform arcObject = Instantiate(arcProjectile, transform.position, Quaternion.identity, transform).transform;
            projectiles.Add(arcObject);

            // Logger.Log($"[ArcWeapon] ✅ Created projectile #{i + 1}");
        }

        // 시너지 설정
        if (isSynergyWeaponActivated)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].GetComponent<ArcProjectile>().SetAnimToSynergy();
            }
        }

        // 배치 및 stat 설정
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);
            projectiles[i].localPosition = Vector3.zero;
            projectiles[i].localRotation = Quaternion.identity;

            // 각도 배치
            float angle = (i - (weaponStats.numberOfAttacks - 1) / 2.0f) * angleSpread;
            Vector3 rotVec = Vector3.forward * angle;
            projectiles[i].Rotate(rotVec);

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = damage;
            projectile.KnockBackChance = knockback;
            projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
            projectile.IsCriticalDamageProj = isCriticalDamage;
            projectile.WeaponName = weaponData.DisplayName;
        }

        // muzzle flash 등...
        AnimShoot();

        // muzzle flash
        if (muzzle == null)
        {
            muzzle = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            muzzle2 = GameManager.instance.poolManager.GetMisc(muzzleFlash);

            if (muzzle != null)
            {
                Transform SLMuzzlePoint = GetComponentInParent<WeaponContainerAnim>().GetSLMuzzlePoint();
                if (SLMuzzlePoint != null)
                {
                    muzzle.transform.parent = SLMuzzlePoint;
                    muzzle.transform.position = SLMuzzlePoint.position;
                }
                else
                {
                    muzzle.transform.parent = ShootPoint; // 편의상 적당히 child를 따라다닐 오브젝트에 페어런트
                    muzzle.transform.position = ShootPoint.position - new Vector3(.2f, 0, 0);
                }
            }
            if (muzzle2 != null)
            {
                Transform SRMuzzlePoint = GetComponentInParent<WeaponContainerAnim>().GetSRMuzzlePoint();
                if (SRMuzzlePoint != null)
                {
                    muzzle2.transform.parent = SRMuzzlePoint;
                    muzzle2.transform.position = SRMuzzlePoint.position;
                }
                else
                {
                    muzzle2.transform.parent = ShootPoint; // 편의상 적당히 child를 따라다닐 오브젝트에 페어런트
                    muzzle2.transform.position = ShootPoint.position - new Vector3(-.2f, 0, 0);
                }
            }
        }

        if (muzzle != null) muzzle.gameObject.SetActive(true);
        if (muzzle2 != null) muzzle2.gameObject.SetActive(true);

        isProjectileActive = true;
        duration = isSynergyWeaponActivated ? synergyDuration : normalDuration;

        // sound
        if (arcShoot != null)
        {
            SoundManager.instance.PlaySoundWith(arcShoot, .7f, true, 0f);
        }
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
        if (muzzle2 != null)
        {
            muzzle2.gameObject.SetActive(false);
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        duration = synergyDuration;
    }
}