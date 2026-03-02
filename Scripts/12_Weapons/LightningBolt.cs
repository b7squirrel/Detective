using System.Collections;
using UnityEngine;

/// <summary>
/// LightningBoltScript를 대체하는 번개 컴포넌트.
/// PoolingKey만 함께 부착하여 사용.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LightningBolt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LineRenderer lineRenderer;

    [Header("Lightning Shape")]
    [SerializeField] int segments = 12;             // 번개 꺾임 횟수 (많을수록 복잡)
    [SerializeField] float displacement = 0.6f;     // 꺾임 최대 폭
    [SerializeField] float regenerateInterval = 0.05f; // 번개 모양 재생성 주기 (초)

    [Header("Visual")]
    [SerializeField] float lineWidth = 0.15f;       // 기본 굵기 (baseDamage 기준)
    [SerializeField] float maxLineWidth = 0.6f;     // 데미지가 아무리 높아도 이 굵기를 넘지 않음
    [SerializeField] float baseDamage = 10f;        // 기본 굵기 기준 데미지 (Inspector에서 초기 데미지와 맞춰줄 것)
    [SerializeField] float uvScrollSpeed = 3f;      // 스프라이트 스크롤 속도

    float currentWidth;                             // 실제 적용 굵기 (데미지 반영)

    // 외부에서 설정
    [HideInInspector] public Transform StartTransform; // ShootPoint를 직접 참조 → 이동 추적
    [HideInInspector] public Vector2 EndPosition;

    // StartTransform에서 매 프레임 갱신되는 실제 시작점
    Vector2 StartPosition => StartTransform != null
        ? (Vector2)StartTransform.position
        : cachedStartPosition;
    Vector2 cachedStartPosition; // StartTransform이 null일 때 fallback

    float regenerateTimer;
    float uvOffset;
    bool isActive;

    // ─────────────────────────────────────────
    void OnEnable()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // SetDamage가 호출되지 않았을 경우 기본값 사용
        if (currentWidth <= 0f)
            currentWidth = lineWidth;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.startWidth = currentWidth;
        lineRenderer.endWidth = currentWidth * 0.4f; // 끝으로 갈수록 가늘어짐

        isActive = true;
        regenerateTimer = 0f;
        uvOffset = 0f;

        GenerateLightningPoints();
    }

    void OnDisable()
    {
        isActive = false;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (!isActive) return;

        // 번개 모양 재생성
        regenerateTimer += Time.deltaTime;
        if (regenerateTimer >= regenerateInterval)
        {
            regenerateTimer = 0f;
            GenerateLightningPoints();
        }

        // UV 스크롤 애니메이션
        uvOffset += uvScrollSpeed * Time.deltaTime;
        if (uvOffset > 1f) uvOffset -= 1f;
        lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(uvOffset, 0f));
    }

    // ─────────────────────────────────────────

    /// <summary>
    /// 시작점 → 끝점 사이에 지그재그 번개 포인트 생성
    /// </summary>
    void GenerateLightningPoints()
    {
        Vector2 start = StartPosition;
        Vector2 end = EndPosition;

        Vector2 dir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x); // 수직 방향
        float totalLength = Vector2.Distance(start, end);
        float segmentLength = totalLength / segments;

        lineRenderer.SetPosition(0, start);

        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector2 basePoint = Vector2.Lerp(start, end, t);

            // 수직 방향으로 랜덤 displacement
            // 양 끝 근처는 덜 꺾이게 (t가 0이나 1에 가까울수록 감쇠)
            float edgeFade = Mathf.Sin(t * Mathf.PI);
            float offset = Random.Range(-displacement, displacement) * edgeFade;

            Vector2 point = basePoint + perpendicular * offset;
            lineRenderer.SetPosition(i, point);
        }

        lineRenderer.SetPosition(segments, end);
    }

    /// <summary>
    /// LightningWeapon에서 Activate 전에 호출: 데미지에 비례해서 굵기 계산
    /// </summary>
    public void SetDamage(float damage)
    {
        float damageRatio = Mathf.Clamp(damage / baseDamage, 1f, maxLineWidth / lineWidth);
        currentWidth = Mathf.Min(lineWidth * damageRatio, maxLineWidth);
    }

    /// <summary>
    /// LightningWeapon에서 호출: ShootPoint Transform + 도착점 설정
    /// ShootPoint가 이동해도 번개 시작점이 따라감
    /// </summary>
    public void Activate(Transform startTransform, Vector2 end, float duration)
    {
        StartTransform = startTransform;
        cachedStartPosition = startTransform != null ? (Vector2)startTransform.position : Vector2.zero;
        EndPosition = end;

        GenerateLightningPoints();
        StartCoroutine(AutoDisable(duration));
    }

    /// <summary>
    /// 시너지용: 고정 시작 위치 사용 (ShootPoint 추적 없음)
    /// </summary>
    public void Activate(Vector2 start, Vector2 end, float duration)
    {
        StartTransform = null;
        cachedStartPosition = start;
        EndPosition = end;

        GenerateLightningPoints();
        StartCoroutine(AutoDisable(duration));
    }

    IEnumerator AutoDisable(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}