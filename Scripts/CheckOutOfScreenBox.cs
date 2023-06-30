using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOutOfScreenBox : MonoBehaviour
{
    float halfHeight, halfWidth;
    BoxCollider2D col;

    void Awake()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        col = GetComponent<BoxCollider2D>();
        col.size = new Vector2(2 * halfWidth, 2 * halfHeight);
    }
}
