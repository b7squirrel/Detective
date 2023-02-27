using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] GameObject brickPrefab;
    float halfHeight, halfWidth;


    private void Awake()
    {
        Vector2 center = FindObjectOfType<Player>().transform.position;

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;

        for (int x = (int)(center.x - halfHeight); x < (int)(center.x + halfHeight); x++)
        {
            for (int y = (int)(center.y - halfHeight + 2); y < (int)(center.y + halfHeight); y++)
            {
                if (x == (int)(center.x - halfHeight) || x == (int)(center.x + halfHeight - 1) || y == (int)(center.y - halfHeight +2) || y == (int)(center.y + halfHeight - 1))
                {
                    GameObject brick = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    brick.transform.SetParent(transform);
                }

            }
        }
    }
}
