using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 반사 레이저 - 적을 관통하며 데미지, Wall/ScreenEdge에서만 반사
/// 최대 3번 반사, 반사할 때마다 데미지 감소
/// </summary>
public class ArcProjectile : ProjectileBase
{
    [Header("Arc Settings")]
    [SerializeField] float maxDistance = 100f; // 레이저 최대 길이
    [SerializeField] int maxReflections = 3; // 최대 반사 횟수
    [SerializeField] float damageDecayPerReflection = 0.15f; // 반사당 데미지 감소율 (15%)
    [SerializeField] float synergyMaxReflections = 5; // 시너지 시 최대 반사 횟수
    [SerializeField] float synergyDamageDecay = 0.1f; // 시너지 시 데미지 감소율 (10%)

    [Header("Visual")]
    [SerializeField] LineRenderer laserLine;
    [SerializeField] Transform hitEffect;
    [SerializeField] float baseWidth = 0.3f;
    [SerializeField] float maxWidth = 1.0f;
    [SerializeField] float baseDamage = 4f;
    [SerializeField] Color laserColor = new Color(1f, 0.5f, 0f); // 주황색
    [SerializeField] float alphaDecayPerReflection = 0.2f; // 반사당 알파 감소량
    [SerializeField] float widthOscillationSpeed = 10f;
    [SerializeField] float widthOscillationAmount = 0.1f;

    [Header("Layers")]
    [SerializeField] LayerMask destructables; // 적 (관통)
    [SerializeField] LayerMask walls; // 벽 (반사)
    [SerializeField] LayerMask screenEdges; // 화면 경계 (반사)

    int currentMaxReflections;
    bool isSynergyActivated;
    WeaponBase cachedWeapon;

    // 데미지를 입힌 적들을 추적 (한 프레임에 중복 데미지 방지)
    HashSet<Collider2D> damagedThisFrame = new HashSet<Collider2D>();
    int frameCount = 7; // BeamWeapon과 동일

    void OnEnable()
    {
        if (cachedWeapon == null)
        {
            cachedWeapon = GetComponentInParent<WeaponBase>();
        }

        currentMaxReflections = isSynergyActivated ? (int)synergyMaxReflections : maxReflections;
        damagedThisFrame.Clear();
    }


    void OnDisable()
    {
        HideLaser();
        damagedThisFrame.Clear();
    }

    protected override void Update()
    {
        if (Time.timeScale == 0) return;

        DrawAndCastLaser();

        // 프레임마다 데미지 추적 초기화
        if (Time.frameCount % frameCount == 0)
        {
            damagedThisFrame.Clear();
        }
    }

    void DrawAndCastLaser()
    {
        if (laserLine == null || cachedWeapon == null || cachedWeapon.ShootPoint == null)
            return;

        Vector2 startPos = cachedWeapon.ShootPoint.position;
        Vector2 direction = transform.up;

        // 반사 경로를 저장할 리스트
        List<Vector2> laserPath = new List<Vector2>();
        laserPath.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentDir = direction;
        int reflectionsUsed = 0;
        float currentDamageMultiplier = 1f;

        // ✨ 무한 루프 방지를 위한 최대 반복 횟수
        int maxIterations = currentMaxReflections + 1;
        int iterations = 0;

        // 반사 루프
        while (iterations < maxIterations)
        {
            iterations++;

            // RaycastAll로 모든 충돌 감지
            LayerMask allLayers = destructables | walls | screenEdges;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                currentPos,
                currentDir,
                maxDistance,
                allLayers
            );

            // 가장 가까운 반사면(벽) 찾기
            RaycastHit2D closestWallHit = new RaycastHit2D();
            float closestWallDistance = float.MaxValue;
            bool foundWall = false;

            foreach (var hit in hits)
            {
                // 벽 또는 화면 경계인지 확인
                int hitLayer = hit.collider.gameObject.layer;
                bool isWall = ((1 << hitLayer) & walls) != 0;
                bool isScreenEdge = ((1 << hitLayer) & screenEdges) != 0;

                if (isWall || isScreenEdge)
                {
                    float distance = Vector2.Distance(currentPos, hit.point);
                    if (distance < closestWallDistance && distance > 0.001f) // ✨ 매우 가까운 충돌 무시
                    {
                        closestWallDistance = distance;
                        closestWallHit = hit;
                        foundWall = true;
                    }
                }
            }

            // ✨ 레이저 끝점 결정 - 항상 벽에 닿도록
            Vector2 endPos;
            if (foundWall)
            {
                endPos = closestWallHit.point;
            }
            else
            {
                // ✨ 벽을 못 찾았다면 경고 로그 (정상적이라면 발생하지 않아야 함)
                Logger.LogWarning($"[ArcProjectile] No wall found! Direction: {currentDir}, Iterations: {iterations}");
                endPos = currentPos + currentDir * maxDistance;
                laserPath.Add(endPos);
                break; // 더 이상 진행 불가
            }

            // 경로에 끝점 추가
            laserPath.Add(endPos);

            // 적들에게 데미지 (프레임 간격으로)
            if (Time.frameCount % frameCount == 0)
            {
                DealDamageToEnemiesInPath(currentPos, endPos, currentDamageMultiplier, hits);
            }

            // 반사 처리
            if (foundWall && reflectionsUsed < currentMaxReflections)
            {
                // 반사 방향 계산 (입사각 = 반사각)
                currentDir = Vector2.Reflect(currentDir, closestWallHit.normal);
                currentPos = closestWallHit.point + closestWallHit.normal * 0.01f; // ✨ 벽에서 약간 떨어뜨림

                reflectionsUsed++;

                // 데미지 감소
                float decayRate = isSynergyActivated ? synergyDamageDecay : damageDecayPerReflection;
                currentDamageMultiplier *= (1f - decayRate);
            }
            else
            {
                // ✨ 최대 반사 횟수 도달 또는 벽 찾음 -> 레이저 종료
                break;
            }
        }

