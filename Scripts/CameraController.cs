using UnityEngine;

public class CameraController : MonoBehaviour
{
    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;
    [SerializeField] float bosscameraMoveSpeed;
    [SerializeField] GameObject dot;
    [SerializeField] float offset; // 이 값만큼 y축 카메라 바운드 조절

    WallManager wallManager;
    float spawnConst;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
    }

    void Update()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        spawnConst = wallManager.GetSpawnAreaConstant();
        spawnConst += .5f * spawnConst;
        boxCol.transform.localScale = Vector3.one * spawnConst;

        if (player != null)
        {
            //transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x, boxCol.bounds.min.x, boxCol.bounds.max.x),
                Mathf.Clamp(player.transform.position.y, boxCol.bounds.min.y + (offset), boxCol.bounds.max.y - offset),
                transform.position.z);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
}
