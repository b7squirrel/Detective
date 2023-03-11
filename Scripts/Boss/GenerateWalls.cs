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

        // 핸드폰의 세로축 길이에 맞춰서 정사각형을 만들기
        for (int x = (int)(center.x - halfHeight); x < (int)(center.x + halfHeight); x += 2)
        {
            for (int y = (int)(center.y - halfHeight - 2); y < (int)(center.y + halfHeight + 2); y += 2)
            {
                // 가장자리만 검색
                if (x == (int)(center.x - halfHeight) || x == (int)(center.x + halfHeight - 2) || y == (int)(center.y - halfHeight - 2) || y == (int)(center.y + halfHeight))
                {
                    GameObject b = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    bricks.Add(b);
                }
            }
        }

        foreach (GameObject item in bricks)
        {
            Transform itemTransform = item.transform;
            Bouncer bouncer = item.GetComponent<Bouncer>();
            int minX = (int)(center.x - halfHeight);
            int maxX = (int)(center.x + halfHeight - 2);
            int minY = (int)(center.y - halfHeight - 2);
            int maxY = (int)(center.y + halfHeight);
            if (itemTransform.position.x == minX)
            {
                // itemTransform.localEulerAngles = new Vector3(0, 0, -90f);
                bouncer.BouncingDir = Vector2.right;
            }
            if (itemTransform.position.x == maxX)
            {
                // itemTransform.localEulerAngles = new Vector3(0, 0, 90f);
                bouncer.BouncingDir = Vector2.left;
            }
            if (itemTransform.position.y == maxY)
            {
                // itemTransform.localEulerAngles = new Vector3(0, 0, 180f);
                bouncer.BouncingDir = Vector2.down;
            }
            if (itemTransform.position.y == minY)
            {
                // itemTransform.localEulerAngles = new Vector3(0, 0, 180f);
                bouncer.BouncingDir = Vector2.up;
            }
        }
    }
}