        // LineRenderer에 경로 그리기
        DrawLaserPath(laserPath, reflectionsUsed);
    }

    /// <summary>
    /// 레이저 경로상의 모든 적에게 데미지
    /// </summary>
    void DealDamageToEnemiesInPath(Vector2 start, Vector2 end, float damageMultiplier, RaycastHit2D[] allHits)
    {
        foreach (var hit in allHits)
        {
            // destructables 레이어인지 확인
            int hitLayer = hit.collider.gameObject.layer;
            bool isDestructable = ((1 << hitLayer) & destructables) != 0;

            if (!isDestructable)
                continue;

            // 이미 이번 프레임에 데미지를 입힌 적인지 확인
            if (damagedThisFrame.Contains(hit.collider))
                continue;

            // Idamageable 체크
            Idamageable damageable = hit.collider.GetComponent<Idamageable>();
            if (damageable == null)
                continue;

            // 화면 안에 있는지 체크
            SpriteRenderer sr = hit.collider.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && !sr.isVisible)
                continue;

            // 데미지 계산
            int adjustedDamage = Mathf.RoundToInt(Damage * damageMultiplier);

            PostMessage(adjustedDamage, hit.point);

            GameObject hitEffectObj = GetComponent<HitEffects>()?.hitEffect;
            if (hitEffectObj != null)
            {
                hitEffectObj.transform.position = hit.point;
            }

            damageable.TakeDamage(
                adjustedDamage,
                KnockBackChance,
                KnockBackSpeedFactor,
                hit.point,
                hitEffectObj
            );

            // 데미지 트래커 기록
            if (!string.IsNullOrEmpty(WeaponName))
            {
                DamageTracker.instance.RecordDamage(WeaponName, adjustedDamage);
            }

            // 데미지 입힌 적으로 기록
            damagedThisFrame.Add(hit.collider);
        }
    }

    void DrawLaserPath(List<Vector2> path, int reflectionsUsed)
    {
        if (laserLine == null)
        {
            Logger.LogWarning($"[ArcProjectile] {gameObject.name} - LaserLine is NULL!");
            return;
        }

        if (path.Count < 2)
        {
            Logger.LogWarning($"[ArcProjectile] {gameObject.name} - Path count too small: {path.Count}");
            return;
        }

        // LineRenderer 포지션 개수 설정
        laserLine.positionCount = path.Count;

        // 모든 포지션 설정
        for (int i = 0; i < path.Count; i++)
        {
            laserLine.SetPosition(i, path[i]);
        }

        // 데미지에 비례하여 굵기 조절
        float damageRatio = Mathf.Clamp(Damage / baseDamage, 1f, maxWidth / baseWidth);
        float targetWidth = baseWidth * damageRatio;

        // 진동 효과
        float oscillation = 1f + Mathf.Sin(Time.time * widthOscillationSpeed) * widthOscillationAmount;

        laserLine.widthMultiplier = targetWidth * oscillation;

        // 반사 횟수에 따라 알파값 감소
        float alpha = 1f - reflectionsUsed * alphaDecayPerReflection;
        alpha = Mathf.Max(alpha, 0.3f); // 최소 30%

        Color currentColor = new Color(laserColor.r, laserColor.g, laserColor.b, alpha);
        laserLine.startColor = currentColor;
        laserLine.endColor = currentColor;

        // Hit Effect는 마지막 지점에 표시
        if (hitEffect != null && path.Count > 0)
        {
            hitEffect.position = path[path.Count - 1];
            hitEffect.gameObject.SetActive(true);
        }
    }

    void HideLaser()
    {
        if (laserLine != null)
        {
            laserLine.positionCount = 0;
        }

        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(false);
        }
    }

    public void SetAnimToSynergy()
    {
        isSynergyActivated = true;
        currentMaxReflections = (int)synergyMaxReflections;
    }

    protected override void DieProjectile()
    {
        gameObject.SetActive(false);
    }

    protected override void HitObject()
    {
        gameObject.SetActive(false);
    }

    // ProjectileBase의 ApplyMovement와 CastDamage는 사용하지 않음
    protected override void ApplyMovement() { }
    protected override void CastDamage() { }
}