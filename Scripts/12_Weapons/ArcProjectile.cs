using System.Collections.Generic;
using UnityEngine;

public class ArcProjectile : ProjectileBase
{
    [Header("Arc Settings")]
    [SerializeField] float maxDistance = 100f;
    [SerializeField] int maxReflections = 1; // ✅ 이 값만 사용
    [SerializeField] float damageDecayPerReflection = 0.15f;
    [SerializeField] float synergyDamageDecay = 0.1f; // 시너지 시 데미지 감소율만 다름

    [Header("Visual - Dual LineRenderer")]
    [SerializeField] LineRenderer innerLine;
    [SerializeField] LineRenderer outerLine;
    [SerializeField] Transform hitEffect;
    
    [Header("Inner Line (Core)")]
    [SerializeField] float innerBaseWidth = 0.15f;
    [SerializeField] float innerMaxWidth = 0.5f;
    [SerializeField] Color innerColor = new Color(1f, 1f, 0.8f);

    [Header("Outer Line (Glow)")]
    [SerializeField] float outerBaseWidth = 0.4f;
    [SerializeField] float outerMaxWidth = 1.2f;
    [SerializeField] Color outerColor = new Color(1f, 0.5f, 0f);
    
    [Header("Common Settings")]
    [SerializeField] float baseDamage = 4f;
    [SerializeField] float alphaDecayPerReflection = 0.1f;
    [SerializeField] float widthOscillationSpeed = 10f;
    [SerializeField] float widthOscillationAmount = 0.1f;

    [Header("Layers")]
    [SerializeField] LayerMask destructables;
    [SerializeField] LayerMask walls;
    [SerializeField] LayerMask screenEdges;

    bool isSynergyActivated; // ✅ 데미지 감소율에만 사용
    WeaponBase cachedWeapon;

    HashSet<Collider2D> damagedThisFrame = new HashSet<Collider2D>();
    int frameCount = 7;

    void OnEnable()
    {
        if (cachedWeapon == null)
        {
            cachedWeapon = GetComponentInParent<WeaponBase>();
        }

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

        if (Time.frameCount % frameCount == 0)
        {
            damagedThisFrame.Clear();
        }
    }

    void DrawAndCastLaser()
    {
        if ((innerLine == null && outerLine == null) || cachedWeapon == null || cachedWeapon.ShootPoint == null)
            return;

        Vector2 startPos = cachedWeapon.ShootPoint.position;
        Vector2 direction = transform.up;

        List<Vector2> laserPath = new List<Vector2>();
        laserPath.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentDir = direction;
        int reflectionsUsed = 0;
        float currentDamageMultiplier = 1f;

        // ✅ maxReflections만 사용
        int maxIterations = maxReflections + 1;
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

            // ✅ maxReflections와 비교
            if (foundWall && reflectionsUsed < maxReflections)
            {
                currentDir = Vector2.Reflect(currentDir, closestWallHit.normal);
                currentPos = closestWallHit.point + closestWallHit.normal * 0.01f;

                reflectionsUsed++;

                // ✅ 시너지는 데미지 감소율에만 영향
                float decayRate = isSynergyActivated ? synergyDamageDecay : damageDecayPerReflection;
                currentDamageMultiplier *= (1f - decayRate);
            }
            else
            {
                break;
            }
        }

        DrawLaserPath(laserPath, reflectionsUsed);
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

        float damageRatio = Mathf.Clamp(Damage / baseDamage, 1f, outerMaxWidth / outerBaseWidth);
        float oscillation = 1f + Mathf.Sin(Time.time * widthOscillationSpeed) * widthOscillationAmount;
        float alpha = 1f - reflectionsUsed * alphaDecayPerReflection;
        alpha = Mathf.Max(alpha, 0.3f);

        // Inner Line
        if (innerLine != null)
        {
            innerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                innerLine.SetPosition(i, path[i]);
            }

            float innerWidth = innerBaseWidth * damageRatio * oscillation;
            innerLine.widthMultiplier = innerWidth;

            Color currentInnerColor = new Color(
                innerColor.r, 
                innerColor.g, 
                innerColor.b, 
                Mathf.Max(alpha, 0.8f)
            );
            innerLine.startColor = currentInnerColor;
            innerLine.endColor = currentInnerColor;
        }

        // Outer Line
        if (outerLine != null)
        {
            outerLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                outerLine.SetPosition(i, path[i]);
            }

            float outerWidth = outerBaseWidth * damageRatio * oscillation;
            outerLine.widthMultiplier = outerWidth;

            Color currentOuterColor = new Color(
                outerColor.r, 
                outerColor.g, 
                outerColor.b, 
                alpha * 0.6f
            );
            outerLine.startColor = currentOuterColor;
            outerLine.endColor = currentOuterColor;
        }

        if (hitEffect != null && path.Count > 0)
        {
            hitEffect.position = path[path.Count - 1];
            hitEffect.gameObject.SetActive(true);
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

        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(false);
        }
    }

    // ✅ 시너지는 데미지 감소율만 변경
    public void SetAnimToSynergy()
    {
        isSynergyActivated = true;
        // maxReflections는 변경하지 않음
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