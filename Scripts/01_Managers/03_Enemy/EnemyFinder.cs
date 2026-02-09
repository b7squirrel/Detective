using System.Collections.Generic;
using UnityEngine;

public class EnemyFinder : MonoBehaviour
{
    public static EnemyFinder instance;

    [SerializeField] LayerMask enemy;
    [SerializeField] float updateInterval = 0.1f;

    // 화면 크기
    private float halfHeight, halfWidth;
    private float searchRadius;

    // 버퍼 (재사용으로 GC 제거)
    private Collider2D[] hitBuffer = new Collider2D[250];
    private List<EnemyDistance> enemyDistanceBuffer = new List<EnemyDistance>(250);

    // 캐싱된 결과
    private List<Vector2> cachedEnemyPositions = new List<Vector2>();
    private float lastUpdateTime = -1f;

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

        // OverlapCircleNonAlloc 사용
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            playerPosition,
            searchRadius,
            hitBuffer,
            enemy
        );

        // 버퍼 클리어
        enemyDistanceBuffer.Clear();

        // Idamageable을 가진 적들만 필터링하고 거리 계산
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

        // 거리순 정렬
        enemyDistanceBuffer.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        // 캐시 업데이트
        cachedEnemyPositions.Clear();
        for (int i = 0; i < enemyDistanceBuffer.Count; i++)
        {
            cachedEnemyPositions.Add(enemyDistanceBuffer[i].position);
        }
    }

    public List<Vector2> GetEnemies(int numberOfEnemies)
    {
        // 캐시가 비어있으면 Vector2.zero로 채워서 반환
        if (cachedEnemyPositions.Count == 0)
        {
            List<Vector2> emptyResult = new List<Vector2>(numberOfEnemies);
            for (int i = 0; i < numberOfEnemies; i++)
            {
                emptyResult.Add(Vector2.zero);
            }
            return emptyResult;
        }

        // 요청한 개수만큼 반환
        List<Vector2> result = new List<Vector2>(numberOfEnemies);
        int count = Mathf.Min(numberOfEnemies, cachedEnemyPositions.Count);

        for (int i = 0; i < count; i++)
        {
            result.Add(cachedEnemyPositions[i]);
        }

        // 부족한 개수만큼 Vector2.zero 추가
        while (result.Count < numberOfEnemies)
        {
            result.Add(Vector2.zero);
        }

        return result;
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

        // 해당 위치의 적 찾기 (약간의 오차 허용)
        Collider2D[] hits = Physics2D.OverlapCircleAll(closestPos, 0.5f, enemy);

        // Idamageable이 있는 적만 반환
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponent<Idamageable>() != null)
            {
                return hits[i].transform;
            }
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

        // excludeTarget이 없으면 그냥 가장 가까운 적 반환
        if (excludeTarget == null)
            return GetClosestEnemyTransform();

        Vector2 excludePos = excludeTarget.position;

        // 제외할 타겟이 아닌 첫 번째 적 찾기
        for (int i = 0; i < cachedEnemyPositions.Count; i++)
        {
            Vector2 enemyPos = cachedEnemyPositions[i];

            // 같은 위치면 스킵 (같은 적)
            if (Vector2.Distance(enemyPos, excludePos) < 0.1f)
                continue;

            // 해당 위치의 적 Transform 찾기
            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyPos, 0.5f, enemy);

            for (int j = 0; j < hits.Length; j++)
            {
                if (hits[j].transform != excludeTarget &&
                    hits[j].GetComponent<Idamageable>() != null)
                {
                    return hits[j].transform;
                }
            }
        }

        // 제외하고 나면 적이 없으면 null
        return null;
    }

    public Collider2D[] GetAllEnemies()
    {
        Vector2 center = GameManager.instance.player.transform.position;
        float allEnemiesRadius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) * 1f;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            center,
            allEnemiesRadius,
            enemy
        );
        return enemies;
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

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // 탐색 범위 (원형)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        // 캐시된 적들
        Gizmos.color = Color.red;
        for (int i = 0; i < cachedEnemyPositions.Count && i < 5; i++)
        {
            Gizmos.DrawWireSphere(cachedEnemyPositions[i], 0.3f);
        }
    }
}