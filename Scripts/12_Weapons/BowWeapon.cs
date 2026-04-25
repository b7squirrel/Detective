using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowWeapon : WeaponBase
{
    [SerializeField] GameObject arrowProjectilePrefab;
    [SerializeField] GameObject synergyProjectilePrefab;
    [SerializeField] AudioClip shoot;

    [Header("Offset Settings")]
    [SerializeField] float positionOffsetRange = 0.5f; // 타겟 위치 주변 반경
    [SerializeField] float directionOffsetAngle = 5f;  // 방향 각도 offset
    [SerializeField] float shotDelay = 0.1f;           // 화살 간 발사 간격

    [Header("Projectile Settings")]
    [SerializeField] float verticalVelocity = 15f; // 초기 수직 속도 (높을수록 높이 올라감)

    [Header("No Target Settings")]
    [SerializeField] float randomShotRadius = 5f; // 적이 없을 때 발사 반경

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // SelectRandomTarget에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(5);

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        GameObject projectilePrefab = isSynergyWeaponActivated ? synergyProjectilePrefab : arrowProjectilePrefab;

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters();
            SoundManager.instance.Play(shoot);

            // 각 화살마다 새로운 타겟 선택
            Vector2 targetPosition = SelectRandomTarget();

            // 랜덤 offset 적용된 타겟 위치 계산
            Vector2 randomOffset = Random.insideUnitCircle * positionOffsetRange;
            Vector2 offsetTargetPosition = targetPosition + randomOffset;

            GameObject projectileObj = GameManager.instance.poolManager.GetMisc(projectilePrefab);

            if (projectileObj != null)
            {
                projectileObj.transform.position = transform.position;

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

                float offsetSpeedFactor = weaponStats.projectileSpeed * .2f;
                float speed = weaponStats.projectileSpeed + UnityEngine.Random.Range(-offsetSpeedFactor, offsetSpeedFactor);
                Vector2 groundVelocity = offsetDirection * speed;

                BowProjectile projectile = projectileObj.GetComponent<BowProjectile>();
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
                }
                else
                {
                    Debug.LogError("BowWeapon: ShadowHeight component not found on projectile!");
                }
            }

            yield return new WaitForSeconds(shotDelay);
        }
    }

    // 타겟 선택 로직. 필드 버퍼 재사용으로 new List 방지
    Vector2 SelectRandomTarget()
    {
        EnemyFinder.instance.GetEnemies(5, enemyQueryBuffer);

        // 유효한 적(Vector2.zero가 아닌) 중에서 랜덤 선택
        int validCount = 0;
        for (int i = 0; i < enemyQueryBuffer.Count; i++)
        {
            if (enemyQueryBuffer[i] != Vector2.zero)
                validCount++;
        }

        if (validCount > 0)
        {
            int randomIndex = Random.Range(0, validCount);
            return enemyQueryBuffer[randomIndex];
        }

        // 적이 없으면 주변 랜덤 위치로 발사
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(0f, randomShotRadius);
        return (Vector2)transform.position + randomDirection * randomDistance;
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
    }
}