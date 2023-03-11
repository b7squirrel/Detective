using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] GameObject brickPrefab;
    [SerializeField] GameObject bossBorders;
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

        int minX = (int)(center.x - halfHeight);
        int maxX = (int)(center.x + halfHeight - 2);
        int minY = (int)(center.y - halfHeight - 2);
        int maxY = (int)(center.y + halfHeight);

        foreach (GameObject item in bricks)
        {
            Transform itemTransform = item.transform;
            Bouncer bouncer = item.GetComponent<Bouncer>();
            
            if (itemTransform.position.x == minX)
            {
                bouncer.BouncingDir = Vector2.right;
            }
            if (itemTransform.position.x == maxX)
            {
                bouncer.BouncingDir = Vector2.left;
            }
            if (itemTransform.position.y == maxY)
            {
                bouncer.BouncingDir = Vector2.down;
            }
            if (itemTransform.position.y == minY)
            {
                bouncer.BouncingDir = Vector2.up;
            }
        }

        // 경계선 밖에 콜라이더를 배치해서 밖으로 빠져나가지 않게 하기
        GameObject borders = Instantiate(bossBorders);
        borders.transform.GetChild(0).transform.position = new Vector2(center.x, maxY);
        borders.transform.GetChild(1).transform.position = new Vector2(center.x, minY);
        borders.transform.GetChild(2).transform.position = new Vector2(minX, center.y);
        borders.transform.GetChild(3).transform.position = new Vector2(maxX, center.y);
    }
}
