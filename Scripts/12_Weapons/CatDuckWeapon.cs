using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatDuckWeapon : WeaponBase
{
    [SerializeField] GameObject laserPointerPrefab;
    [SerializeField] GameObject synergyLaserPointerPrefab;
    [SerializeField] AudioClip laserShoot;
    [SerializeField] AudioClip[] catMeows;

    [Header("Laser Pointer Settings")]
    [SerializeField] float enemyTargetRadius = 3f;
    [SerializeField] float marginPercentage = 0.2f;

    [Header("Cat Spawn Settings")]
    [SerializeField] float catSpawnDelay = 0.01f;
    [SerializeField] float catTrajectoryTime = 0.6f;
    [SerializeField] float offScreenDistance = 3f;
    [SerializeField] float catMaxHeight = 3f;

    // ⭐ 런타임에 결정되는 고양이 프리팹
    GameObject currentCatPrefab;

    protected override void Awake()
    {
        base.Awake();
        CalculateScreenBounds();
    }

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
            Logger.LogError("[CatDuckWeapon] Main camera not found!");
        }
    }

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentCatPrefab = equippedItem.projectilePrefab;
            Logger.Log($"[CatDuckWeapon] 고양이 프리팹 사용: {equippedItem.Name} / IsLead: {InitialWeapon}");
        }
        else
        {
            Logger.LogWarning("[CatDuckWeapon] 장착된 고양이 모자 아이템이 없습니다!");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(1);
        if (closestEnemyPosition == null || closestEnemyPosition.Count == 0)
            return;
        if (closestEnemyPosition[0] == Vector2.zero)
            return;

        ShootLaserPointer();
    }

    void ShootLaserPointer()
    {
        Vector2 targetPosition = GetTargetPosition();

        if (targetPosition == Vector2.zero)
            return;

        GameObject laserPrefab = isSynergyWeaponActivated ? synergyLaserPointerPrefab : laserPointerPrefab;
        GameObject laserObj = GameManager.instance.poolManager.GetMisc(laserPrefab);

        if (laserObj != null)
        {
            laserObj.transform.position = targetPosition;

            LaserPointer laserPointer = laserObj.GetComponent<LaserPointer>();
            if (laserPointer != null)
            {
                GetAttackParameters();
                laserPointer.Initialize(this, damage, knockback, knockbackSpeedFactor, isCriticalDamage, weaponData.DisplayName, weaponStats.sizeOfArea, weaponStats.numberOfAttacks);
            }
            else
            {
                Logger.LogError("[CatDuckWeapon] LaserPointer component not found!");
            }

            SoundManager.instance.Play(laserShoot);
        }
        else
        {
            Logger.LogError("[CatDuckWeapon] Failed to get LaserPointer from pool!");
        }
    }

    public void SpawnCats(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        if (currentCatPrefab == null)
        {
            Logger.LogError("[CatDuckWeapon] currentCatPrefab이 null입니다!");
            return;
        }

        Logger.Log($"SpawnCats called: numberOfCats={numberOfCats}, catSpawnDelay={catSpawnDelay}");

        if (catSpawnDelay > 0f)
        {
            StartCoroutine(SpawnCatsCo(targetPosition, pointer, numberOfCats));
        }
        else
        {
            SpawnCatsImmediately(targetPosition, pointer, numberOfCats);
        }
    }

    void SpawnCatsImmediately(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        for (int i = 0; i < numberOfCats; i++)
        {
            Vector2 spawnPosition = GetOffScreenPosition(targetPosition);
            GameObject catObj = GameManager.instance.poolManager.GetMisc(currentCatPrefab);

            if (catObj != null)
            {
                catObj.transform.position = spawnPosition;
                CatProjectile catProjectile = catObj.GetComponent<CatProjectile>();
                if (catProjectile != null)
                    catProjectile.InitializeWithHeight(targetPosition, pointer, catMaxHeight);

                if (catMeows != null && catMeows.Length > 0 && i < 4)
                    SoundManager.instance.Play(catMeows[Random.Range(0, catMeows.Length)]);
            }
        }
    }

    IEnumerator SpawnCatsCo(Vector2 targetPosition, LaserPointer pointer, int numberOfCats)
    {
        for (int i = 0; i < numberOfCats; i++)
        {
            Vector2 spawnPosition = GetOffScreenPosition(targetPosition);
            GameObject catObj = GameManager.instance.poolManager.GetMisc(currentCatPrefab);

            if (catObj != null)
            {
                catObj.transform.position = spawnPosition;
                CatProjectile catProjectile = catObj.GetComponent<CatProjectile>();
                if (catProjectile != null)
                    catProjectile.InitializeWithHeight(targetPosition, pointer, catMaxHeight);

                if (catMeows != null && catMeows.Length > 0 && i < 4)
                    SoundManager.instance.Play(catMeows[Random.Range(0, catMeows.Length)]);
            }

            yield return new WaitForSeconds(catSpawnDelay);
        }
    }

    Vector2 GetOffScreenPosition(Vector2 targetPosition)
    {
        Camera cam = Camera.main;
        Vector2 camPos = cam.transform.position;

        int direction = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (direction)
        {
            case 0: // 위
                spawnPos = new Vector2(
                    camPos.x + Random.Range(-halfWidth, halfWidth),
                    camPos.y + halfHeight + offScreenDistance
                );
                break;
            case 1: // 아래
                spawnPos = new Vector2(
                    camPos.x + Random.Range(-halfWidth, halfWidth),
                    camPos.y - halfHeight - offScreenDistance
                );
                break;
            case 2: // 왼쪽
                spawnPos = new Vector2(
                    camPos.x - halfWidth - offScreenDistance,
                    camPos.y + Random.Range(-halfHeight, halfHeight)
                );
                break;
            case 3: // 오른쪽
                spawnPos = new Vector2(
                    camPos.x + halfWidth + offScreenDistance,
                    camPos.y + Random.Range(-halfHeight, halfHeight)
                );
                break;
        }

        return spawnPos;
    }

    Vector2 GetTargetPosition()
    {
        List<Vector2> closestEnemies = EnemyFinder.instance.GetEnemies(1);

        if (closestEnemies == null || closestEnemies.Count == 0 || closestEnemies[0] == Vector2.zero)
            return Vector2.zero;

        Vector2 enemyPos = closestEnemies[0];
        Vector2 offset = Random.insideUnitCircle * enemyTargetRadius;
        Vector2 targetPos = enemyPos + offset;

        return ClampToScreen(targetPos);
    }

    Vector2 ClampToScreen(Vector2 position)
    {
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