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
        float offsetY = new Vector2(Screen.width,Screen.height - topEdge.y).y;

        List<Vector2> edges = new List<Vector2>();
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width,0)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height - offsetY)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(0,Screen.height - offsetY)));
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edgeCollder.SetPoints(edges);
    }
}
