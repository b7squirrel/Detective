using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Player player;
    float halfHeight, halfWidth;
    [SerializeField] BoxCollider2D boxCol;

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
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x, boxCol.bounds.min.x + halfWidth, boxCol.bounds.max.x - halfWidth),
                Mathf.Clamp(player.transform.position.y, boxCol.bounds.min.y, boxCol.bounds.max.y),
                transform.position.z);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = new Color(1,0,0,.3f);
        Gizmos.DrawCube(transform.position, new Vector2(halfWidth * 2f, halfHeight * 2f));
    }
}
