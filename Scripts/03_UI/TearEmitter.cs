using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 위에서 UI 이미지를 파티클처럼 발사하는 범용 이미터.
/// Time.timeScale = 0 일 때도 정상 작동합니다 (Unscaled Time 사용).
/// 오브젝트가 활성화(SetActive true)되는 순간 자동으로 눈물 방출을 시작합니다.
/// </summary>
public class TearEmitter : MonoBehaviour
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Inspector 설정
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Header("── 스폰 위치 ──")]
    [Tooltip("눈물이 나오는 기준 위치")]
    public RectTransform spawnPoint;

    [Header("── 발사 방향 ──")]
    [Tooltip("기본 발사 방향 벡터 (자동 정규화됨). 아래쪽이면 (0,-1)")]
    public Vector2 launchDirection = Vector2.down;

    [Tooltip("발사 방향 랜덤 오프셋 각도 (±도). 클수록 퍼짐")]
    [Range(0f, 90f)]
    public float directionOffsetAngle = 25f;

    [Header("── 파티클 속성 ──")]
    [Tooltip("눈물 스프라이트 (없으면 흰 사각형으로 표시됨)")]
    public Sprite tearSprite;

    [Tooltip("눈물 이미지 크기 범위 (픽셀 기준, Min ~ Max)")]
    public Vector2 sizeRange = new Vector2(14f, 30f);

    [Tooltip("발사 속도 범위 (픽셀/초, Min ~ Max)")]
    public Vector2 speedRange = new Vector2(120f, 320f);

    [Tooltip("파티클 수명 범위 (초, Min ~ Max)")]
    public Vector2 lifetimeRange = new Vector2(0.5f, 1.1f);

    [Tooltip("중력 가속도 (픽셀/초²). 눈물이 아래로 가속됨")]
    public float gravity = 250f;

    [Header("── 스폰 빈도 ──")]
    [Tooltip("초당 스폰 개수")]
    public float emitRate = 18f;

    [Header("── 오브젝트 풀 ──")]
    [Tooltip("미리 생성해둘 풀 크기 (emitRate × 평균수명 × 1.5 정도)")]
    public int poolSize = 35;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  내부 변수
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private bool          _isEmitting;
    private bool          _initialized;
    private Canvas        _rootCanvas;
    private RectTransform _canvasRect;
    private readonly List<RectTransform> _pool = new List<RectTransform>();
    private Coroutine     _emitCoroutine;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  유니티 이벤트
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        if (!_initialized) Initialize();
        StartEmitting();
    }

    void OnDisable()
    {
        StopEmitting();
    }

    void OnDestroy()
    {
        foreach (var rt in _pool)
            if (rt != null) Destroy(rt.gameObject);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  초기화
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void Initialize()
    {
        if (_initialized) return;

        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas != null)
        {
            Canvas parent;
            while ((parent = _rootCanvas.transform.parent?.GetComponent<Canvas>()) != null)
                _rootCanvas = parent;
        }

        if (_rootCanvas == null)
        {
            Debug.LogError("[TearEmitter] 부모 Canvas를 찾을 수 없습니다.");
            return;
        }

        _canvasRect = _rootCanvas.GetComponent<RectTransform>();

        if (spawnPoint == null)
            spawnPoint = GetComponent<RectTransform>();

        BuildPool();
        _initialized = true;
    }

    void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
            _pool.Add(CreateTearObject());
    }

    RectTransform CreateTearObject()
    {
        var go = new GameObject("Tear", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_canvasRect, false);

        var img = go.GetComponent<Image>();
        img.sprite        = tearSprite;
        img.raycastTarget = false;
        img.color         = Color.white;

        go.SetActive(false);
        return go.GetComponent<RectTransform>();
    }

    RectTransform RentFromPool()
    {
        foreach (var rt in _pool)
            if (!rt.gameObject.activeSelf) return rt;

        Debug.LogWarning("[TearEmitter] Pool 부족 — 동적 확장. poolSize 늘리기를 권장합니다.");
        var extra = CreateTearObject();
        _pool.Add(extra);
        return extra;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  공개 API
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public void StartEmitting()
    {
        if (_isEmitting || !_initialized) return;
        _isEmitting    = true;
        _emitCoroutine = StartCoroutine(EmitLoop());
    }

    public void StopEmitting()
    {
        if (!_isEmitting) return;
        _isEmitting = false;
        if (_emitCoroutine != null)
        {
            StopCoroutine(_emitCoroutine);
            _emitCoroutine = null;
        }
    }

    public bool IsEmitting => _isEmitting;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  스폰 루프 (Unscaled Time)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    IEnumerator EmitLoop()
    {
        float interval = 1f / Mathf.Max(emitRate, 0.01f);
        while (_isEmitting)
        {
            SpawnTear();
            yield return new WaitForSecondsRealtime(interval); // ← Unscaled
        }
    }

    void SpawnTear()
    {
        var rt = RentFromPool();
        rt.gameObject.SetActive(true);

        rt.localPosition = WorldToCanvasLocal(spawnPoint.position);

        float size   = Random.Range(sizeRange.x, sizeRange.y);
        rt.sizeDelta = new Vector2(size, size);

        float offsetDeg = Random.Range(-directionOffsetAngle, directionOffsetAngle);
        Vector2 dir     = Rotate(launchDirection.normalized, offsetDeg);

        float speed    = Random.Range(speedRange.x,    speedRange.y);
        float lifetime = Random.Range(lifetimeRange.x, lifetimeRange.y);

        StartCoroutine(MoveTear(rt, dir, speed, lifetime));
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  파티클 이동 코루틴 (Unscaled Time)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    IEnumerator MoveTear(RectTransform rt, Vector2 dir, float speed, float lifetime)
    {
        if (rt == null) yield break;

        var     img      = rt.GetComponent<Image>();
        Vector2 velocity = dir * speed;
        float   elapsed  = 0f;

        const float FADE_START = 0.65f;

        while (elapsed < lifetime)
        {
            if (rt == null || !rt.gameObject.activeSelf) yield break;

            float dt = Time.unscaledDeltaTime; // ← Unscaled

            velocity         += Vector2.down * gravity * dt;
            rt.localPosition += (Vector3)(velocity * dt);
            elapsed          += dt;

            float progress = elapsed / lifetime;
            float alpha    = progress >= FADE_START
                ? Mathf.InverseLerp(1f, FADE_START, progress)
                : 1f;
            img.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        img.color = Color.white;
        rt.gameObject.SetActive(false);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  유틸리티
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    Vector3 WorldToCanvasLocal(Vector3 worldPos)
    {
        Camera cam     = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                         ? null
                         : _rootCanvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, cam, out Vector2 local);
        return local;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y,
                           sin * v.x + cos * v.y);
    }
}