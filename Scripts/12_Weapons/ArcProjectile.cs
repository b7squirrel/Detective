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
    [SerializeField] GameObject reflectionHitEffect;
    [SerializeField] GameObject endHitEffect;

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
    Transform assignedMuzzlePoint;

    HashSet<Collider2D> damagedThisFrame = new HashSet<Collider2D>();
    int frameCount = 7;

    List<GameObject> activeReflectionEffects = new List<GameObject>();
    GameObject activeEndEffect;

    // ✅ 매 프레임 new List 방지: 필드로 선언 후 Clear() 재사용
    readonly List<Vector2> laserPath = new List<Vector2>(10);
    readonly List<Vector2> reflectionPoints = new List<Vector2>(5);

    // ✅ RaycastNonAlloc용 static 버퍼
    static readonly RaycastHit2D[] raycastBuffer = new RaycastHit2D[20];

    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        assignedMuzzlePoint = muzzlePoint;
    }

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱

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
        activeReflectionEffects.RemoveAll(e => e == null);
        activeEndEffect = null;
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
        if (innerLine == null && outerLine == null)
        {
            Logger.LogWarning($"[ArcProjectile] No LineRenderer!");
            return;
        }

        if (cachedWeapon == null)
        {
            Logger.LogWarning($"[ArcProjectile] No cachedWeapon!");
            return;
        }

        Transform originPoint = assignedMuzzlePoint != null ? assignedMuzzlePoint : cachedWeapon.ShootPoint;
        if (originPoint == null)
        {
            Logger.LogWarning($"[ArcProjectile] No origin point!");
            return;
        }

        Vector2 startPos = originPoint.position;
        Vector2 direction = transform.up;

        // ✅ Clear()로 재사용 (new List 제거)
        laserPath.Clear();
        reflectionPoints.Clear();
        laserPath.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentDir = direction;
        int reflectionsUsed = 0;
        float currentDamageMultiplier = 1f;

        int effectiveMaxReflections = maxReflections + (isSynergyActivated ? 1 : 0);
        int maxIterations = effectiveMaxReflections + 1;
        int iterations = 0;

        LayerMask allLayers = destructables | walls | screenEdges;

        while (iterations < maxIterations)
        {
            iterations++;

            // ✅ RaycastNonAlloc으로 GC 방지
            int hitCount = Physics2D.RaycastNonAlloc(currentPos, currentDir, raycastBuffer, maxDistance, allLayers);

            RaycastHit2D closestWallHit = new RaycastHit2D();
            float closestWallDistance = float.MaxValue;
            bool foundWall = false;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = raycastBuffer[i];
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
                DealDamageToEnemiesInPath(currentPos, endPos, currentDamageMultiplier, hitCount);
            }

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

        DrawLaserPath(laserPath, reflectionsUsed);
        UpdateHitEffects(reflectionPoints, laserPath[laserPath.Count - 1]);
    }

    // ✅ hitCount 파라미터로 변경 (별도 배열 전달 불필요 - static 버퍼 공유)
    void DealDamageToEnemiesInPath(Vector2 start, Vector2 end, float damageMultiplier, int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            var hit = raycastBuffer[i];
            int hitLayer = hit.collider.gameObject.layer;
            bool isDestructable = ((1 << hitLayer) & destructables) != 0;
            if (!isDestructable) continue;
            if (damagedThisFrame.Contains(hit.collider)) continue;

            Idamageable damageable = hit.collider.GetComponent<Idamageable>();
            if (damageable == null) continue;

            SpriteRenderer sr = hit.collider.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && !sr.isVisible) continue;

            int adjustedDamage = Mathf.RoundToInt(Damage * damageMultiplier);
            PostMessage(adjustedDamage, hit.point);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : null;
            if (hitEffectObj != null)
                hitEffectObj.transform.position = hit.point;

            damageable.TakeDamage(adjustedDamage, KnockBackChance, KnockBackSpeedFactor, hit.point, hitEffectObj);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, adjustedDamage);

            damagedThisFrame.Add(hit.collider);
        }
    }

    void DrawLaserPath(List<Vector2> path, int reflectionsUsed)
    {
        if (path.Count < 2) return;

        float alpha = Mathf.Max(1f - reflectionsUsed * alphaDecayPerReflection, 0.6f);

        if (outerLine != null)
        {
            outerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
                outerLine.SetPosition(i, path[i]);

            outerLine.startWidth = outerBaseWidth;
            outerLine.endWidth = outerBaseWidth;
            Color c = new Color(outerColor.r, outerColor.g, outerColor.b, alpha * 0.8f);
            outerLine.startColor = c;
            outerLine.endColor = c;
        }

        if (innerLine != null)
        {
            innerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
                innerLine.SetPosition(i, path[i]);

            innerLine.startWidth = innerBaseWidth;
            innerLine.endWidth = innerBaseWidth;
            Color c = new Color(innerColor.r, innerColor.g, innerColor.b, 1f);
            innerLine.startColor = c;
            innerLine.endColor = c;
        }
    }

    void UpdateHitEffects(List<Vector2> rPoints, Vector2 endPoint)
    {
        if (reflectionHitEffect != null)
        {
            while (activeReflectionEffects.Count < rPoints.Count)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(reflectionHitEffect);
                if (effect != null) activeReflectionEffects.Add(effect);
                else break;
            }

            for (int i = 0; i < rPoints.Count; i++)
            {
                if (i < activeReflectionEffects.Count && activeReflectionEffects[i] != null)
                {
                    activeReflectionEffects[i].transform.position = rPoints[i];
                    activeReflectionEffects[i].SetActive(true);
                }
            }

            for (int i = rPoints.Count; i < activeReflectionEffects.Count; i++)
            {
                if (activeReflectionEffects[i] != null)
                    activeReflectionEffects[i].SetActive(false);
            }
        }

        if (endHitEffect != null)
        {
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
        if (innerLine != null) innerLine.positionCount = 0;
        if (outerLine != null) outerLine.positionCount = 0;

        foreach (var effect in activeReflectionEffects)
            if (effect != null) effect.SetActive(false);

        if (activeEndEffect != null) activeEndEffect.SetActive(false);
    }

    public void SetAnimToSynergy()
    {
        if (isSynergyActivated) return;
        isSynergyActivated = true;
    }

    public void AddMaxReflections(int amount)
    {
        maxReflections += amount;
    }

    protected override void DieProjectile() { gameObject.SetActive(false); }
    protected override void HitObject() { gameObject.SetActive(false); }
    protected override void ApplyMovement() { }
    protected override void CastDamage() { }
}