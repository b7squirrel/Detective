using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnAreas; // 4개
    [SerializeField] Transform spawnBox; // 스폰 포인트들을 가지고 있는 박스
    [SerializeField] Transform propContainer; // 프랍을 담을 empty object. 하이어라키 정리용
    [SerializeField] GameObject[] PropToSpawn;
    PropPoint[] points; // 스폰 포인트들
    List<Vector2> visitedArea;
    void Start()
    {
        points = GetComponentsInChildren<PropPoint>();
        if (visitedArea == null)
        {
            visitedArea = new List<Vector2>();
        }
        for (int i = 0; i < spawnAreas.Length; i++)
        {
            SpawnProps(spawnAreas[i].transform.position);

        }
    }
    // 스폰박스를 끌어다 놓고
    // 스폰 포인트만큼 스폰하고
    // 이곳을 방문한 곳으로 기록해 놓기
    public void SpawnProps(Vector2 pos)
    {
        for (int i = 0; i < visitedArea.Count; i++)
        {
            if (visitedArea[i] == pos)
                return;
        }

        GetSpawnBox(pos);

        for (int i = 0; i < points.Length; i++)
        {
            GameObject go = Instantiate(PropToSpawn[(int)Random.Range(0, 2)], points[i].transform.position, Quaternion.identity);
            go.transform.parent = propContainer;
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
