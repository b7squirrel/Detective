using UnityEngine;
using System.Collections;

public class EnemyRangedAttack : MonoBehaviour
{
    #region Variables
    [Header("Ranged Attack Settings")]
    [SerializeField] float attackInterval = 3f;
    [SerializeField] float distanceToPlayer = 5f;
    [SerializeField] GameObject projectilePrefab;
    
    float nextAttackTime;
    bool finishedSpawn;
    bool isInitialized;
    
    // References
    EnemyBase enemyBase;
    Animator anim;

    [Header("디버그")]
    [SerializeField] bool showDebugLog;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        nextAttackTime = 0f;
        finishedSpawn = false;
    }

    void LateUpdate()
    {
        if (!isInitialized)
            return;
            
        if (enemyBase == null)
            return;

        RangedAttackCoolDown();
    }
    #endregion

    #region Initialization
    public void InitRangedAttack(EnemyData data)
    {
        Debug.Log($"[InitRangedAttack] 호출됨 - enemyType: {data.enemyType}");

        if (data.enemyType != EnemyType.Ranged)
        {
            Debug.Log($"[InitRangedAttack] Ranged 타입이 아니므로 비활성화");
            isInitialized = false;
            return;
        }

        attackInterval = data.attackInterval;
        distanceToPlayer = data.distanceToPlayer;
        projectilePrefab = data.projectilePrefab;

        // 스폰 후 쿨다운 시간 대기
        nextAttackTime = Time.time + attackInterval;

        Debug.Log($"[InitRangedAttack] 초기화 완료 - 첫 공격은 {attackInterval}초 후");

        isInitialized = true;

        StartCoroutine(AutoFinishSpawnCo());
    }

    IEnumerator AutoFinishSpawnCo()
    {
        yield return new WaitForSeconds(1f);

        if (!finishedSpawn)
        {
            DebugLog("[AutoFinishSpawn] 자동으로 스폰 완료 처리");
            finishedSpawn = true;
        }
    }

    public void SetFinishedSpawn(bool finished)
    {
        DebugLog($"[SetFinishedSpawn] 호출됨 - finished: {finished}");
        finishedSpawn = finished;
    }
    #endregion

    #region Ranged Attack Logic
    void RangedAttackCoolDown()
    {
        if (!finishedSpawn)
            return;

        if (Time.time >= nextAttackTime)
        {
            DebugLog($"[RangedAttackCoolDown] 공격 시간 도달 - Time: {Time.time}");
            
            if (DetectingPlayer())
            {
                DebugLog("[RangedAttackCoolDown] 플레이어 감지됨");
                
                float randomValue = Random.Range(0f, 1f);
                if (randomValue <= .1f)
                {
                    DebugLog($"[RangedAttackCoolDown] 공격 실행! (확률: {randomValue})");
                    FireProjectile();
                }
                else
                {
                    DebugLog($"[RangedAttackCoolDown] 확률 실패 (확률: {randomValue})");
                }
            }
            else
            {
                DebugLog("[RangedAttackCoolDown] 플레이어가 범위 밖");
            }
            
            nextAttackTime = Time.time + attackInterval;
        }
    }

    bool DetectingPlayer()
    {
        if (enemyBase.Target == null)
        {
            DebugLogWarninig("[DetectingPlayer] player가 null입니다!");
            return false;
        }

        Transform playerTransform = enemyBase.Target.transform;
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = playerTransform.position;
        
        float sqrDist = (playerPos - enemyPos).sqrMagnitude;
        float actualDistance = Mathf.Sqrt(sqrDist);
        bool inRange = sqrDist < Mathf.Pow(distanceToPlayer, 2f);
        
        string rangeStatus = inRange ? "범위 안" : "범위 밖";
        
        DebugLog($"[DetectingPlayer] 적:{enemyPos} 플레이어:{playerPos}");
        DebugLog($"[DetectingPlayer] 거리: {actualDistance:F2} / {distanceToPlayer} | {rangeStatus}");
        
        return inRange;
    }

    void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            DebugLogError("[FireProjectile] projectilePrefab이 null입니다!");
            return;
        }

        Logger.Log($"[FireProjectile] 투사체 생성: {projectilePrefab.name}");
        GameObject cannonBall = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        var projectile = cannonBall.GetComponentInChildren<IEnemyProjectile>();
        if (projectile == null)
        {
            DebugLogError("[FireProjectile] IEnemyProjectile 컴포넌트를 찾을 수 없습니다!");
            Destroy(cannonBall);
            return;
        }
        
        projectile.InitProjectileDamage(enemyBase.Stats.rangedDamage);
        
        if (anim != null)
        {
            anim.SetBool("Attack", true);
        }
        
        DebugLog("[FireProjectile] 투사체 생성 완료!");
    }
    #endregion

    void DebugLog(string _log)
    {
        if(showDebugLog) Logger.Log(_log);
    }
    void DebugLogWarninig(string _log)
    {
        if(showDebugLog) Logger.LogWarning(_log);
    }
    void DebugLogError(string _log)
    {
        if(showDebugLog) Logger.LogError(_log);
    }
    

    #region Gizmos
    // ⭐ 에디터에서 공격 범위 시각화
    void OnDrawGizmos()
    {
        // 초기화되지 않았거나 Ranged 타입이 아니면 그리지 않음
        if (!isInitialized)
            return;

        // 공격 범위 원 그리기
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 주황색 반투명
        Gizmos.DrawSphere(transform.position, distanceToPlayer);
        
        // 테두리 (더 진하게)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // 주황색
        DrawWireCircle(transform.position, distanceToPlayer);
        
        // 플레이어가 범위 안에 있는지 표시
        if (Application.isPlaying && enemyBase != null && enemyBase.Target != null)
        {
            Transform playerTransform = enemyBase.Target.transform;
            Vector2 enemyPos = transform.position;
            Vector2 playerPos = playerTransform.position;
            
            float distance = Vector2.Distance(enemyPos, playerPos);
            bool inRange = distance < distanceToPlayer;
            
            // 플레이어까지 선 그리기
            Gizmos.color = inRange ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
            
            // 거리 표시 (Scene 뷰에서만)
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f, 
                $"거리: {distance:F1} / {distanceToPlayer}\n{(inRange ? "범위 안" : "범위 밖")}"
            );
            #endif
        }
    }

    // 원 그리기 헬퍼 함수
    void DrawWireCircle(Vector3 center, float radius, int segments = 64)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    #endregion
}