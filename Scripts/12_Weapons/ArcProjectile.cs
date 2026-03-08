using System.Collections.Generic;
using UnityEngine;

public class ArcProjectile : ProjectileBase
{
    [Header("Arc Settings")]
    [SerializeField] float maxDistance = 100f;

    [Tooltip("upgradeData의 sizeOfArea로 결정")]
    [SerializeField] int maxReflections = 1;
    [SerializeField] float damageDecayPerReflection = 0.15f;
    [SerializeField] float synergyDamageDecay = 0.1f;

    [Header("Visual - Dual LineRenderer")]
    LineRenderer outerLine;
    LineRenderer innerLine;

    [Header("Hit Effects")]
    [SerializeField] GameObject reflectionHitEffect; // ✨ 반사 지점용
    [SerializeField] GameObject endHitEffect; // ✨ 끝점용

    [Header("Inner Line (Core)")]
    [SerializeField] float innerBaseWidth = 0.2f;
    [SerializeField] Color innerColor = new Color(1f, 1f, 0.8f);

    [Header("Outer Line (Glow)")]
    [SerializeField] float outerBaseWidth = 0.5f;
    [SerializeField] Color outerColor = new Color(1f, 0.5f, 0f);

    [Header("Common Settings")]
    [SerializeField] float baseDamage = 4f;
    [SerializeField] float alphaDecayPerReflection = 0.05f;

    [Header("Layers")]
    [SerializeField] LayerMask destructables;
    [SerializeField] LayerMask walls;
    [SerializeField] LayerMask screenEdges;

    bool isSynergyActivated;
    WeaponBase cachedWeapon;
    Transform assignedMuzzlePoint; // ✅ 추가: CenterMuzzlePoint 저장

    HashSet<Collider2D> damagedThisFrame = new HashSet<Collider2D>();
    int frameCount = 7;

    // ✨ 반사 지점 HitEffect들을 저장
    List<GameObject> activeReflectionEffects = new List<GameObject>();
    GameObject activeEndEffect; // 끝점 effect

