using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;
    [SerializeField] float bosscameraMoveSpeed;
    [SerializeField] GameObject dot;
    [SerializeField] float offset; // 이 값만큼 y축 카메라 바운드 조절
    [SerializeField] float offsetUpperWall; // 윗벽은 경험치바에 가려지지 않게 따로 바운드 조절

    [Header("Zoom In-Out")]
    [SerializeField] float startSize = 15f;
    [SerializeField] float endSize = 28f;
    [SerializeField] float zoomDuration = 1.2f;

    WallManager wallManager;
    float spawnConst;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        Camera.main.orthographicSize = startSize;

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
    }

    void Start()
    {
        StartCoroutine(ZoomOutRoutine());
    }

    IEnumerator ZoomOutRoutine()
    {
        float elapsed = 0f;
        Camera cam = Camera.main;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t); // 감속하며 부드럽게 멈춤

            cam.orthographicSize = Mathf.Lerp(startSize, endSize, easedT);

            // Gizmo용 값도 매 프레임 갱신
            halfHeight = cam.orthographicSize;
            halfWidth = cam.aspect * halfHeight;

            yield return null;
        }

        cam.orthographicSize = endSize;
        halfHeight = endSize;
        halfWidth = cam.aspect * halfHeight;
    }

    void Update()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        spawnConst = wallManager.GetSpawnAreaConstant();
        spawnConst += .5f * spawnConst;
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