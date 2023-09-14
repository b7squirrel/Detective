using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] GameObject brickPrefab;
    [SerializeField] GameObject bossBorders;
    [SerializeField] LayerMask enemyLayer;
    List<GameObject> bricks;

    public void GenWalls(int halfBouncerNumber)
    {
        
        if (bricks == null)
        {
            bricks = new List<GameObject>();
        }

        Vector2 center = FindObjectOfType<Player>().transform.position;

        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = Camera.main.aspect * halfHeight;


        // 핸드폰의 세로축 길이에 맞춰서 정사각형을 만들기
        Vector2Int playerPos = new Vector2Int((int)center.x, (int)center.y);

        GameObject container = new GameObject();
        container.transform.position = Vector3.zero;
        container.gameObject.name = "WallContainer";

        for (int x = playerPos.x - halfBouncerNumber; x < playerPos.x + halfBouncerNumber + 1; x += 2)
        {
            for (int y = playerPos.y - halfBouncerNumber; y < playerPos.y + halfBouncerNumber + 1; y += 2)
            {
                // 가장자리만 검색
                if (x == playerPos.x - halfBouncerNumber || x == playerPos.x + halfBouncerNumber || y == playerPos.y - halfBouncerNumber || y == playerPos.y + halfBouncerNumber)
                {
                    GameObject b = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    b.transform.parent = container.transform;
                    bricks.Add(b);
                }
            }
        }

        int minX = playerPos.x - halfBouncerNumber;
        int maxX = playerPos.x + halfBouncerNumber;
        int minY = playerPos.y - halfBouncerNumber;
        int maxY = playerPos.y + halfBouncerNumber;

        for (int i = 0; i < bricks.Count; i++)
        {
            Transform itemTransform = bricks[i].transform;
            Bouncer bouncer = bricks[i].GetComponent<Bouncer>();

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

        // gameManager에 등록되어 있는 camera boundary에 접근해서 벽의 크기와 같게 만들어 줌
        Collider2D boundary = GameManager.instance.cameraBoundary;
        // boundary.transform.localScale = Vector3.one * 2f * halfHeight;
        boundary.transform.localScale = new Vector3(2f * halfHeight, 5f, 1f);
        boundary.transform.position = (Vector2)Player.instance.transform.position;

        // 경계선 밖에 콜라이더를 배치해서 밖으로 빠져나가지 않게 하기
        GameObject borders = Instantiate(bossBorders);
        borders.transform.GetChild(0).transform.position = new Vector2(center.x, maxY);
        borders.transform.GetChild(1).transform.position = new Vector2(center.x, minY);
        borders.transform.GetChild(2).transform.position = new Vector2(minX, center.y);
        borders.transform.GetChild(3).transform.position = new Vector2(maxX, center.y);

        // 보스가 등장 할 때 보스벽의 바깥쪽에 있는 적들은 모두 DIe()처리
        // 모든 적 검색해서 allE에 담음
        Collider2D[] enemiesInScene = Physics2D.OverlapBoxAll(playerPos, new Vector2(200f, 200f), 0f, enemyLayer);
        List<EnemyBase> allEnemies = new List<EnemyBase>();
        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            EnemyBase e = enemiesInScene[i].transform.GetComponent<EnemyBase>();
            if (e == null)
                continue;
            allEnemies.Add(e);
        }

        // 벽 안의 적 검색해서 allE에서 빼냄
        Collider2D[] enemiesInWall = 
                Physics2D.OverlapBoxAll(playerPos, new Vector2(2 * halfBouncerNumber, 2 * halfBouncerNumber), 0f, enemyLayer);
        
        for (int i = 0; i < enemiesInWall.Length; i++)
        {
            EnemyBase e = enemiesInWall[i].transform.GetComponent<EnemyBase>();
            if (e == null)
                continue;
            allEnemies.Remove(e); // 벽 밖의 적들만 남게된다
        }
        for (int i = 0; i < allEnemies.Count; i++)
        {
            allEnemies[i].Die();
            
        }
    }
}
