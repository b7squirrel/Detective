using UnityEngine;

public class CameraController : MonoBehaviour
{
    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;
    [SerializeField] float bosscameraMoveSpeed;
    [SerializeField] GameObject dot;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;

        boxCol.transform.localScale = Vector3.one * 10000;
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
}
