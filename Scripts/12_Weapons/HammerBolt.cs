using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 토르 망치 스타일 지그재그 전기 볼트
/// WhipWeapon의 AttackAtHitPoint()에서 생성됩니다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class HammerBolt : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  런타임 주입 값
    // ─────────────────────────────────────────
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float duration;

    // 데미지 파라미터
    private int boltDamage;
    private float boltKnockback;
    private float boltKnockbackSpeedFactor;
    private GameObject boltHitEffect;
    private LayerMask boltEnemyLayer;

    // 중복 데미지 방지
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    // config 캐시
    private HammerBoltConfig cfg;

    // ─────────────────────────────────────────
    //  내부 변수
    // ─────────────────────────────────────────
    private LineRenderer lr;
    private LineRenderer lrGlow; // 외부 파란 선
    private float elapsed;
    private float wiggleTimer;
    private float[] baseAlphaValues;
    private float[] cachedOffsets;

    // ─────────────────────────────────────────
    //  초기화 (config 주입 후 SetupLineRenderer 호출)
    // ─────────────────────────────────────────
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

        // 외부 글로우용 LineRenderer를 자식 오브젝트에 추가
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(transform);
        lrGlow = glowObj.AddComponent<LineRenderer>();
    }

    private void SetupLineRenderer()
    {
        // ── 내부 코어 (흰색, 얇음)
        SetupSingleLR(lr, cfg.lineWidth, cfg.coreColor, sortingOrder: 11);

        // ── 외부 글로우 (파랑, 두꺼움)
        SetupSingleLR(lrGlow, cfg.lineWidth * cfg.glowWidthMultiplier, cfg.glowColor, sortingOrder: 10);
    }

    private void SetupSingleLR(LineRenderer target, float width, Color color, int sortingOrder)
    {
        target.positionCount = cfg.segmentCount + 1;
        target.startWidth = width;
        target.endWidth = width * 0.3f;
        target.useWorldSpace = true;
        target.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        target.receiveShadows = false;
        target.sortingLayerName = "Effect"; // ← 레이어 설정
        target.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
        if (shader == null) shader = Shader.Find("Particles/Additive");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        target.material = new Material(shader);
        target.material.color = color;

        // 코어: 처음부터 끝까지 흰색 유지
        // 글로우: 끝으로 갈수록 투명
        bool isCore = (sortingOrder == 11);
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(Color.white, 0f),
            new GradientColorKey(color,       0.3f),
            new GradientColorKey(color * 0.6f, 1f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(0f,                    0f),
            new GradientAlphaKey(1f,                    0.2f),
            new GradientAlphaKey(isCore ? 1f : 0.6f,    0.7f),
            new GradientAlphaKey(0f,                    1f)
            }
        );
        target.colorGradient = gradient;
    }

    // ─────────────────────────────────────────
    //  외부 진입점
    // ─────────────────────────────────────────
    public void SpawnBolt(
        HammerBoltConfig config,
        Vector3 start,
        Vector3 end,
        float boltDuration,
        int damage,
        float knockback,
        float knockbackSpeedFactor,
        GameObject hitEffect,
        LayerMask enemyLayer)
    {
        cfg = config;

        // config 주입 후 LineRenderer 세팅
        SetupLineRenderer();

        startPoint = start;
        endPoint = end;
        duration = boltDuration;
        elapsed = 0f;
        wiggleTimer = 0f;

        boltDamage = damage;
        boltKnockback = knockback;
        boltKnockbackSpeedFactor = knockbackSpeedFactor;
        boltHitEffect = hitEffect;
        boltEnemyLayer = enemyLayer;

        hitEnemies.Clear();

        // 데미지에 따라 굵기 조정
        float damageScale = Mathf.Clamp(1f + (damage - 10) * 0.02f, 1f, 3f);

        // 코어
        lr.startWidth = cfg.lineWidth * damageScale;
        lr.endWidth = cfg.lineWidth * damageScale * 0.3f;

        // 글로우 ← 이 부분 추가
        lrGlow.startWidth = cfg.lineWidth * cfg.glowWidthMultiplier * damageScale;
        lrGlow.endWidth = cfg.lineWidth * cfg.glowWidthMultiplier * damageScale * 0.3f;

        cachedOffsets = new float[cfg.segmentCount + 1];
        RefreshCachedOffsets();

        StartCoroutine(AnimateBolt());
    }

    // ─────────────────────────────────────────
    //  코어 로직
    // ─────────────────────────────────────────
    private void RefreshCachedOffsets()
    {
        for (int i = 0; i <= cfg.segmentCount; i++)
            cachedOffsets[i] = Random.Range(-cfg.wiggleAmount, cfg.wiggleAmount);
    }

    private void RebuildTravelingPoints(float tailT, float headT)
    {
        // 기존 lr 업데이트 로직 동일하게 lrGlow에도 적용
        RebuildSingleLR(lr, tailT, headT);
        RebuildSingleLR(lrGlow, tailT, headT);
    }

    private void RebuildSingleLR(LineRenderer target, float tailT, float headT)
    {
        Vector3 boltDir = endPoint - startPoint;
        float totalLength = boltDir.magnitude;
        Vector3 forward = boltDir.normalized;
        Vector3 perp = new Vector3(-forward.y, forward.x, 0f);

        target.positionCount = cfg.segmentCount + 1;

        for (int i = 0; i <= cfg.segmentCount; i++)
        {
            float localT = (float)i / cfg.segmentCount;
            float worldT = tailT + (headT - tailT) * localT;
            Vector3 basePos = startPoint + forward * (totalLength * worldT);
            float envelope = Mathf.Sin(localT * Mathf.PI);
            float offset = cachedOffsets[i] * envelope;

            target.SetPosition(i, basePos + perp * offset);
        }
    }

    private void CheckDamageAlongBolt(float tailT, float headT)
    {
        Vector3 boltDir = endPoint - startPoint;
        float totalLength = boltDir.magnitude;
        Vector3 forward = boltDir.normalized;

        Vector3 tailPos = startPoint + forward * (totalLength * tailT);
        Vector3 headPos = startPoint + forward * (totalLength * headT);
        Vector3 segDir = headPos - tailPos;
        float segLen = segDir.magnitude;

        if (segLen < 0.01f) return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            tailPos,
            cfg.damageCheckRadius,
            segDir.normalized,
            segLen,
            boltEnemyLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hitEnemies.Contains(hit.collider)) continue;

            if (hit.collider.CompareTag("Enemy"))
            {
                Idamageable target = hit.collider.GetComponent<Idamageable>();
                if (target != null)
                {
                    target.TakeDamage(
                        boltDamage,
                        boltKnockback,
                        boltKnockbackSpeedFactor,
                        hit.point,
                        boltHitEffect
                    );
                    hitEnemies.Add(hit.collider);
                }
            }
        }
    }

    private IEnumerator AnimateBolt()
    {
        float travelDuration = duration * cfg.travelRatio;
        float fadeDuration = duration * (1f - cfg.travelRatio);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            wiggleTimer += Time.deltaTime;

            if (wiggleTimer >= cfg.wiggleInterval)
            {
                wiggleTimer = 0f;
                RefreshCachedOffsets();
            }

            float headT, tailT, alpha;

            if (elapsed < travelDuration)
            {
                float progress = elapsed / travelDuration;
                float center = Mathf.Lerp(cfg.windowSize * 0.5f, 1f - cfg.windowSize * 0.5f, progress);
                headT = Mathf.Clamp01(center + cfg.windowSize * 0.5f);
                tailT = Mathf.Clamp01(center - cfg.windowSize * 0.5f);
                alpha = 1f;

                CheckDamageAlongBolt(tailT, headT);
            }
            else
            {
                float fadeRatio = (elapsed - travelDuration) / fadeDuration;
                float tailStart = 1f - cfg.windowSize;
                headT = 1f;
                tailT = Mathf.Lerp(tailStart, 1f, fadeRatio);
                alpha = 1f - Mathf.Clamp01(fadeRatio);
            }

            RebuildTravelingPoints(tailT, headT);
            SetAlpha(alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        ApplyAlpha(lr, alpha);
        ApplyAlpha(lrGlow, alpha);
    }

    private void ApplyAlpha(LineRenderer target, float alpha)
    {
        Gradient g = target.colorGradient;
        GradientAlphaKey[] keys = g.alphaKeys;

        for (int i = 0; i < keys.Length; i++)
            keys[i].alpha = keys[i].alpha * alpha; // 현재값 기준 (Gradient는 매번 새로 가져옴)

        g.alphaKeys = keys;
        target.colorGradient = g;
    }

    // ─────────────────────────────────────────
    //  팩토리 메서드
    // ─────────────────────────────────────────
    public static HammerBolt Create(
        HammerBoltConfig config,
        Vector3 start,
        Vector3 end,
        float duration,
        int damage,
        float knockback,
        float knockbackSpeedFactor,
        GameObject hitEffect,
        LayerMask enemyLayer)
    {
        GameObject go = new GameObject("HammerBolt");
        HammerBolt bolt = go.AddComponent<HammerBolt>();
        bolt.SpawnBolt(config, start, end, duration, damage,
                       knockback, knockbackSpeedFactor, hitEffect, enemyLayer);
        return bolt;
    }

    // ─────────────────────────────────────────
    //  에디터 디버그
    // ─────────────────────────────────────────
#if UNITY_EDITOR
    private float debugTailT;
    private float debugHeadT;

    private void OnDrawGizmos()
    {
        if (cfg == null || startPoint == endPoint) return;

        Vector3 boltDir = endPoint - startPoint;
        float totalLength = boltDir.magnitude;
        Vector3 forward = boltDir.normalized;

        Vector3 tailPos = startPoint + forward * (totalLength * debugTailT);
        Vector3 headPos = startPoint + forward * (totalLength * debugHeadT);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(tailPos, cfg.damageCheckRadius);
        Gizmos.DrawSphere(headPos, cfg.damageCheckRadius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawLine(tailPos, headPos);
    }
#endif
}