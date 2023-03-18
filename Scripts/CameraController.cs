using UnityEngine;

public class CameraController : MonoBehaviour
{
    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;
    [SerializeField] float bosscameraMoveSpeed;
    [SerializeField] GameObject dot;
    Vector3 target = Vector3.zero;
    bool isBossCamera;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;

        boxCol.transform.localScale = Vector3.one * 10000;

        target = Vector3.zero;
    }

    void Update()
    {
        if (target != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, bosscameraMoveSpeed * Time.unscaledDeltaTime);
            float distance = Vector3.Distance(transform.position, target);

            if (isBossCamera == false)
            {
                if(distance < .1f)
                {
                    target = Vector3.zero;
                }
            }
            return;
        }

        if (player != null)
        {
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x, boxCol.bounds.min.x + halfWidth, boxCol.bounds.max.x - halfWidth),
                Mathf.Clamp(player.transform.position.y, boxCol.bounds.min.y, boxCol.bounds.max.y),
                transform.position.z);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
    public void CameraToTarget(Vector3 target, bool isBossCamera)
    {
        this.target = new Vector3(target.x, target.y, -10f);
        this.isBossCamera = isBossCamera;
    }
}
