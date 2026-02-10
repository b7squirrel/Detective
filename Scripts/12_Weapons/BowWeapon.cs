using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowWeapon : WeaponBase
{
    [SerializeField] GameObject arrowProjectilePrefab;
    [SerializeField] GameObject synergyProjectilePrefab;
    [SerializeField] AudioClip shoot;
    
    [Header("Offset Settings")]
    [SerializeField] float positionOffsetRange = 0.5f; // 타겟 위치 주변 반경 (줄임)
    [SerializeField] float directionOffsetAngle = 5f; // 방향 각도 offset (줄임)
    [SerializeField] float shotDelay = 0.1f; // 화살 간 발사 간격
    
    [Header("Projectile Settings")]
    [SerializeField] float verticalVelocity = 15f; // 초기 수직 속도 (높을수록 높이 올라감)
    
    [Header("No Target Settings")]
    [SerializeField] float randomShotRadius = 5f; // 적이 없을 때 발사 반경
    
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
        
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters();
            SoundManager.instance.Play(shoot);
            
            // 🔥 각 화살마다 새로운 타겟 선택
            Vector2 targetPosition = SelectRandomTarget();
            
            // 랜덤 offset 적용된 타겟 위치 계산
            Vector2 randomOffset = Random.insideUnitCircle * positionOffsetRange;
            Vector2 offsetTargetPosition = targetPosition + randomOffset;
            
            GameObject projectileObj = GameManager.instance.poolManager.GetMisc(projectilePrefab);
            
            if (projectileObj != null)
            {
                // 위치 리셋
                projectileObj.transform.position = transform.position;
                
                // 자식 오브젝트 리셋
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

                // 수평 속도 = offset된 방향 * weaponStats.projectileSpeed 사용
                float offsetSpeedFactor = weaponStats.projectileSpeed * .2f;
                float speed = weaponStats.projectileSpeed + UnityEngine.Random.Range(-offsetSpeedFactor, offsetSpeedFactor);
                Vector2 groundVelocity = offsetDirection * speed;

                // BowProjectile 설정
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
                
                // ShadowHeight 설정
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

    // 🔥 타겟 선택 로직을 별도 메서드로 분리
    Vector2 SelectRandomTarget()
    {
        // 가장 가까운 적 5개 가져오기
        List<Vector2> closestEnemies = EnemyFinder.instance.GetEnemies(5);
        
        // 유효한 적들만 필터링 (Vector2.zero가 아닌 것들)
        List<Vector2> validEnemies = new List<Vector2>();
        if (closestEnemies != null)
        {
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                if (closestEnemies[i] != Vector2.zero)
                {
                    validEnemies.Add(closestEnemies[i]);
                }
            }
        }
        
        Vector2 targetPosition;
        
        // 유효한 적이 있으면 랜덤 선택
        if (validEnemies.Count > 0)
        {
            int randomIndex = Random.Range(0, validEnemies.Count);
            targetPosition = validEnemies[randomIndex];
            Logger.Log($"[bowWeapon] 적 선택: {targetPosition}");
        }
        else
        {
            // 적이 없으면 weaponContainer 주변 반경 안에 랜덤 위치
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(0f, randomShotRadius);
            targetPosition = (Vector2)transform.position + randomDirection * randomDistance;
            Logger.Log($"[bowWeapon] 랜덤 위치 선택: {targetPosition}");
        }
        
        return targetPosition;
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}