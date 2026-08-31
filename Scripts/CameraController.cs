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

    [Header("Zoom In-Out")]
    [SerializeField] float startSize = 15f;
    [SerializeField] float endSize = 28f;
    [SerializeField] float zoomDuration = 1.2f;
    [SerializeField] float zoomStartDelay = 0.1f;

    [Header("Boss Zoom Out")] // ⭐ 추가
    [SerializeField] float bossZoomSize = 35f; // ⭐ 추가: 보스 등장 시 목표 카메라 크기

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
        spawnConst = wallManager.GetSpawnAreaConstant();
        spawnConst += .8f * spawnConst;
        boxCol.transform.localScale = Vector3.one * spawnConst;

        if (player != null)
        {
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x, boxCol.bounds.min.x, boxCol.bounds.max.x),
                Mathf.Clamp(player.transform.position.y, boxCol.bounds.min.y + offset, boxCol.bounds.max.y - offsetUpperWall),
                transform.position.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
}