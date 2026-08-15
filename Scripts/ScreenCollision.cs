using System.Collections.Generic;
using UnityEngine;

public class ScreenCollision : MonoBehaviour
{
    float halfHeight, halfWidth;
    EdgeCollider2D edgeCollder;
    [SerializeField] RectTransform screenEdgeReference;
    Vector2 topEdge;
    float adjustedHalfHeight;
    [SerializeField] GameObject debuggingDot;

    Camera cam; // Camera.main 캐싱
    float lastOrthoSize;

    void Awake()
    {
        cam = Camera.main; // 한 번만 찾아서 캐싱

        topEdge = screenEdgeReference.transform.position;

        halfHeight = cam.orthographicSize;
        halfWidth = cam.aspect * halfHeight;
        adjustedHalfHeight = halfHeight - topEdge.y;

        edgeCollder = this.GetComponent<EdgeCollider2D>();
        CreateEdgeCollider();

        lastOrthoSize = cam.orthographicSize;
    }

    void Update()
    {
        if (!Mathf.Approximately(cam.orthographicSize, lastOrthoSize))
        {
            CreateEdgeCollider();
            lastOrthoSize = cam.orthographicSize;
        }
    }

    void CreateEdgeCollider()
    {
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();

        float zDistance = -cam.transform.position.z;
        Vector2 camPos = cam.transform.position; // 현재 카메라 위치

        // world 좌표에서 camPos를 빼서 '진짜' 로컬 오프셋으로 변환
        Vector2 bottomLeft = (Vector2)cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance)) - camPos;
        Vector2 topLeft = (Vector2)cam.ViewportToWorldPoint(new Vector3(0, 1, zDistance)) - camPos;
        Vector2 topRight = (Vector2)cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance)) - camPos;
        Vector2 bottomRight = (Vector2)cam.ViewportToWorldPoint(new Vector3(1, 0, zDistance)) - camPos;


        Vector2[] points = new Vector2[]
        {
            bottomLeft,
            topLeft,
            topRight,
            bottomRight,
            bottomLeft
        };

        edgeCollider.points = points;
    }
}