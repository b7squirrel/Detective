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
            for (int y = (int)(center.y - halfHeight - 2); y < (int)(center.y + halfHeight + 2); y++)
            {
                if (x == (int)(center.x - halfHeight) || x == (int)(center.x + halfHeight - 1) || y == (int)(center.y - halfHeight - 2) || y == (int)(center.y + halfHeight + 1))
                {
                    GameObject b = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    bricks.Add(b);
                }
            }
        }

        foreach(GameObject item in bricks)
        {
            Transform itemTransform = item.transform;
            Bouncer bouncer = item.GetComponent<Bouncer>();
            int minX = (int)(center.x - halfHeight);
            int maxX = (int)(center.x + halfHeight -1);
            int minY = (int)(center.y - halfHeight - 2);
            int maxY = (int)(center.y + halfHeight + 1);
            if (itemTransform.position.x == minX && itemTransform.position.y != minY && itemTransform.position.y != maxY)
            {
                itemTransform.localEulerAngles = new Vector3(0, 0, -90f);
                bouncer.BouncingDir = Vector2.right;
                continue;
            }
            if (itemTransform.position.x == maxX && itemTransform.position.y != minY && itemTransform.position.y != maxY)
            {
                itemTransform.localEulerAngles = new Vector3(0, 0, 90f);
                bouncer.BouncingDir = Vector2.left;
                continue;
            }
            if (itemTransform.position.y == maxY && itemTransform.position.x != minX && itemTransform.position.x != maxX)
            {
                itemTransform.localEulerAngles = new Vector3(0, 0, 180f);
                bouncer.BouncingDir = Vector2.down;
                continue;
            }
        }
    }
}
