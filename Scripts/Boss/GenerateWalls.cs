using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] GameObject brickPrefab;
    List<GameObject> bricks;

    public void GenWalls()
    {
        if (bricks == null)
        {
            bricks = new List<GameObject>();
        }

        Vector2 center = FindObjectOfType<Player>().transform.position;

        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = Camera.main.aspect * halfHeight;

        for (int x = (int)(center.x - halfHeight); x < (int)(center.x + halfHeight); x++)
        {
            for (int y = (int)(center.y - halfHeight + 2); y < (int)(center.y + halfHeight); y++)
            {
                if (x == (int)(center.x - halfHeight) || x == (int)(center.x + halfHeight - 1) || y == (int)(center.y - halfHeight + 2) || y == (int)(center.y + halfHeight - 1))
                {
                    GameObject b = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    bricks.Add(b);
                }
            }
        }


    }
}
