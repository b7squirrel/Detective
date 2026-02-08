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
void Awake()
    {
topEdge = screenEdgeReference.transform.position;
// offset 위치 표시
// Vector2 worldPos = Camera.main.ScreenToWorldPoint(topEdge);
// GameObject dot2 = Instantiate(debuggingDot, transform);
// dot2.transform.position = worldPos;
halfHeight = Camera.main.orthographicSize;
halfWidth = Camera.main.aspect * halfHeight;
adjustedHalfHeight = halfHeight - topEdge.y;
edgeCollder = this.GetComponent<EdgeCollider2D>();
CreateEdgeCollider();
    }
//call this at start and whenever the resolution changes
void CreateEdgeCollider()
    {
Camera cam = Camera.main;
EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
// 카메라의 Z 위치를 기준으로 화면 모서리 위치를 구함
float zDistance = -cam.transform.position.z;
Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
Vector2 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, zDistance));
Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, zDistance));
Vector2 bottomRight = cam.ViewportToWorldPoint(new Vector3(1, 0, zDistance));
// EdgeCollider는 닫힌 루프를 원하면 시작점을 끝에 다시 넣어야 함
Vector2[] points = new Vector2[]
        {
bottomLeft,
topLeft,
topRight,
bottomRight,
bottomLeft // 시작점으로 다시 돌아옴
        };
edgeCollider.points = points;
    }
}