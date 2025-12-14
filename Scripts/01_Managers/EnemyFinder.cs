using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어에 붙여서 사용. safe area를 턈색해야 하므로 플레이어에 붙어서 이동하는 것이 좋음
/// </summary>
public class EnemyFinder : MonoBehaviour
{
    public static EnemyFinder instance;
    float halfHeight, halfWidth;
    Vector2 size;

    List<Vector2> allEnemies;

    [SerializeField] LayerMask enemy;

    List<Vector2> pickedEnemies;

    int delay; // 적을 찾는 함수를 얼마나 자주 할 것인지

    void Awake()
    {
        instance = this;
        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;
        size = new Vector2(halfWidth * .6f, halfHeight * .6f);

        pickedEnemies = new List<Vector2>();
        allEnemies = new();
    }

    void Update()
    {
        delay++;
        if (delay < 10) return;
        pickedEnemies.Clear();
        FindTarget(2);
        delay = 0;
    }

    void FindTarget(int numberOfTargets)
    {
        // 화면 안에서 공격 가능한 개체들 검색
        Collider2D[] hits =
            Physics2D.OverlapBoxAll(transform.position, size, 0f, enemy);

        allEnemies.Clear();

        for (int i = 0; i < hits.Length; i++)
        {
            Idamageable Idamage = hits[i].GetComponent<Idamageable>();
            if (Idamage != null)
            {
                allEnemies.Add(hits[i].GetComponent<Transform>().position);
            }
        }

        // 순회하면서 원하는 갯수만큼 공격 가능한 개체들을 수집
        float distanceToclosestEnemy = float.MaxValue;
        Vector2 closestEnemy = Vector2.zero;

        for (int i = 0; i < numberOfTargets; i++)
        {
            for (int y = 0; y < allEnemies.Count; y++)
            {
                float distanceToEnmey =
                Vector3.Distance(allEnemies[y], transform.position);

                if (distanceToEnmey < distanceToclosestEnemy)
                {
                    distanceToclosestEnemy = distanceToEnmey;
                    closestEnemy = allEnemies[y];
                }
            }

            // foreach가 다 돌고 나서 가장 가까운 적이 존재하면
            // 반환할 pickedEnemies에 추가하고, 그 적을 제외하고 다시 순회검색 
            if (closestEnemy != Vector2.zero)
            {
                pickedEnemies.Add(closestEnemy);
                allEnemies.Remove(closestEnemy);
            }
            else
            {
                pickedEnemies.Add(Vector2.zero);
            }
        }
    }
    public List<Vector2> GetEnemies(int _enemyNumbers)
    {
        List<Vector2> enemies = new();
        if (pickedEnemies.Count == 0) return null;
        for (int i = 0; i < _enemyNumbers; i++)
        {
            enemies.Add(pickedEnemies[i]);
        }
        return enemies;
    }

    public Collider2D[] GetAllEnemies()
    {
        Vector2 center = GameManager.instance.player.transform.position;

        Collider2D[] enemies =
                Physics2D.OverlapAreaAll(center - new Vector2(halfWidth * 1f, halfHeight * 1f),
                                            center + new Vector2(halfWidth * 1f, halfHeight * 1f), enemy);
        return enemies;
    }
    public Transform GetAllEnemyTransform()
    {
        return null;
    }

    #region 적 등록, 제거, Get
    //public void AddEnemyToList(Transform _enemyToAdd)
    //{
    //    if (fieldEnemies == null) fieldEnemies = new List<Transform>();
    //    fieldEnemies.Add(_enemyToAdd);
    //    enemyNames.Add(_enemyToAdd.name);
    //}
    //public void RemoveEnemyFromList(Transform _enemyToRemove)
    //{
    //    fieldEnemies.Remove(_enemyToRemove);
    //    enemyNames.Remove(_enemyToRemove.name);
    //}
    //public List<Transform> GetEnemyList()
    //{
    //    return fieldEnemies;
    //}
    #endregion
}
