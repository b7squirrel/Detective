using UnityEngine;
using System.Collections.Generic;

public class ZapProjectile : ProjectileBase
{
    [Header("Zap Settings")]
    [SerializeField] float switchTargetTime = 0.3f;         // 타겟 전환 주기
    [SerializeField] float damageInterval = 0.5f;           // 데미지 주기 (초 단위)
    [SerializeField] float maxTargetDistance = 15f;         // 최대 타겟 거리
    [SerializeField] float switchTargetTimeSynergy = 0.3f;  // 시너지 타겟 전환 주기
    [SerializeField] float damageIntervalSynergy = 0.5f;    // 시너지 데미지 주기
    Transform assignedMuzzlePoint;

    [Header("Visual")]
    [SerializeField] LineRenderer laserLineOuter;           // 외곽 레이저 (굵고 어두움)
    [SerializeField] LineRenderer laserLineInner;           // 중앙 레이저 (가늘고 밝음)
    [SerializeField] Transform hitEffect;
    [SerializeField] float baseWidth = 0.1f;
    [SerializeField] float maxWidth = 0.5f;
    [SerializeField] float baseDamage = 4f;
    [SerializeField] Color lightColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] Color darkColor = new Color(0f, 0f, 1f);
    [SerializeField] float widthOscillationSpeed = 20f;     // 진동 속도
    [SerializeField] float widthOscillationAmount = 0.15f;  // 진동 폭 (0~1)
    [SerializeField] float innerWidthRatio = 0.4f;          // 내부 레이저 굵기 비율

    [Header("Layers")]
    [SerializeField] LayerMask destructables;

    [Header("Audio")]
    [SerializeField] AudioClip targetSwitchSound;

    Transform currentTarget;
    Vector2 cachedTargetPoint; // 타겟의 랜덤 히트 포인트 캐싱
    float switchTimer;
    float damageTimer;
    bool isSynergyActivated;
    WeaponBase cachedWeapon; // GetComponentInParent 반복 방지

    // FindNewTarget에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(5);
    private Collider2D[] overlapBuffer = new Collider2D[10]; // OverlapCircleAll 대신 NonAlloc 버퍼

    void OnEnable()
    {
        if (cachedWeapon == null)
        {
            cachedWeapon = GetComponentInParent<WeaponBase>();
        }

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

        // 타겟 유효성 체크
        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
            FindNewTarget();
        }

        // 타겟 전환 타이머
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

        // 화면 밖이면 무효
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible) return false;

        return true;
    }

    void FindNewTarget()
    {
        // 버퍼 재사용으로 new List 방지
        EnemyFinder.instance.GetEnemies(5, enemyQueryBuffer);

        // 유효한 적(Vector2.zero가 아닌) 중에서 랜덤 선택
        int validCount = 0;
        for (int i = 0; i < enemyQueryBuffer.Count; i++)
        {
            if (enemyQueryBuffer[i] != Vector2.zero)
                validCount++;
        }

        if (validCount == 0)
        {
            currentTarget = null;
            return;
        }

        // 유효한 적 중 랜덤 선택
        int randomValidIndex = Random.Range(0, validCount);
        Vector2 randomEnemy = Vector2.zero;
        int counted = 0;
        for (int i = 0; i < enemyQueryBuffer.Count; i++)
        {
            if (enemyQueryBuffer[i] != Vector2.zero)
            {
                if (counted == randomValidIndex)
                {
                    randomEnemy = enemyQueryBuffer[i];
                    break;
                }
                counted++;
            }
        }

        // 거리 체크
        if (Vector2.Distance(transform.position, randomEnemy) > maxTargetDistance)
        {
            currentTarget = null;
            return;
        }

        // NonAlloc으로 버퍼 재사용 (OverlapCircleAll 대신)
        int hitCount = Physics2D.OverlapCircleNonAlloc(randomEnemy, 0.5f, overlapBuffer, destructables);

        for (int i = 0; i < hitCount; i++)
        {
            if (overlapBuffer[i].GetComponent<Idamageable>() == null) continue;

            // 화면 밖이면 스킵
            SpriteRenderer sr = overlapBuffer[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null && !sr.isVisible) continue;

            currentTarget = overlapBuffer[i].transform;

            // 서브보스/스테이지보스면 랜덤 바디 포인트, 일반 적은 position
            EnemyBase enemyBase = overlapBuffer[i].GetComponent<EnemyBase>();
            cachedTargetPoint = enemyBase != null
                ? enemyBase.GetRandomBodyPoint()
                : (Vector2)currentTarget.position;

            if (targetSwitchSound != null)
            {
                SoundManager.instance.PlaySoundWith(targetSwitchSound, 0.7f, true, 0);
            }

            return;
        }

        currentTarget = null;
    }

    void DrawLaser()
    {
        if ((laserLineOuter == null && laserLineInner == null) || currentTarget == null)
            return;

        if (cachedWeapon == null || cachedWeapon.ShootPoint == null)
            return;

        // assignedMuzzlePoint가 있으면 그걸 사용, 없으면 ShootPoint 폴백
        Transform originPoint = (assignedMuzzlePoint != null) ? assignedMuzzlePoint : cachedWeapon.ShootPoint;
        if (originPoint == null) return;

        Vector2 startPos = originPoint.position;
        Vector2 endPos = cachedTargetPoint;

        // 데미지에 비례하여 기본 굵기 조절
        float damageRatio = Mathf.Clamp(Damage / baseDamage, 1f, maxWidth / baseWidth);
        float targetWidth = baseWidth * damageRatio;

        // 진동 효과
        float oscillation = 1f + Mathf.Sin(Time.time * widthOscillationSpeed) * widthOscillationAmount;

        // 색상 계산
        float colorT = Mathf.Clamp01((damageRatio - 1f) / 4f);
        Color outerColor = Color.Lerp(lightColor, darkColor, colorT);
        Color innerColor = Color.Lerp(Color.white, outerColor, 0.1f); // 흰색 90%와 outer color 10% 혼합

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
        {
            hitEffect.gameObject.SetActive(false);
        }
    }

    void DealDamage()
    {
        if (currentTarget == null) return;

        // 시간 기반 데미지 — 모든 기기에서 동일한 주기
        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval) return;
        damageTimer = 0f;

        Idamageable damageable = currentTarget.GetComponent<Idamageable>();
        if (damageable == null) return;

        // 화면 밖이면 데미지 전달 안 함
        SpriteRenderer spriteRenderer = currentTarget.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible) return;

        PostMessage(Damage, cachedTargetPoint);

        GameObject hitEffectObj = GetComponent<HitEffects>()?.hitEffect;
        if (hitEffectObj != null)
        {
            hitEffectObj.transform.position = cachedTargetPoint;
        }

        damageable.TakeDamage(Damage,
                              KnockBackChance,
                              KnockBackSpeedFactor,
                              cachedTargetPoint,
                              hitEffectObj);

        if (!string.IsNullOrEmpty(WeaponName))
        {
            DamageTracker.instance.RecordDamage(WeaponName, Damage);
        }
    }

    public void SetAnimToSynergy()
    {
        // 시너지 모드: 더 빠른 타겟 전환 및 데미지
        switchTargetTime = switchTargetTimeSynergy;
        damageInterval = damageIntervalSynergy;
        isSynergyActivated = true;
    }

    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        assignedMuzzlePoint = muzzlePoint;
    }

    protected override void DieProjectile()
    {
        gameObject.SetActive(false);
    }

    protected override void HitObject()
    {
        gameObject.SetActive(false);
    }
}