using System.Collections.Generic;
using UnityEngine;

public class EnemyFinder : MonoBehaviour
{
    public static EnemyFinder instance;

    [SerializeField] LayerMask enemy;
    [SerializeField] LayerMask props;
    [SerializeField] float updateInterval = 0.1f;

    // 화면 크기
    private float halfHeight, halfWidth;
    private float searchRadius;

    // 버퍼 (재사용으로 GC 제거)
    private Collider2D[] hitBuffer = new Collider2D[250];
    private Collider2D[] overlapBuffer = new Collider2D[10]; // GetClosestEnemyTransform용 버퍼
    private List<EnemyDistance> enemyDistanceBuffer = new List<EnemyDistance>(250);

    // 캐싱된 결과
    private List<Vector2> cachedEnemyPositions = new List<Vector2>();
    private float lastUpdateTime = -1f;

    // 람다 정렬 대신 캐싱된 Comparer 사용하여 GC 방지
    private static readonly EnemyDistanceComparer distanceComparer = new EnemyDistanceComparer();

    void Awake()
    {
        instance = this;

        halfHeight = Camera.main.orthographicSize;
        halfWidth = Camera.main.aspect * halfHeight;

        // 화면 대각선 길이를 반지름으로 사용
        searchRadius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) * 0.6f;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateEnemyCache();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdateEnemyCache()
    {
        Vector2 playerPosition = transform.position;

        // OverlapCircleNonAlloc으로 GC 없이 탐색
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            playerPosition,
            searchRadius,
            hitBuffer,
            enemy | props
        );

        enemyDistanceBuffer.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Idamageable damageable = hitBuffer[i].GetComponent<Idamageable>();
            if (damageable != null)
            {
                Vector2 enemyPosition = hitBuffer[i].transform.position;
                float sqrDistance = (enemyPosition - playerPosition).sqrMagnitude;

                enemyDistanceBuffer.Add(new EnemyDistance
                {
                    position = enemyPosition,
                    sqrDistance = sqrDistance
                });
            }
        }

        // 캐싱된 Comparer로 정렬 (람다 사용 시 매번 GC 발생)
        enemyDistanceBuffer.Sort(distanceComparer);

        cachedEnemyPositions.Clear();
        for (int i = 0; i < enemyDistanceBuffer.Count; i++)
        {
            cachedEnemyPositions.Add(enemyDistanceBuffer[i].position);
        }
    }

    /// <summary>
    /// 가장 가까운 적 위치를 외부 List에 채워줍니다.
    /// 호출부에서 List를 필드로 선언해 재사용하면 GC가 발생하지 않습니다.
    /// </summary>
    public void GetEnemies(int numberOfEnemies, List<Vector2> result)
    {
        result.Clear();

        int count = Mathf.Min(numberOfEnemies, cachedEnemyPositions.Count);
        for (int i = 0; i < count; i++)
            result.Add(cachedEnemyPositions[i]);

        // 부족한 개수만큼 Vector2.zero로 채움
        while (result.Count < numberOfEnemies)
            result.Add(Vector2.zero);
    }

    /// <summary>
    /// 가장 가까운 적의 위치 반환
    /// </summary>
    public Vector2 GetClosestEnemy()
    {
        if (cachedEnemyPositions.Count == 0)
            return Vector2.zero;

        return cachedEnemyPositions[0];
    }

    /// <summary>
    /// 가장 가까운 적의 Transform 반환
    /// </summary>
    public Transform GetClosestEnemyTransform()
    {
        Vector2 closestPos = GetClosestEnemy();
        if (closestPos == Vector2.zero)
            return null;

        // NonAlloc으로 배열 재사용
        int count = Physics2D.OverlapCircleNonAlloc(closestPos, 0.5f, overlapBuffer, enemy);
        for (int i = 0; i < count; i++)
        {
            if (overlapBuffer[i].GetComponent<Idamageable>() != null)
                return overlapBuffer[i].transform;
        }

        return null;
    }

    /// <summary>
    /// 특정 타겟을 제외한 가장 가까운 적의 Transform 반환 (Zap용)
    /// </summary>
    public Transform GetClosestEnemyTransformExcept(Transform excludeTarget)
    {
        if (cachedEnemyPositions.Count == 0)
            return null;

        if (excludeTarget == null)
            return GetClosestEnemyTransform();

        Vector2 excludePos = excludeTarget.position;

        for (int i = 0; i < cachedEnemyPositions.Count; i++)
        {
            Vector2 enemyPos = cachedEnemyPositions[i];

            // 같은 위치면 스킵 (같은 적)
            if (Vector2.Distance(enemyPos, excludePos) < 0.1f)
                continue;

            // NonAlloc으로 배열 재사용
            int count = Physics2D.OverlapCircleNonAlloc(enemyPos, 0.5f, overlapBuffer, enemy);
            for (int j = 0; j < count; j++)
            {
                if (overlapBuffer[j].transform != excludeTarget &&
                    overlapBuffer[j].GetComponent<Idamageable>() != null)
                {
                    return overlapBuffer[j].transform;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 화면 범위 내 모든 적 반환. NonAlloc 버퍼를 사용합니다.
    /// count를 반환하므로 호출부에서 배열 크기를 직접 제어할 수 있습니다.
    /// </summary>
    public Collider2D[] GetAllEnemies()
    {
        Vector2 center = GameManager.instance.player.transform.position;
        float allEnemiesRadius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) * 1f;

        // hitBuffer 재사용 (최대 250개)
        Physics2D.OverlapCircleNonAlloc(center, allEnemiesRadius, hitBuffer, enemy | props);
        return hitBuffer;
    }

    public int GetCachedEnemyCount()
    {
        return cachedEnemyPositions.Count;
    }

    private struct EnemyDistance
    {
        public Vector2 position;
        public float sqrDistance;
    }

    // 람다 대신 캐싱된 Comparer 사용으로 정렬 시 GC 방지
    private class EnemyDistanceComparer : IComparer<EnemyDistance>
    {
        public int Compare(EnemyDistance a, EnemyDistance b)
            => a.sqrDistance.CompareTo(b.sqrDistance);
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.red;
        for (int i = 0; i < cachedEnemyPositions.Count && i < 5; i++)
        {
            Gizmos.DrawWireSphere(cachedEnemyPositions[i], 0.3f);
        }
    }
}