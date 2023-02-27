using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCollision : MonoBehaviour
{
    float halfHeight, halfWidth;
    EdgeCollider2D edgeCollder;
    void Awake()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        Debug.Log("Half Width = " + halfWidth + "Half Height = " + halfHeight);

        edgeCollder = this.GetComponent<EdgeCollider2D>();
        CreateEdgeCollider();
    }
    //call this at start and whenever the resolution changes
    void CreateEdgeCollider()
    {
        List<Vector2> edges = new List<Vector2>();
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width,0)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width,Screen.height)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(0,Screen.height)));
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edgeCollder.SetPoints(edges);
    }
}
