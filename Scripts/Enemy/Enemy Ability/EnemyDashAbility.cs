using UnityEngine;
using System.Collections;

/// <summary>
/// 대시 능력을 담당하는 컴포넌트 (시간 기반)
/// 일정 시간 동안 플레이어 방향으로 빠르게 돌진
/// </summary>
public class EnemyDashAbility : MonoBehaviour
{
    #region Variables
    [Header("Dash Settings - Runtime Values")]
    float dashCooldown;      // EnemyData에서 받아옴
    float dashSpeed;         // EnemyData에서 받아옴
    float dashDuration;      // EnemyData에서 받아옴
    
    float nextDashTime;
    bool finishedSpawn;
    bool isInitialized;
    bool isDashing;
    
    Vector2 dashDirection;
    float dashEndTime;       // 대시 종료 시간
    
    // References
    EnemyBase enemyBase;
    Rigidbody2D rb;
    Animator anim;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        nextDashTime = 0f;
        finishedSpawn = false;
        isDashing = false;
    }

    void FixedUpdate()
    {
        if (!isInitialized)
            return;
            
        if (enemyBase == null)
            return;

        if (isDashing)
        {
            ApplyDashMovement();
        }
        else
        {
            DashCoolDown();
        }
    }
    #endregion

    #region Initialization
    public void InitDash(EnemyData data)
    {
        Debug.Log($"[InitDash] 호출됨 - specialAbility: {data.specialAbility}");
        
        if (data.specialAbility != SpecialAbility.Dash)
        {
            Debug.Log($"[InitDash] Dash 능력이 아니므로 비활성화");
            isInitialized = false;
            return;
        }

        dashCooldown = data.dashCooldown;
        dashSpeed = data.dashSpeed;
        dashDuration = data.dashDuration; 
        
        Debug.Log($"[InitDash] 초기화 완료 - Cooldown: {dashCooldown}s, Speed: {dashSpeed}, Duration: {dashDuration}s");
        
        isInitialized = true;
        
        StartCoroutine(AutoFinishSpawnCo());
    }

    IEnumerator AutoFinishSpawnCo()
    {
        yield return new WaitForSeconds(1f);
        
        if (!finishedSpawn)
        {
            Debug.Log("[AutoFinishSpawn] 자동으로 스폰 완료 처리");
            finishedSpawn = true;
        }
    }

    public void SetFinishedSpawn(bool finished)
    {
        Debug.Log($"[SetFinishedSpawn] 호출됨 - finished: {finished}");
        finishedSpawn = finished;
    }
    #endregion

    #region Dash Logic
    void DashCoolDown()
    {
        if (!finishedSpawn)
            return;

        if (isDashing)
            return;

        if (Time.time >= nextDashTime)
        {
            Debug.Log($"[DashCoolDown] 대시 시간 도달 - Time: {Time.time}");
            StartDash();
            nextDashTime = Time.time + dashCooldown;
        }
    }

    void StartDash()
{
    if (enemyBase.Target == null)
    {
        Debug.LogWarning("[StartDash] player가 null입니다!");
        return;
    }

    Transform playerTransform = enemyBase.Target.transform;
    
    dashDirection = (playerTransform.position - transform.position).normalized;
    
    isDashing = true;
    dashEndTime = Time.time + dashDuration;
    
    // ⭐ 디버그: 대시 전 속도 확인
    Debug.Log($"[StartDash] 대시 시작! 현재 EnemyBase.currentSpeed: {enemyBase.GetCurrentSpeed()}");
    
    if (anim != null)
    {
        anim.SetBool("Dash", true);
    }
}

void EndDash()
{
    isDashing = false;
    
    // 디버그: 대시 종료 후 속도 확인
    Debug.Log($"[EndDash] 대시 종료! 현재 EnemyBase.currentSpeed: {enemyBase.GetCurrentSpeed()}");
    
    if (anim != null)
    {
        anim.SetBool("Dash", false);
    }
}

    void ApplyDashMovement()
    {
        // ⭐ 시간 기반: 시간으로 종료 체크
        if (Time.time >= dashEndTime)
        {
            Debug.Log($"[ApplyDashMovement] 대시 시간 종료!");
            EndDash();
            return;
        }

        // 대시 이동 적용
        rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
    }

    public bool IsDashing()
    {
        return isDashing;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (!isInitialized)
            return;

        // 대시 예상 거리 계산
        float expectedDistance = dashSpeed * dashDuration;

        // 대시 예상 거리 표시 (원)
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, expectedDistance);
        
        // 테두리
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        DrawWireCircle(transform.position, expectedDistance);
        
        // 대시 중이라면
        if (Application.isPlaying && isDashing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, dashDirection * expectedDistance);
            
            #if UNITY_EDITOR
            float remainingTime = dashEndTime - Time.time;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f, 
                $"DASHING!\n남은 시간: {remainingTime:F2}s / {dashDuration}s"
            );
            #endif
        }
        // 대시 예상 경로
        else if (Application.isPlaying && enemyBase != null && enemyBase.Target != null)
        {
            Transform playerTransform = enemyBase.Target.transform;
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Vector2 dashEndPos = (Vector2)transform.position + dirToPlayer * expectedDistance;
            
            Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawLine(transform.position, dashEndPos);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(dashEndPos, 0.5f);
            
            #if UNITY_EDITOR
            float timeUntilDash = nextDashTime - Time.time;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f, 
                $"다음 대시: {Mathf.Max(0, timeUntilDash):F1}초\n지속: {dashDuration}s (거리: {expectedDistance:F1})"
            );
            #endif
        }
    }

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