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
    GameObject muzzle; // muzzleFlash를 생성해서 담아두는 곳
    GameObject muzzle2; // muzzleFlash를 생성해서 담아두는 곳
    [SerializeField] AudioClip zapShoot;
    public AudioClip ZapShootSound => zapShoot; // ← 추가: ZapProjectile이 접근할 수 있도록

    // 랜덤 데미지 포인트
    Transform currentTarget;
    Vector2 cachedTargetPoint; // ← 추가: 타겟의 랜덤 히트 포인트 캐싱

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

        // 생성
        for (int i = 0; i < numberOfProjectilesToGen; i++)
        {
            Transform zapObject = Instantiate(zapProjectile, transform.position, Quaternion.identity, transform).transform;
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
        WeaponContainerAnim containerAnim = GetComponentInParent<WeaponContainerAnim>();
        Transform slPoint = containerAnim?.GetSLMuzzlePoint();
        Transform srPoint = containerAnim?.GetSRMuzzlePoint();

        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);

            ProjectileBase projectile = projectiles[i].GetComponent<ProjectileBase>();
            projectile.Damage = damage;
            projectile.KnockBackChance = knockback;
            projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
            projectile.IsCriticalDamageProj = isCriticalDamage;
            projectile.WeaponName = weaponData.DisplayName;

            // ✅ 추가: 짝수 인덱스 → SL, 홀수 인덱스 → SR
            ZapProjectile zap = projectiles[i].GetComponent<ZapProjectile>();
            if (zap != null)
            {
                Transform muzzle = (i % 2 == 0) ? slPoint : srPoint;
                zap.SetMuzzlePoint(muzzle);
            }
        }

        // // 눈 반짝
        // AnimShoot();

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
        muzzle.gameObject.SetActive(true);
        muzzle2.gameObject.SetActive(true);

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

        // 두 개의 muzzle을 모두 비활성화
        if (muzzle != null)
        {
            muzzle.gameObject.SetActive(false);
        }
        if (muzzle2 != null)  // ← 추가
        {
            muzzle2.gameObject.SetActive(false);  // ← 추가
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        duration = synergyDuration;
    }
}