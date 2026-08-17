using System.Collections;
using UnityEngine;

public class BowWeapon : WeaponBase
{
    [SerializeField] GameObject arrowProjectilePrefab;
    [SerializeField] GameObject synergyProjectilePrefab;
    [SerializeField] AudioClip shoot;

    [Header("Timing")]
    [SerializeField] float shotDelay = 0.1f; // 화살 간 발사 간격

    [Header("Projectile Settings")]
    [SerializeField] float maxHeight = 3f; // 포물선 최고 높이

    [Header("Ground Damage Settings")]
    [SerializeField] float groundDamageRadius = 0.5f; // 착지 후 데미지 범위 (고정값)

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        GameObject projectilePrefab = isSynergyWeaponActivated ? synergyProjectilePrefab : arrowProjectilePrefab;

        // ✅ 이번 공격(volley) 전체가 공유할 중심점과 반경을 한 번만 계산
        Vector2 volleyCenter = transform.position;
        float landingRadius = weaponStats.sizeOfArea;
        // float landingRadius = 0f; // ✅ 임시: 반경 0으로 고정 (착지 위치 = 발사 위치, 순수 수직 포물선만 확인)
        Debug.Log($"[BowWeapon] landingRadius(sizeOfArea) = {landingRadius}"); // ✅ 추가

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters();
            SoundManager.instance.Play(shoot);

            Vector2 landingPosition = GetRandomPointInCircle(volleyCenter, landingRadius);
            Debug.Log($"[BowWeapon] shot {i}: landingPosition = {landingPosition}, offset from center = {landingPosition - volleyCenter}"); // ✅ 추가

            GameObject projectileObj = GameManager.instance.poolManager.GetMisc(projectilePrefab);

            if (projectileObj != null)
            {
                BowProjectile projectile = projectileObj.GetComponent<BowProjectile>();
                projectile?.PrepareForLaunch(); // ✅ 트레일 + 회전 기준점 모두 강제 리셋

                projectileObj.transform.position = transform.position; // 텔레포트

                SpriteRenderer sprite = projectileObj.GetComponentInChildren<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.transform.localPosition = Vector3.zero;
                }

                if (projectile != null)
                {
                    projectile.Damage = damage;
                    projectile.KnockBackChance = knockback;
                    projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
                    projectile.IsCriticalDamageProj = isCriticalDamage;
                    projectile.WeaponName = weaponData.DisplayName;
                    projectile.SizeOfArea = groundDamageRadius;
                }

                ShadowHeight shadowHeight = projectileObj.GetComponent<ShadowHeight>();
                if (shadowHeight != null)
                {
                    shadowHeight.InitializeBowArc(landingPosition, maxHeight);
                }
            }

            yield return new WaitForSeconds(shotDelay);
        }
    }

    // center를 기준으로 반경 radius 안에서 면적 기준 균등한 랜덤 위치 리턴
    Vector2 GetRandomPointInCircle(Vector2 center, float radius)
    {
        return center + Random.insideUnitCircle * radius;
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
    }

    protected override void RotateWeapon()
    {
        if (GameManager.instance.IsPaused) return;
        if (weaponTools == null) return;

        // 활은 특정 적을 조준하지 않으므로 항상 위(수직)를 보도록 고정
        weaponTools.transform.rotation = Quaternion.Euler(0, 0, 90f);
    }

    protected override void FlipWeaponTools()
    {
        // 활은 적 방향에 따른 좌우 반전이 필요 없으므로 아무 것도 하지 않음
    }
}