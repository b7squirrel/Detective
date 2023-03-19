using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnAreas; // 4개
    [SerializeField] Transform spawnBox; // 스폰 포인트들을 가지고 있는 박스
    [SerializeField] GameObject PropToSpawn;
    PropPoint[] points; // 스폰 포인트들
    List<Vector2> visitedArea;
    void Start()
    {
        points = GetComponentsInChildren<PropPoint>();
        if (visitedArea == null)
        {
            visitedArea = new List<Vector2>();
        }
        foreach (var item in spawnAreas)
        {
            SpawnProps(item.transform.position);
        }
    }
    // 스폰박스를 끌어다 놓고
    // 스폰 포인트만큼 스폰하고
    // 이곳을 방문한 곳으로 기록해 놓기
    public void SpawnProps(Vector2 pos)
    {
        foreach (var item in visitedArea)
        {
            if (item == pos)
                return;
        }

        GetSpawnBox(pos);
        foreach (var item in points)
        {
            Instantiate(PropToSpawn, item.transform.position, Quaternion.identity);
        }

        visitedArea.Add(pos);
    }
    public bool CheckIfVisited(Vector2 pos)
    {
        if (visitedArea.Find(x => x == pos) != null) return true;
        return false;
    }
    void GetSpawnBox(Vector2 targetPos)
    {
        spawnBox.transform.position = targetPos;
    }
}
