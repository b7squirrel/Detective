using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyWeapon : WeaponBase
{
    [SerializeField] GameObject partyProjectilePrefab; // 폴백용
    [SerializeField] AudioClip shoot;

    [Header("Offset Settings")]
    [SerializeField] float positionOffsetRange = 2.0f; // 타겟 위치 주변 반경
    [SerializeField] float directionOffsetAngle = 15f; // 방향 각도 offset (±도)
    [SerializeField] float shotDelay = 0.1f;           // 투사체 간 발사 간격

    [Header("Projectile Settings")]
    [SerializeField] float projectileSpeed = 10f;  // 수평 이동 속도
    [SerializeField] float verticalVelocity = 15f; // 초기 수직 속도 (높을수록 높이 올라감)

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // 런타임에 결정되는 프로젝타일
    GameObject currentPartyProjectilePrefab;

    // Attack에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentPartyProjectilePrefab = equippedItem.projectilePrefab;
            Logger.Log($"[PartyWeapon] 프로젝타일 사용: {equippedItem.Name} / IsLead: {InitialWeapon}");
        }
        else
        {
            currentPartyProjectilePrefab = partyProjectilePrefab;
            Logger.LogWarning("[PartyWeapon] 기본값 사용");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // 버퍼 재사용으로 new List 방지
        EnemyFinder.instance.GetEnemies(1, enemyQueryBuffer);
        if (enemyQueryBuffer.Count == 0 || enemyQueryBuffer[0] == Vector2.zero)
            return;

        StartCoroutine(AttackCo(enemyQueryBuffer[0]));
    }

    IEnumerator AttackCo(Vector2 targetPosition)
    {
        if (currentPartyProjectilePrefab == null)
        {
            Logger.LogError("[PartyWeapon] currentPartyProjectilePrefab이 null입니다!");
            yield break;
        }

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters();
            SoundManager.instance.Play(shoot);

            // 랜덤 offset 적용된 타겟 위치 계산
            Vector2 randomOffset = Random.insideUnitCircle * positionOffsetRange;
            Vector2 offsetTargetPosition = targetPosition + randomOffset;

            GameObject projectileObj = GameManager.instance.poolManager.GetMisc(currentPartyProjectilePrefab);

            if (projectileObj != null)
            {
                projectileObj.transform.position = ShootPoint.position;

                SpriteRenderer sprite = projectileObj.GetComponentInChildren<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.transform.localPosition = Vector3.zero;
                }

                // 타겟 방향 계산
                Vector2 direction = (offsetTargetPosition - (Vector2)transform.position).normalized;

                // 방향에 랜덤 각도 offset 적용
                float randomAngle = Random.Range(-directionOffsetAngle, directionOffsetAngle);
                Vector2 offsetDirection = Quaternion.Euler(0, 0, randomAngle) * direction;

                Vector2 groundVelocity = offsetDirection * projectileSpeed;

                PartyProjectile projectile = projectileObj.GetComponent<PartyProjectile>();
                if (projectile != null)
                {
                    projectile.Damage = damage;
                    projectile.KnockBackChance = knockback;
                    projectile.KnockBackSpeedFactor = knockbackSpeedFactor;
                    projectile.IsCriticalDamageProj = isCriticalDamage;
                    projectile.WeaponName = weaponData.DisplayName;
                    projectile.SizeOfArea = weaponStats.sizeOfArea;
                }

                ShadowHeight shadowHeight = projectileObj.GetComponent<ShadowHeight>();
                if (shadowHeight != null)
                {
                    shadowHeight.Initialize(groundVelocity, verticalVelocity);
                    Debug.Log($"GroundVelocity: {groundVelocity}, VerticalVelocity: {verticalVelocity}");
                }
                else
                {
                    Debug.LogError("PartyWeapon: ShadowHeight component not found on projectile!");
                }
            }

            yield return new WaitForSeconds(shotDelay);
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}