    // ✅ 추가: 외부에서 머즐 포인트 주입
    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        assignedMuzzlePoint = muzzlePoint;
    }

    void Awake()
    {
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();

        if (lineRenderers.Length >= 2)
        {
            outerLine = lineRenderers[0];
            innerLine = lineRenderers[1];
        }
        else
        {
            Logger.LogWarning($"[ArcProjectile] Need 2 LineRenderers! Found: {lineRenderers.Length}");
        }
    }

    void OnEnable()
    {
        if (cachedWeapon == null)
            cachedWeapon = GetComponentInParent<WeaponBase>();

        damagedThisFrame.Clear();

        // ✅ null 요소 제거 후 재시도할 수 있도록 정리
        activeReflectionEffects.RemoveAll(e => e == null);
        activeEndEffect = null; // ✅ 매번 새로 가져오도록 초기화
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

        if (Time.frameCount % frameCount == 0)
        {
            damagedThisFrame.Clear();
        }
    }

    void DrawAndCastLaser()
    {
        if ((innerLine == null && outerLine == null))
        {
            Logger.LogWarning($"[ArcProjectile] No LineRenderer!");
            return;
        }

        if (cachedWeapon == null)
        {
            Logger.LogWarning($"[ArcProjectile] No cachedWeapon!");
            return;
        }

        // ✅ 수정: assignedMuzzlePoint가 있으면 사용, 없으면 ShootPoint 폴백
        Transform originPoint = (assignedMuzzlePoint != null) ? assignedMuzzlePoint : cachedWeapon.ShootPoint;

        if (originPoint == null)
        {
            Logger.LogWarning($"[ArcProjectile] No origin point (MuzzlePoint or ShootPoint)!");
            return;
        }

        Vector2 startPos = originPoint.position;
        Vector2 direction = transform.up;

        if (Time.frameCount % 60 == 0)
        {
            Logger.Log($"[ArcProjectile] DrawAndCastLaser - startPos: {startPos}, direction: {direction}");
        }

        List<Vector2> laserPath = new List<Vector2>();
        laserPath.Add(startPos);

        List<Vector2> reflectionPoints = new List<Vector2>();

        Vector2 currentPos = startPos;
        Vector2 currentDir = direction;
        int reflectionsUsed = 0;
        float currentDamageMultiplier = 1f;

        // ✅ 시너지 보너스를 런타임으로 계산
        int effectiveMaxReflections = maxReflections + (isSynergyActivated ? 1 : 0);

        int maxIterations = effectiveMaxReflections + 1;
        int iterations = 0;

        while (iterations < maxIterations)
        {
            iterations++;

            LayerMask allLayers = destructables | walls | screenEdges;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                currentPos,
                currentDir,
                maxDistance,
                allLayers
            );

            RaycastHit2D closestWallHit = new RaycastHit2D();
            float closestWallDistance = float.MaxValue;
            bool foundWall = false;

            foreach (var hit in hits)
            {
                int hitLayer = hit.collider.gameObject.layer;
                bool isWall = ((1 << hitLayer) & walls) != 0;
                bool isScreenEdge = ((1 << hitLayer) & screenEdges) != 0;

                if (isWall || isScreenEdge)
                {
                    float distance = Vector2.Distance(currentPos, hit.point);
                    if (distance < closestWallDistance && distance > 0.001f)
                    {
                        closestWallDistance = distance;
                        closestWallHit = hit;
                        foundWall = true;
                    }
                }
            }

            Vector2 endPos;
            if (foundWall)
            {
                endPos = closestWallHit.point;
            }
            else
            {
                endPos = currentPos + currentDir * maxDistance;
                laserPath.Add(endPos);
                break;
            }

            laserPath.Add(endPos);

            if (Time.frameCount % frameCount == 0)
            {
                DealDamageToEnemiesInPath(currentPos, endPos, currentDamageMultiplier, hits);
            }

            // ✅ effectiveMaxReflections 사용
            if (foundWall && reflectionsUsed < effectiveMaxReflections)
            {
                reflectionPoints.Add(closestWallHit.point);

                currentDir = Vector2.Reflect(currentDir, closestWallHit.normal);
                currentPos = closestWallHit.point + closestWallHit.normal * 0.01f;

                reflectionsUsed++;

                float decayRate = isSynergyActivated ? synergyDamageDecay : damageDecayPerReflection;
                currentDamageMultiplier *= (1f - decayRate);
            }
            else
            {
                break;
            }
        }

        if (Time.frameCount % 60 == 0)
        {
            Logger.Log($"[ArcProjectile] Path count: {laserPath.Count}, Reflection points: {reflectionPoints.Count}");
        }

        DrawLaserPath(laserPath, reflectionsUsed);
        UpdateHitEffects(reflectionPoints, laserPath[laserPath.Count - 1]);
    }

    void DealDamageToEnemiesInPath(Vector2 start, Vector2 end, float damageMultiplier, RaycastHit2D[] allHits)
    {
        foreach (var hit in allHits)
        {
            int hitLayer = hit.collider.gameObject.layer;
            bool isDestructable = ((1 << hitLayer) & destructables) != 0;

            if (!isDestructable)
                continue;

            if (damagedThisFrame.Contains(hit.collider))
                continue;

            Idamageable damageable = hit.collider.GetComponent<Idamageable>();
            if (damageable == null)
                continue;

            SpriteRenderer sr = hit.collider.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && !sr.isVisible)
                continue;

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

            if (!string.IsNullOrEmpty(WeaponName))
            {
                DamageTracker.instance.RecordDamage(WeaponName, adjustedDamage);
            }

            damagedThisFrame.Add(hit.collider);
        }
    }

    void DrawLaserPath(List<Vector2> path, int reflectionsUsed)
    {
        if (path.Count < 2)
            return;

        float alpha = 1f - reflectionsUsed * alphaDecayPerReflection;
        alpha = Mathf.Max(alpha, 0.6f);

        // Outer Line
        if (outerLine != null)
        {
            outerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                outerLine.SetPosition(i, path[i]);
            }

            outerLine.startWidth = outerBaseWidth;
            outerLine.endWidth = outerBaseWidth;

            Color currentOuterColor = new Color(
                outerColor.r,
                outerColor.g,
                outerColor.b,
                alpha * 0.8f
            );
            outerLine.startColor = currentOuterColor;
            outerLine.endColor = currentOuterColor;
        }

        // Inner Line
        if (innerLine != null)
        {
            innerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                innerLine.SetPosition(i, path[i]);
            }

            innerLine.startWidth = innerBaseWidth;
            innerLine.endWidth = innerBaseWidth;

            Color currentInnerColor = new Color(
                innerColor.r,
                innerColor.g,
                innerColor.b,
                1f
            );
            innerLine.startColor = currentInnerColor;
            innerLine.endColor = currentInnerColor;
        }
    }

    // ✨ HitEffect 업데이트
    void UpdateHitEffects(List<Vector2> reflectionPoints, Vector2 endPoint)
    {
        // 1. 반사 지점 Effects
        if (reflectionHitEffect != null)
        {
            while (activeReflectionEffects.Count < reflectionPoints.Count)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(reflectionHitEffect);
                if (effect != null)
                    activeReflectionEffects.Add(effect);
                else
                    break;
            }

            for (int i = 0; i < reflectionPoints.Count; i++)
            {
                if (i < activeReflectionEffects.Count && activeReflectionEffects[i] != null)
                {
                    activeReflectionEffects[i].transform.position = reflectionPoints[i];
                    activeReflectionEffects[i].SetActive(true);
                }
            }

            for (int i = reflectionPoints.Count; i < activeReflectionEffects.Count; i++)
            {
                if (activeReflectionEffects[i] != null)
                    activeReflectionEffects[i].SetActive(false);
            }
        }

        // 2. 끝점 Effect
        if (endHitEffect != null)
        {
            // ✅ null이면 매번 재시도
            if (activeEndEffect == null)
                activeEndEffect = GameManager.instance.poolManager.GetMisc(endHitEffect);

            if (activeEndEffect != null)
            {
                activeEndEffect.transform.position = endPoint;
                activeEndEffect.SetActive(true);
            }
        }
    }

    void HideLaser()
    {
        if (innerLine != null)
        {
            innerLine.positionCount = 0;
        }

        if (outerLine != null)
        {
            outerLine.positionCount = 0;
        }

        // ✨ 모든 HitEffect 비활성화
        foreach (var effect in activeReflectionEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }

        if (activeEndEffect != null)
        {
            activeEndEffect.SetActive(false);
        }
    }

    // SetAnimToSynergy() - maxReflections 직접 수정 제거
    public void SetAnimToSynergy()
    {
        if (isSynergyActivated) return;
        isSynergyActivated = true;
    }

    public void AddMaxReflections(int amount)
    {
        maxReflections += amount;
    }

    protected override void DieProjectile()
    {
        gameObject.SetActive(false);
    }

    protected override void HitObject()
    {
        gameObject.SetActive(false);
    }

    protected override void ApplyMovement() { }
    protected override void CastDamage() { }
}