using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatDuckWeapon : WeaponBase
{
    [SerializeField] GameObject[] catProjectilePrefabs; // 다양한 색상의 고양이들
    [SerializeField] GameObject laserPointerPrefab;
    [SerializeField] GameObject synergyLaserPointerPrefab;
    [SerializeField] GameObject catProjectilePrefab; // 고양이 프리펩
    [SerializeField] AudioClip laserShoot;
    [SerializeField] AudioClip[] catMeows; // 고양이 소리 (옵션)

    [Header("Laser Pointer Settings")]
    [SerializeField] float enemyTargetRadius = 3f; // 적 근처 반경
    [SerializeField] float marginPercentage = 0.2f; // 화면 가장자리에서 20% 안쪽

    [Header("Cat Spawn Settings")]
    [SerializeField] float catSpawnDelay = 0.01f; // 고양이 간 스폰 간격
    [SerializeField] float catTrajectoryTime = 0.6f; // 고양이 비행 시간
    [SerializeField] float offScreenDistance = 3f; // 화면 밖 거리

    

    protected override void Awake()
    {
        base.Awake();
        // ✅ 카메라 크기 계산
        CalculateScreenBounds();
    }
    // ✅ 새 메서드 추가
    void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
        else
        {
            Logger.LogError("CatDuckWeapon: Main camera not found!");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // 가장 가까운 적 확인
        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(1);
        if (closestEnemyPosition == null || closestEnemyPosition.Count == 0)
            return;
        if (closestEnemyPosition[0] == Vector2.zero)
            return;

        // 레이저 포인터 발사
        ShootLaserPointer();
    }

    void ShootLaserPointer()
    {
        // 타겟 위치 계산
        Vector2 targetPosition = GetTargetPosition();

        // 타겟 위치가 유효하지 않으면 취소
        if (targetPosition == Vector2.zero)
        {
            return;
        }

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
                Logger.LogError("CatDuckWeapon: LaserPointer component not found!");
            }

            // 사운드 재생
            SoundManager.instance.Play(laserShoot);
        }
        else
        {
            Logger.LogError("CatDuckWeapon: Failed to get LaserPointer from pool!");
        }
    }

    public void SpawnCats(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        Logger.Log($"SpawnCats called: numberOfCats={numberOfCats}, catSpawnDelay={catSpawnDelay}");

        if (catSpawnDelay > 0f)
        {
            // 딜레이가 있으면 코루틴 사용
            StartCoroutine(SpawnCatsCo(targetPosition, pointer, numberOfCats));
        }
        else
        {
            // 딜레이가 0이면 즉시 모두 스폰
            SpawnCatsImmediately(targetPosition, pointer, numberOfCats);
        }
    }

    void SpawnCatsImmediately(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        for (int i = 0; i < numberOfCats; i++)
        {
            // 화면 밖 랜덤 위치에서 스폰
            Vector2 spawnPosition = GetOffScreenPosition(targetPosition);

            // 랜덤하게 고양이 프리펩 선택
            GameObject selectedPrefab = catProjectilePrefabs[Random.Range(0, catProjectilePrefabs.Length)];
            GameObject catObj = GameManager.instance.poolManager.GetMisc(selectedPrefab);

            if (catObj != null)
            {
                catObj.transform.position = spawnPosition;

                // 고양이 초기화 - 모두 레이저 포인터로 날아감
                CatProjectile catProjectile = catObj.GetComponent<CatProjectile>();


                if (catProjectile != null)
                {
                    catProjectile.Initialize(targetPosition, pointer, catTrajectoryTime);
                }
                else
                {
                    Logger.LogError($"Cat {i + 1}: CatProjectile component not found!");
                }

                // 고양이 소리 (처음 4마리만, 배열에서 랜덤 선택)
                if (catMeows != null && catMeows.Length > 0 && i < 4)
                {
                    int index = UnityEngine.Random.Range(0, catMeows.Length);
                    SoundManager.instance.Play(catMeows[index]);
                }

            }
            else
            {
                Logger.LogError($"Failed to spawn cat {i + 1}!");
            }
        }
    }
    IEnumerator SpawnCatsCo(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        for (int i = 0; i < numberOfCats; i++)
        {
            // 화면 밖 랜덤 위치에서 스폰
            Vector2 spawnPosition = GetOffScreenPosition(targetPosition);

            // 랜덤하게 고양이 프리펩 선택
            GameObject selectedPrefab = catProjectilePrefabs[Random.Range(0, catProjectilePrefabs.Length)];
            GameObject catObj = GameManager.instance.poolManager.GetMisc(selectedPrefab);

            if (catObj != null)
            {
                catObj.transform.position = spawnPosition;

                // 고양이 초기화 - 모두 레이저 포인터로 날아감
                CatProjectile catProjectile = catObj.GetComponent<CatProjectile>();
                if (catProjectile != null)
                {
                    catProjectile.Initialize(targetPosition, pointer, catTrajectoryTime);
                }

                // 고양이 소리 (처음 4마리만, 배열에서 랜덤 선택)
                if (catMeows != null && catMeows.Length > 0 && i < 4)
                {
                    int index = UnityEngine.Random.Range(0, catMeows.Length);
                    SoundManager.instance.Play(catMeows[index]);
                }
            }

            // 다음 고양이까지 딜레이
            yield return new WaitForSeconds(catSpawnDelay);
        }
    }

    Vector2 GetOffScreenPosition(Vector2 targetPosition)
{
    // ✅ 카메라 위치 가져오기
    Camera cam = Camera.main;
    Vector2 camPos = cam.transform.position;
    
    // 4방향 중 랜덤 선택 (상, 하, 좌, 우)
    int direction = Random.Range(0, 4);

    Vector2 spawnPos = Vector2.zero;

    switch (direction)
    {
        case 0: // 위
            spawnPos = new Vector2(
                camPos.x + Random.Range(-halfWidth, halfWidth),  // ✅ 카메라 X 추가
                camPos.y + halfHeight + offScreenDistance         // ✅ 카메라 Y 추가
            );
            break;

        case 1: // 아래
            spawnPos = new Vector2(
                camPos.x + Random.Range(-halfWidth, halfWidth),  // ✅ 카메라 X 추가
                camPos.y - halfHeight - offScreenDistance         // ✅ 카메라 Y 추가
            );
            break;

        case 2: // 왼쪽
            spawnPos = new Vector2(
                camPos.x - halfWidth - offScreenDistance,         // ✅ 카메라 X 추가
                camPos.y + Random.Range(-halfHeight, halfHeight)  // ✅ 카메라 Y 추가
            );
            break;

        case 3: // 오른쪽
            spawnPos = new Vector2(
                camPos.x + halfWidth + offScreenDistance,         // ✅ 카메라 X 추가
                camPos.y + Random.Range(-halfHeight, halfHeight)  // ✅ 카메라 Y 추가
            );
            break;
    }

    return spawnPos;
}

    Vector2 GetTargetPosition()
    {
        // 가장 가까운 적 찾기
        List<Vector2> closestEnemies = EnemyFinder.instance.GetEnemies(1);

        // 적이 없으면 공격하지 않음
        if (closestEnemies == null || closestEnemies.Count == 0 || closestEnemies[0] == Vector2.zero)
        {
            Debug.Log("CatDuckWeapon: No enemies found, skipping attack");
            return Vector2.zero;
        }

        // 적 위치
        Vector2 enemyPos = closestEnemies[0];

        // 적 근처 랜덤 오프셋
        Vector2 offset = Random.insideUnitCircle * enemyTargetRadius;
        Vector2 targetPos = enemyPos + offset;

        // 화면 안으로 클램프
        targetPos = ClampToScreen(targetPos);

        return targetPos;
    }

    Vector2 ClampToScreen(Vector2 position)
    {
        // 화면 크기에 따른 안전한 margin 계산
        float safeMarginX = halfWidth * marginPercentage;
        float safeMarginY = halfHeight * marginPercentage;

        float clampedX = Mathf.Clamp(position.x, -halfWidth + safeMarginX, halfWidth - safeMarginX);
        float clampedY = Mathf.Clamp(position.y, -halfHeight + safeMarginY, halfHeight - safeMarginY);

        return new Vector2(clampedX, clampedY);
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}