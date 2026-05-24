using UnityEngine;
using System.Collections.Generic;

public class ZapProjectile : ProjectileBase
{
    [Header("Zap Settings")]
    [SerializeField] float switchTargetTime = 0.3f;
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] float maxTargetDistance = 15f;
    [SerializeField] float switchTargetTimeSynergy = 0.3f;
    [SerializeField] float damageIntervalSynergy = 0.5f;
    Transform assignedMuzzlePoint;

    [Header("Visual")]
    [SerializeField] LineRenderer laserLineOuter;
    [SerializeField] LineRenderer laserLineInner;
    [SerializeField] Transform hitEffect;
    [SerializeField] float baseWidth = 0.1f;
    [SerializeField] float maxWidth = 0.5f;
    [SerializeField] float baseDamage = 4f;
    [SerializeField] Color lightColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] Color darkColor = new Color(0f, 0f, 1f);
    [SerializeField] float widthOscillationSpeed = 20f;
    [SerializeField] float widthOscillationAmount = 0.15f;
    [SerializeField] float innerWidthRatio = 0.4f;

    [Header("Layers")]
    [SerializeField] LayerMask destructables;

    [Header("Audio")]
    [SerializeField] AudioClip targetSwitchSound;

    Transform currentTarget;
    Vector2 cachedTargetPoint;
    float switchTimer;
    float damageTimer;
    bool isSynergyActivated;
    WeaponBase cachedWeapon;

    // ✅ 필드 버퍼 재사용 (new List 방지)
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(5);

    // ✅ static으로 모든 ZapProjectile 인스턴스가 공유 (메모리 절약)
    static readonly Collider2D[] zapOverlapBuffer = new Collider2D[10];

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱
    }

    void OnEnable()
    {
        if (cachedWeapon == null)
            cachedWeapon = GetComponentInParent<WeaponBase>();

        switchTimer = 0f;
        damageTimer = 0f;
        FindNewTarget();
    }

    void OnDisable()
    {
        HideLaser();
        currentTarget = null;
    }

    protected override void Update()
    {
        if (Time.timeScale == 0) return;

        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
            FindNewTarget();
        }

        switchTimer += Time.deltaTime;
        if (switchTimer >= switchTargetTime)
        {
            switchTimer = 0f;
            FindNewTarget();
        }

        if (currentTarget != null)
        {
            DrawLaser();
            DealDamage();
        }
        else
        {
            HideLaser();
        }
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;
        if (target.GetComponent<Idamageable>() == null) return false;

        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible) return false;

        return true;
    }

    void FindNewTarget()
    {
        EnemyFinder.instance.GetEnemies(5, enemyQueryBuffer);

        int validCount = 0;
        for (int i = 0; i < enemyQueryBuffer.Count; i++)
        {
            if (enemyQueryBuffer[i] != Vector2.zero)
                validCount++;
        }

        if (validCount == 0) { currentTarget = null; return; }

        int randomValidIndex = Random.Range(0, validCount);
        Vector2 randomEnemy = Vector2.zero;
        int counted = 0;
        for (int i = 0; i < enemyQueryBuffer.Count; i++)
        {
            if (enemyQueryBuffer[i] != Vector2.zero)
            {
                if (counted == randomValidIndex) { randomEnemy = enemyQueryBuffer[i]; break; }
                counted++;
            }
        }

        if (Vector2.Distance(transform.position, randomEnemy) > maxTargetDistance)
        {
            currentTarget = null;
            return;
        }

        // ✅ static 버퍼 재사용
        int hitCount = Physics2D.OverlapCircleNonAlloc(randomEnemy, 0.5f, zapOverlapBuffer, destructables);

        for (int i = 0; i < hitCount; i++)
        {
            if (zapOverlapBuffer[i].GetComponent<Idamageable>() == null) continue;

            SpriteRenderer sr = zapOverlapBuffer[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null && !sr.isVisible) continue;

            currentTarget = zapOverlapBuffer[i].transform;

            EnemyBase enemyBase = zapOverlapBuffer[i].GetComponent<EnemyBase>();
            cachedTargetPoint = enemyBase != null
                ? enemyBase.GetRandomBodyPoint()
                : (Vector2)currentTarget.position;

            if (targetSwitchSound != null)
                SoundManager.instance.PlaySoundWith(targetSwitchSound, 0.7f, true, 0);

            return;
        }

        currentTarget = null;
    }

    void DrawLaser()
    {
        if (laserLineOuter == null && laserLineInner == null) return;
        if (currentTarget == null) return;
        if (cachedWeapon == null || cachedWeapon.ShootPoint == null) return;

        Transform originPoint = assignedMuzzlePoint != null ? assignedMuzzlePoint : cachedWeapon.ShootPoint;
        if (originPoint == null) return;

        Vector2 startPos = originPoint.position;
        Vector2 endPos = cachedTargetPoint;

        float damageRatio = Mathf.Clamp(Damage / baseDamage, 1f, maxWidth / baseWidth);
        float targetWidth = baseWidth * damageRatio;
        float oscillation = 1f + Mathf.Sin(Time.time * widthOscillationSpeed) * widthOscillationAmount;

        float colorT = Mathf.Clamp01((damageRatio - 1f) / 4f);
        Color outerColor = Color.Lerp(lightColor, darkColor, colorT);
        Color innerColor = Color.Lerp(Color.white, outerColor, 0.1f);

        if (laserLineOuter != null)
        {
            laserLineOuter.SetPosition(0, startPos);
            laserLineOuter.SetPosition(1, endPos);
            laserLineOuter.widthMultiplier = targetWidth * oscillation;
            laserLineOuter.startColor = new Color(outerColor.r, outerColor.g, outerColor.b, 0.6f);
            laserLineOuter.endColor = new Color(outerColor.r, outerColor.g, outerColor.b, 0.6f);
        }

        if (laserLineInner != null)
        {
            laserLineInner.SetPosition(0, startPos);
            laserLineInner.SetPosition(1, endPos);
            laserLineInner.widthMultiplier = targetWidth * oscillation * innerWidthRatio;
            laserLineInner.startColor = new Color(innerColor.r, innerColor.g, innerColor.b, 1f);
            laserLineInner.endColor = new Color(innerColor.r, innerColor.g, innerColor.b, 1f);
        }

        if (hitEffect != null)
        {
            hitEffect.position = endPos;
            hitEffect.gameObject.SetActive(true);
        }
    }

    void HideLaser()
    {
        if (laserLineOuter != null)
        {
            laserLineOuter.startColor = new Color(laserLineOuter.startColor.r, laserLineOuter.startColor.g, laserLineOuter.startColor.b, 0);
            laserLineOuter.endColor = new Color(laserLineOuter.endColor.r, laserLineOuter.endColor.g, laserLineOuter.endColor.b, 0);
        }

        if (laserLineInner != null)
        {
            laserLineInner.startColor = new Color(laserLineInner.startColor.r, laserLineInner.startColor.g, laserLineInner.startColor.b, 0);
            laserLineInner.endColor = new Color(laserLineInner.endColor.r, laserLineInner.endColor.g, laserLineInner.endColor.b, 0);
        }

        if (hitEffect != null)
            hitEffect.gameObject.SetActive(false);
    }

    void DealDamage()
    {
        if (currentTarget == null) return;

        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval) return;
        damageTimer = 0f;

        Idamageable damageable = currentTarget.GetComponent<Idamageable>();
        if (damageable == null) return;

        SpriteRenderer spriteRenderer = currentTarget.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible) return;

        PostMessage(Damage, cachedTargetPoint);

        // ✅ 캐싱된 hitEffects 사용 (매 데미지마다 GetComponent 제거)
        GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : null;
        if (hitEffectObj != null)
            hitEffectObj.transform.position = cachedTargetPoint;

        damageable.TakeDamage(Damage, KnockBackChance, KnockBackSpeedFactor, cachedTargetPoint, hitEffectObj);

        if (!string.IsNullOrEmpty(WeaponName))
            DamageTracker.instance.RecordDamage(WeaponName, Damage);
    }

    public void SetAnimToSynergy()
    {
        switchTargetTime = switchTargetTimeSynergy;
        damageInterval = damageIntervalSynergy;
        isSynergyActivated = true;
    }

    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        assignedMuzzlePoint = muzzlePoint;
    }

    protected override void DieProjectile() { gameObject.SetActive(false); }
    protected override void HitObject() { gameObject.SetActive(false); }
}