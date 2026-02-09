using UnityEngine;
using System.Collections.Generic;

public class ZapProjectile : ProjectileBase
{
    [Header("Zap Settings")]
    [SerializeField] float switchTargetTime = 0.3f; // 타겟 전환 주기
    [SerializeField] float damageInterval = 0.5f; // 데미지 주기 (초 단위)
    [SerializeField] float maxTargetDistance = 15f; // 최대 타겟 거리
    [SerializeField] float switchTargetTimeSynergy = 0.3f; // 타겟 전환 주기
    [SerializeField] float damageIntervalSynergy = 0.5f; // 데미지 주기 (초 단위)

    [Header("Visual")]
    [SerializeField] LineRenderer laserLine;
    [SerializeField] Transform hitEffect;
    [SerializeField] float baseWidth = 0.1f; // 기본 굵기
    [SerializeField] float maxWidth = 0.5f; //최대 굵기
    [SerializeField] float baseDamage = 4f; //기준 데미지
    [SerializeField] Color lightColor = new Color(0.5f, 0.8f, 1f); //추가
    [SerializeField] Color darkColor = new Color(0f, 0f, 1f); // 추가

    [Header("Layers")]
    [SerializeField] LayerMask destructables;

    [Header("Audio")]
    [SerializeField] AudioClip targetSwitchSound;

    Transform currentTarget; // 현재 타겟
    float switchTimer; // 타겟 전환 타이머
    float damageTimer; // ✅ 데미지 타이머
    bool isSynergyActivated;

    void OnEnable()
    {
        // 타이머 초기화
        switchTimer = 0f;
        damageTimer = 0f; // ✅ 추가
        
        // 첫 타겟 검색
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

        // 타겟 유효성 강화 체크
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

        // 타겟이 있으면 레이저 그리기 및 데미지
        if (currentTarget != null)
        {
            DrawLaser();
            DealDamage();
        }
        else
        {
            // 타겟이 없으면 레이저 숨기기
            HideLaser();
        }
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null)
            return false;
        
        if (!target.gameObject.activeInHierarchy)
            return false;
        
        // Idamageable 컴포넌트가 있는지 체크
        Idamageable damageable = target.GetComponent<Idamageable>();
        if (damageable == null)
            return false;
        
        // 화면 밖이면 무효
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible)
            return false;
        
        return true;
    }

    void FindNewTarget()
    {
        // 랜덤하게 가까운 적 중 하나 선택
        List<Vector2> nearbyEnemies = EnemyFinder.instance.GetEnemies(5);
        
        if (nearbyEnemies == null || nearbyEnemies.Count == 0)
        {
            currentTarget = null;
            return;
        }
        
        // Vector2.zero가 아닌 실제 적들만 필터링
        List<Vector2> validEnemies = new List<Vector2>();
        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i] != Vector2.zero)
            {
                validEnemies.Add(nearbyEnemies[i]);
            }
        }
        
        if (validEnemies.Count == 0)
        {
            currentTarget = null;
            return;
        }
        
        // 랜덤하게 하나 선택
        Vector2 randomEnemy = validEnemies[Random.Range(0, validEnemies.Count)];
        
        // 거리 체크
        float distance = Vector2.Distance(transform.position, randomEnemy);
        if (distance > maxTargetDistance)
        {
            currentTarget = null;
            return;
        }
        
        // 해당 위치의 적 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(randomEnemy, 0.5f, destructables);
        
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponent<Idamageable>() != null)
            {
                // 화면 내에 있는 적만 선택
                SpriteRenderer sr = hits[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null && !sr.isVisible)
                    continue; // 화면 밖이면 스킵
                
                currentTarget = hits[i].transform;
                
                // 새 타겟을 찾았을 때 사운드 재생
                if (targetSwitchSound != null)
                {
                    SoundManager.instance.PlaySoundWith(targetSwitchSound, 0.7f, true, 0);
                }
                
                return;
            }
        }
        
        currentTarget = null;
    }

    void DrawLaser()
    {
        if (laserLine == null || currentTarget == null)
            return;

        // 매번 최신 ShootPoint 위치 가져오기
        WeaponBase weapon = GetComponentInParent<WeaponBase>();
        if (weapon == null || weapon.ShootPoint == null)
            return;

        // 시작점과 끝점 설정
        Vector2 startPos = weapon.ShootPoint.position;
        Vector2 endPos = currentTarget.position;

        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);

        // 데미지에 비례하여 굵기 조절
        float damageRatio = Mathf.Clamp(Damage / baseDamage, 1f, maxWidth / baseWidth);
        laserLine.widthMultiplier = baseWidth * damageRatio;

        // Inspector에서 설정한 색상 사용
        float colorT = Mathf.Clamp01((damageRatio - 1f) / 4f);
        Color laserColor = Color.Lerp(lightColor, darkColor, colorT);

        laserLine.startColor = new Color(laserColor.r, laserColor.g, laserColor.b, 1);
        laserLine.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 1);

        // 레이저 보이기
        laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 1);
        laserLine.endColor = new Color(laserLine.endColor.r, laserLine.endColor.g, laserLine.endColor.b, 1);

        // Hit Effect 위치
        if (hitEffect != null)
        {
            hitEffect.position = endPos;
            hitEffect.gameObject.SetActive(true);
        }
    }

    void HideLaser()
    {
        if (laserLine != null)
        {
            laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 0);
            laserLine.endColor = new Color(laserLine.endColor.r, laserLine.endColor.g, laserLine.endColor.b, 0);
        }
        
        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(false);
        }
    }

    void DealDamage()
    {
        if (currentTarget == null) return;

        // ✅ 시간 기반 데미지 - 모든 기기에서 동일
        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval)
            return;
        
        damageTimer = 0f; // 리셋

        Idamageable damageable = currentTarget.GetComponent<Idamageable>();
        if (damageable == null)
            return;

        // 카메라 밖이면 데미지 전달 안함
        SpriteRenderer spriteRenderer = currentTarget.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.isVisible)
            return;

        // 데미지 처리
        PostMessage(Damage, currentTarget.position);
        GameObject hitEffectObj = GetComponent<HitEffects>()?.hitEffect;
        if (hitEffectObj != null)
        {
            hitEffectObj.transform.position = currentTarget.position;
        }

        damageable.TakeDamage(Damage, 
                             KnockBackChance, 
                             KnockBackSpeedFactor, 
                             currentTarget.position, 
                             hitEffectObj);

        // 데미지 트래커 기록
        if (!string.IsNullOrEmpty(WeaponName))
        {
            DamageTracker.instance.RecordDamage(WeaponName, Damage);
        }
    }

    public void SetAnimToSynergy()
    {
        // 시너지 모드: 더 빠른 타겟 전환 및 데미지
        switchTargetTime = switchTargetTimeSynergy;
        damageInterval = damageIntervalSynergy; // ✅ 시너지: 더 빠른 데미지
        isSynergyActivated = true;
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