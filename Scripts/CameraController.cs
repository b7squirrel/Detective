using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController instance; // ⭐ 추가: Spawner 등 외부에서 접근하기 위한 싱글톤

    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;
    [SerializeField] float bosscameraMoveSpeed;
    [SerializeField] GameObject dot;
    [SerializeField] float offset;
    [SerializeField] float offsetUpperWall;
    [SerializeField] float offsetSide; // ⭐ 추가: 좌우 여유값 (인스펙터에서 조정)

    [Header("Zoom In-Out")]
    [SerializeField] float startSize = 15f;
    [SerializeField] float endSize = 28f;
    [SerializeField] float zoomDuration = 1.2f;
    [SerializeField] float zoomStartDelay = 0.1f;

    [Header("Boss Zoom Out")] // ⭐ 추가
    [SerializeField] float bossZoomSize = 35f; // ⭐ 추가: 보스 등장 시 목표 카메라 크기

    [Header("Bounds Buffer")]
    [SerializeField] float boundsBufferX = 0.8f; // ⭐ 추가: 기존 .8f를 X축 전용으로 분리
    [SerializeField] float boundsBufferY = 1.5f; // ⭐ 추가: Y축은 더 크게 시작 (테스트하며 조정)

    WallManager wallManager;
    float spawnConst;

    void Awake()
    {
        instance = this; // ⭐ 추가

        player = FindObjectOfType<Player>();
        Camera.main.orthographicSize = startSize;
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
    }

    void Start()
    {
        // StartCoroutine(ZoomOutRoutine());
    }

    // 기존 시작용 줌 코루틴 — 그대로 유지
    IEnumerator ZoomOutRoutine()
    {
        yield return new WaitForSeconds(0);
        float elapsed = 0f;
        Camera cam = Camera.main;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, easedT);
            halfHeight = cam.orthographicSize;
            halfWidth = cam.aspect * halfHeight;
            yield return null;
        }

        cam.orthographicSize = endSize;
        halfHeight = endSize;
        halfWidth = cam.aspect * halfHeight;
    }

    public void ZoomInOnStart()
    {
        StartCoroutine(ZoomOutRoutine());
    }

    // ⭐ 추가: 보스 등장 시 줌아웃 — 현재 카메라 크기에서 bossZoomSize로 이동
    IEnumerator ZoomToSizeRoutine(float targetSize, float duration)
    {
        float elapsed = 0f;
        Camera cam = Camera.main;
        float fromSize = cam.orthographicSize; // 현재 값에서 시작 (18이든 뭐든 안전)

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            cam.orthographicSize = Mathf.Lerp(fromSize, targetSize, easedT);
            halfHeight = cam.orthographicSize;
            halfWidth = cam.aspect * halfHeight;
            yield return null;
        }

        cam.orthographicSize = targetSize;
        halfHeight = targetSize;
        halfWidth = cam.aspect * halfHeight;
    }

    public void ZoomOutForBoss() // ⭐ 추가: Spawner에서 호출할 함수
    {
        StartCoroutine(ZoomToSizeRoutine(bossZoomSize, zoomDuration));
    }

    void Update()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();

        float spawnConstX = wallManager.GetSpawnAreaConstant();   // ⭐ 변경: 기존 spawnConst → X축 전용
        float spawnConstY = wallManager.GetSpawnAreaConstantY();  // ⭐ 추가: Y축 전용

        float scaleX = spawnConstX + boundsBufferX * spawnConstX; // ⭐ 변경
        float scaleY = spawnConstY + boundsBufferY * spawnConstY; // ⭐ 추가
        boxCol.transform.localScale = new Vector3(scaleX, scaleY, 1f); // ⭐ 변경: X/Y 따로 적용

        if (player != null)
        {
            float minX = boxCol.bounds.min.x + halfWidth - offsetSide;
            float maxX = boxCol.bounds.max.x - halfWidth + offsetSide;
            float targetX;
            if (minX <= maxX)
            {
                targetX = Mathf.Clamp(player.transform.position.x, minX, maxX);
            }
            else
            {
                targetX = boxCol.bounds.center.x;
            }

            float minY = boxCol.bounds.min.y + halfHeight - offset;         // ⭐ 앞서 합의한 형태 유지
            float maxY = boxCol.bounds.max.y - halfHeight + offsetUpperWall; // ⭐ 앞서 합의한 형태 유지
            float targetY;
            if (minY <= maxY)
            {
                targetY = Mathf.Clamp(player.transform.position.y, minY, maxY);
            }
            else
            {
                targetY = boxCol.bounds.center.y;
            }

            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
}