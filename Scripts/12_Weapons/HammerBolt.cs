using System.Collections;
using UnityEngine;

/// <summary>
/// 토르 망치 스타일 지그재그 전기 볼트
/// WhipWeapon의 AttackAtHitPoint()에서 생성됩니다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class HammerBolt : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  설정값 (SpawnBolt()로 외부에서 주입)
    // ─────────────────────────────────────────
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float duration = 0.5f;

    // ─────────────────────────────────────────
    //  내부 파라미터 (Inspector에서 조정 가능)
    // ─────────────────────────────────────────
    [Header("볼트 모양")]
    [Tooltip("선을 나누는 segment 수 (많을수록 복잡한 지그재그)")]
    [SerializeField] private int segmentCount = 20;

    [Tooltip("수직 흔들림 최대 폭")]
    [SerializeField] private float wiggleAmount = 1f;

    [Tooltip("지지직 갱신 주기(초) - 작을수록 빠르게 떨림")]
    [SerializeField] private float wiggleInterval = 0.04f;

    [Header("비주얼")]
    [SerializeField] private float lineWidth = 0.07f;
    [SerializeField] private Color boltColor = new Color(0.5f, 0.8f, 1f, 1f);

    [Header("이동 효과")]
    [Tooltip("전체 길이 중 보이는 윈도우 비율 (0.35 = 35%)")]
    [SerializeField] private float windowSize = 0.35f;

    [Tooltip("이동 구간 비율 (전체 duration 중 이동에 쓰는 비율)")]
    [SerializeField] private float travelRatio = 1.3f;

    // ─────────────────────────────────────────
    //  내부 변수
    // ─────────────────────────────────────────
    private LineRenderer lr;
    private float elapsed;
    private float wiggleTimer;
    private float[] baseAlphaValues;

    // ⚡ wiggle 오프셋 캐시 - 주기마다만 갱신해서 지지직 효과
    private float[] cachedOffsets;

    // ─────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lr.positionCount     = segmentCount + 1;
        lr.startWidth        = lineWidth;
        lr.endWidth          = lineWidth * 0.3f;
        lr.useWorldSpace     = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;
        lr.sortingOrder      = 10;

        Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
        if (shader == null) shader = Shader.Find("Particles/Additive");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        lr.material       = new Material(shader);
        lr.material.color = boltColor;

        SetupGradient();
    }

    private void SetupGradient()
    {
        // 꼬리 → 투명, 앞부분 → 밝게, 머리 → 다시 투명 (뾰족한 느낌)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(0f,  0f),
            new GradientAlphaKey(1f,  0.2f),
            new GradientAlphaKey(1f,  0.7f),
            new GradientAlphaKey(0f,  1f)
        };

        baseAlphaValues = new float[alphaKeys.Length];
        for (int i = 0; i < alphaKeys.Length; i++)
            baseAlphaValues[i] = alphaKeys[i].alpha;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(boltColor * 0.3f, 0f),
                new GradientColorKey(Color.white,      0.2f),
                new GradientColorKey(boltColor,        0.7f),
                new GradientColorKey(boltColor * 0.5f, 1f)
            },
            alphaKeys
        );
        lr.colorGradient = gradient;
    }

    // ─────────────────────────────────────────
    //  외부 진입점
    // ─────────────────────────────────────────
    public void SpawnBolt(Vector3 start, Vector3 end, float boltDuration = 0.5f, int damage = 10)
    {
        startPoint  = start;
        endPoint    = end;
        duration    = boltDuration;
        elapsed     = 0f;
        wiggleTimer = 0f;

        // 데미지에 따라 굵기 조정
        float damageScale = Mathf.Clamp(1f + (damage - 10) * 0.02f, 1f, 3f);
        lr.startWidth = lineWidth * damageScale;
        lr.endWidth   = lineWidth * damageScale * 0.3f;

        // 오프셋 캐시 초기화
        cachedOffsets = new float[segmentCount + 1];
        RefreshCachedOffsets();

        StartCoroutine(AnimateBolt());
    }

    // ─────────────────────────────────────────
    //  코어 로직
    // ─────────────────────────────────────────

    /// <summary>
    /// wiggle 오프셋을 새로 랜덤 생성합니다. (wiggleInterval마다 호출)
    /// </summary>
    private void RefreshCachedOffsets()
    {
        for (int i = 0; i <= segmentCount; i++)
        {
            cachedOffsets[i] = Random.Range(-wiggleAmount, wiggleAmount);
        }
    }

    /// <summary>
    /// tailT ~ headT 구간의 볼트를 그립니다.
    /// tailT, headT 모두 0~1 (전체 볼트 길이 기준)
    /// </summary>
    private void RebuildTravelingPoints(float tailT, float headT)
    {
        Vector3 boltDir     = endPoint - startPoint;
        float   totalLength = boltDir.magnitude;
        Vector3 forward     = boltDir.normalized;
        Vector3 perp        = new Vector3(-forward.y, forward.x, 0f);

        lr.positionCount = segmentCount + 1;

        for (int i = 0; i <= segmentCount; i++)
        {
            // 윈도우 내 로컬 비율 (0 = 꼬리, 1 = 머리)
            float localT = (float)i / segmentCount;

            // 실제 볼트 전체 길이 위의 위치
            float worldT = tailT + (headT - tailT) * localT;

            Vector3 basePos  = startPoint + forward * (totalLength * worldT);

            // 양 끝이 뾰족한 envelope
            float envelope = Mathf.Sin(localT * Mathf.PI);
            float offset   = cachedOffsets[i] * envelope;

            lr.SetPosition(i, basePos + perp * offset);
        }
    }

    private IEnumerator AnimateBolt()
    {
        float travelDuration = duration * travelRatio;
        float fadeDuration   = duration * (1f - travelRatio);

        while (elapsed < duration)
        {
            elapsed     += Time.deltaTime;
            wiggleTimer += Time.deltaTime;

            // 주기마다 오프셋 갱신 → 지지직 효과
            if (wiggleTimer >= wiggleInterval)
            {
                wiggleTimer = 0f;
                RefreshCachedOffsets();
            }

            float headT, tailT, alpha;

            if (elapsed < travelDuration)
            {
                // ── 이동 단계: 윈도우 전체가 앞으로 이동
                //
                // progress 0 → 1 : 윈도우 중심이 start → end 방향으로 이동
                // 시작: center = windowSize * 0.5  (머리가 start 근처에서 출발)
                // 종료: center = 1 - windowSize * 0.5 (꼬리가 end 근처에 도착)
                float progress = elapsed / travelDuration;
                float center   = Mathf.Lerp(windowSize * 0.5f, 1f - windowSize * 0.5f, progress);
                headT = Mathf.Clamp01(center + windowSize * 0.5f);
                tailT = Mathf.Clamp01(center - windowSize * 0.5f);
                alpha = 1f;
            }
            else
            {
                float fadeRatio = (elapsed - travelDuration) / fadeDuration;
                float tailStart = 1f - windowSize; // 이동 단계가 끝난 tailT 위치
                headT = 1f;
                tailT = Mathf.Lerp(tailStart, 1f, fadeRatio); // ← tailStart에서 1로 이동
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
        Gradient g               = lr.colorGradient;
        GradientAlphaKey[] keys  = g.alphaKeys;

        for (int i = 0; i < keys.Length; i++)
        {
            float baseAlpha = (baseAlphaValues != null && i < baseAlphaValues.Length)
                ? baseAlphaValues[i]
                : keys[i].alpha;

            keys[i].alpha = baseAlpha * alpha;
        }

        g.alphaKeys      = keys;
        lr.colorGradient = g;
    }

    // ─────────────────────────────────────────
    //  팩토리 메서드
    // ─────────────────────────────────────────
    public static HammerBolt Create(Vector3 start, Vector3 end, float duration = 0.5f, int damage = 10)
    {
        GameObject go   = new GameObject("HammerBolt");
        HammerBolt bolt = go.AddComponent<HammerBolt>();
        bolt.SpawnBolt(start, end, duration, damage);
        return bolt;
    }
}