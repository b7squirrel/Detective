using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatDuckWeapon : WeaponBase
{
    [SerializeField] GameObject laserPointerPrefab;
    [SerializeField] GameObject synergyLaserPointerPrefab;
    [SerializeField] GameObject catProjectilePrefab; // 고양이 프리펩
    [SerializeField] AudioClip laserShoot;
    [SerializeField] AudioClip catMeow; // 고양이 소리 (옵션)
    
    [Header("Laser Pointer Settings")]
    [SerializeField] float targetNearEnemyChance = 0.5f; // 적 근처를 타겟팅할 확률 (50%)
    [SerializeField] float enemyTargetRadius = 3f; // 적 근처 반경
    
    [Header("Cat Spawn Settings")]
    [SerializeField] float catSpawnDelay = 0.1f; // 고양이 간 스폰 간격
    [SerializeField] float catTrajectoryTime = 0.6f; // 고양이 비행 시간
    [SerializeField] float offScreenDistance = 3f; // 화면 밖 거리
    
    [Header("Screen Bounds")]
    float halfHeight;
    float halfWidth;

    protected override void Awake()
    {
        base.Awake();
        
        // 화면 크기 계산
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
    }

    protected override void Attack()
    {
        base.Attack();
        
        // 레이저 포인터 발사
        ShootLaserPointer();
    }

    void ShootLaserPointer()
    {
        // 타겟 위치 계산
        Vector2 targetPosition = GetTargetPosition();
        
        // 레이저 포인터 생성
        GameObject laserPointerPrefab = isSynergyWeaponActivated ? synergyLaserPointerPrefab : this.laserPointerPrefab;
        GameObject laserObj = GameManager.instance.poolManager.GetMisc(laserPointerPrefab);
        
        if (laserObj != null)
        {
            laserObj.transform.position = targetPosition;
            
            // 레이저 포인터 설정
            LaserPointer laserPointer = laserObj.GetComponent<LaserPointer>();
            if (laserPointer != null)
            {
                GetAttackParameters();
                laserPointer.Initialize(this, damage, knockback, knockbackSpeedFactor, isCriticalDamage, weaponData.DisplayName, weaponStats.sizeOfArea, weaponStats.numberOfAttacks);
            }
            else
            {
                Debug.LogError("CatDuckWeapon: LaserPointer component not found!");
            }
            
            // 사운드 재생
            SoundManager.instance.Play(laserShoot);
        }
        
        Debug.Log($"CatDuckWeapon: Laser pointer shot at {targetPosition}");
    }

    public void SpawnCats(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        StartCoroutine(SpawnCatsCo(targetPosition, pointer, numberOfCats));
    }

    IEnumerator SpawnCatsCo(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        for (int i = 0; i < numberOfCats; i++)
        {
            // 화면 밖 랜덤 위치에서 스폰
            Vector2 spawnPosition = GetOffScreenPosition(targetPosition);
            
            // 고양이 생성
            GameObject catObj = GameManager.instance.poolManager.GetMisc(catProjectilePrefab);
            
            if (catObj != null)
            {
                catObj.transform.position = spawnPosition;
                
                // 고양이 초기화
                CatProjectile catProjectile = catObj.GetComponent<CatProjectile>();
                if (catProjectile != null)
                {
                    catProjectile.Initialize(targetPosition, pointer, catTrajectoryTime);
                }
                
                // 고양이 소리 (옵션)
                if (catMeow != null)
                {
                    SoundManager.instance.Play(catMeow);
                }
                
                Debug.Log($"CatDuckWeapon: Cat {i + 1}/{numberOfCats} spawned at {spawnPosition}");
            }
            
            // 다음 고양이까지 딜레이
            yield return new WaitForSeconds(catSpawnDelay);
        }
    }

    Vector2 GetOffScreenPosition(Vector2 targetPosition)
    {
        // 4방향 중 랜덤 선택 (상, 하, 좌, 우)
        int direction = Random.Range(0, 4);
        
        Vector2 spawnPos = Vector2.zero;
        
        switch (direction)
        {
            case 0: // 위
                spawnPos = new Vector2(
                    Random.Range(-halfWidth, halfWidth),
                    halfHeight + offScreenDistance
                );
                break;
                
            case 1: // 아래
                spawnPos = new Vector2(
                    Random.Range(-halfWidth, halfWidth),
                    -halfHeight - offScreenDistance
                );
                break;
                
            case 2: // 왼쪽
                spawnPos = new Vector2(
                    -halfWidth - offScreenDistance,
                    Random.Range(-halfHeight, halfHeight)
                );
                break;
                
            case 3: // 오른쪽
                spawnPos = new Vector2(
                    halfWidth + offScreenDistance,
                    Random.Range(-halfHeight, halfHeight)
                );
                break;
        }
        
        return spawnPos;
    }

    Vector2 GetTargetPosition()
    {
        // 적 근처를 타겟팅할지 결정
        bool targetNearEnemy = Random.value < targetNearEnemyChance;
        
        if (targetNearEnemy)
        {
            // 가장 가까운 적 찾기
            List<Vector2> closestEnemies = EnemyFinder.instance.GetEnemies(1);
            
            if (closestEnemies != null && closestEnemies.Count > 0 && closestEnemies[0] != Vector2.zero)
            {
                // 적 근처 랜덤 위치
                Vector2 enemyPos = closestEnemies[0];
                Vector2 offset = Random.insideUnitCircle * enemyTargetRadius;
                Vector2 targetPos = enemyPos + offset;
                
                // 화면 밖으로 나가지 않도록 클램프
                targetPos = ClampToScreen(targetPos);
                return targetPos;
            }
        }
        
        // 완전 랜덤 위치 (화면 내)
        return GetRandomScreenPosition();
    }

    Vector2 GetRandomScreenPosition()
    {
        // 화면 안쪽 80% 영역에서 랜덤 (가장자리 제외)
        float margin = 2f;
        float x = Random.Range(-halfWidth + margin, halfWidth - margin);
        float y = Random.Range(-halfHeight + margin, halfHeight - margin);
        
        return new Vector2(x, y);
    }

    Vector2 ClampToScreen(Vector2 position)
    {
        float margin = 2f;
        float clampedX = Mathf.Clamp(position.x, -halfWidth + margin, halfWidth - margin);
        float clampedY = Mathf.Clamp(position.y, -halfHeight + margin, halfHeight - margin);
        
        return new Vector2(clampedX, clampedY);
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}