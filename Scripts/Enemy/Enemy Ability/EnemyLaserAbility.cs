using UnityEngine;
using System.Collections;

/// <summary>
/// 레이저 능력을 담당하는 컴포넌트
/// 예고 → 발사 → 종료 순서로 작동
/// </summary>
public class EnemyLaserAbility : MonoBehaviour
{
    #region Variables
    [Header("Laser Settings - Runtime Values")]
    float laserCooldown;
    float laserAnticipationTime;
    float laserFireDuration;
    int laserDamage;
    float laserRange;
    float laserWidth;

    float nextLaserTime;
    bool finishedSpawn;
    bool isInitialized;

    public enum LaserState { Idle, Anticipation, Fire, Settle }
    LaserState currentState = LaserState.Idle;

    Vector2 laserDirection;
    float stateEndTime;

    // References
    EnemyBase enemyBase;
    Rigidbody2D rb;
    Animator anim;

    // LineRenderer
    [Header("Visual Components")]
    [SerializeField] LineRenderer anticipationLine;
    [SerializeField] LineRenderer fireLine;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        CreateLineRenderers();
    }

    void OnEnable()
    {
        nextLaserTime = 0f;
        finishedSpawn = false;
        currentState = LaserState.Idle;

        if (anticipationLine != null)
            anticipationLine.enabled = false;
        if (fireLine != null)
            fireLine.enabled = false;
    }

    void Update()
    {
        if (!isInitialized)
            return;

        if (enemyBase == null)
            return;

        UpdateLaserState();
        UpdateLineRenderers();
    }
    #endregion

    #region Initialization
    public void InitLaser(EnemyData data)
    {
        // Logger.Log($"[InitLaser] 호출됨 - specialAbility: {data.specialAbility}");

        if (data.specialAbility != SpecialAbility.Laser)
        {
            // Logger.Log($"[InitLaser] Laser 능력이 아니므로 비활성화");
            isInitialized = false;
            return;
        }

        laserCooldown = data.laserCooldown;
        laserAnticipationTime = data.laserAnticipationTime;
        laserFireDuration = data.laserFireDuration;
        laserDamage = data.laserDamage;
        laserRange = data.laserRange;
        laserWidth = data.laserWidth;

        // 스폰 후 쿨다운 시간 대기
        nextLaserTime = Time.time + laserCooldown;

        // Logger.Log($"[InitLaser] 초기화 완료 - 첫 레이저는 {laserCooldown}초 후");

        isInitialized = true;

        StartCoroutine(AutoFinishSpawnCo());
    }

    IEnumerator AutoFinishSpawnCo()
    {
        yield return new WaitForSeconds(1f);

        if (!finishedSpawn)
        {
            // Logger.Log("[AutoFinishSpawn] 자동으로 스폰 완료 처리");
            finishedSpawn = true;
        }
    }

    public void SetFinishedSpawn(bool finished)
    {
        // Logger.Log($"[SetFinishedSpawn] 호출됨 - finished: {finished}");
        finishedSpawn = finished;
    }
    #endregion

    #region LineRenderer Setup
    void CreateLineRenderers()
    {
        // Logger.Log("[CreateLineRenderers] LineRenderer 생성 시작");

        // 예고선 생성
        GameObject anticipationObj = new GameObject("AnticipationLine");
        anticipationObj.transform.SetParent(transform);
        anticipationObj.transform.localPosition = Vector3.zero;

        anticipationLine = anticipationObj.AddComponent<LineRenderer>();
        SetupLineRenderer(anticipationLine, new Color(1f, 0f, 0f, 0.7f), 0.05f);
        anticipationLine.enabled = false;

        // Logger.Log($"[CreateLineRenderers] anticipationLine 생성 완료: {anticipationLine != null}");

        // 레이저 생성
        GameObject fireObj = new GameObject("FireLine");
        fireObj.transform.SetParent(transform);
        fireObj.transform.localPosition = Vector3.zero;

        fireLine = fireObj.AddComponent<LineRenderer>();
        SetupLineRenderer(fireLine, new Color(1f, 1f, 0.3f, 1f), 0.2f);
        fireLine.enabled = false;
    }

    void SetupLineRenderer(LineRenderer line, Color color, float width)
    {
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;

        // line.material = new Material(Shader.Find("Sprites/Default"));
        // line.material = new Material(Shader.Find("Unlit/Color"));
        line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        line.startColor = color;
        line.endColor = color;

        line.sortingLayerName = "Effect";
        line.sortingOrder = 10;

        line.useWorldSpace = true;

        // Logger.Log($"[SetupLineRenderer] 생성됨 - Color: {color}, Width: {width}");
    }
    #endregion

    #region LineRenderer Update
    void UpdateLineRenderers()
    {
        switch (currentState)
        {
            case LaserState.Idle:
            case LaserState.Settle:
                if (anticipationLine != null)
                    anticipationLine.enabled = false;
                if (fireLine != null)
                    fireLine.enabled = false;
                break;

            case LaserState.Anticipation:
                // Logger.Log("[UpdateLineRenderers] Anticipation - 예고선 표시");
                if (anticipationLine != null)
                {
                    anticipationLine.enabled = true;
                    UpdateLinePosition(anticipationLine);
                    // Logger.Log($"[UpdateLineRenderers] anticipationLine enabled: {anticipationLine.enabled}");
                }
                else
                {
                    // Logger.LogError("[UpdateLineRenderers] anticipationLine이 null!");
                }
                if (fireLine != null)
                    fireLine.enabled = false;
                break;

            case LaserState.Fire:
                // Logger.Log("[UpdateLineRenderers] Fire - 레이저 표시");
                if (anticipationLine != null)
                    anticipationLine.enabled = false;
                if (fireLine != null)
                {
                    fireLine.enabled = true;
                    UpdateLinePosition(fireLine);
                    // Logger.Log($"[UpdateLineRenderers] fireLine enabled: {fireLine.enabled}");
                }
                else
                {
                    // Logger.LogError("[UpdateLineRenderers] fireLine이 null!");
                }
                break;
        }
    }

    void UpdateLinePosition(LineRenderer line)
    {
        if (line == null)
        {
            // Logger.LogError("[UpdateLinePosition] line이 null!");
            return;
        }

        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + laserDirection * laserRange;

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        // Logger.Log($"[UpdateLinePosition] Start: {startPos}, End: {endPos}, Range: {laserRange}");
    }
    #endregion

    #region Laser State Machine
    void UpdateLaserState()
    {
        if (!finishedSpawn)
            return;

        switch (currentState)
        {
            case LaserState.Idle:
                UpdateIdleState();
                break;

            case LaserState.Anticipation:
                UpdateAnticipationState();
                break;

            case LaserState.Fire:
                UpdateFireState();
                break;

            case LaserState.Settle:
                UpdateSettleState();
                break;
        }
    }

    void UpdateIdleState()
    {
        if (Time.time >= nextLaserTime)
        {
            // Logger.Log($"[Laser] Idle → Anticipation");
            StartAnticipation();
        }
    }

    void UpdateAnticipationState()
    {
        if (Time.time >= stateEndTime)
        {
            // Logger.Log($"[Laser] Anticipation → Fire");
            StartFire();
        }
    }

    void UpdateFireState()
    {
        if (Time.time >= stateEndTime)
        {
            // Logger.Log($"[Laser] Fire → Settle");
            StartSettle();
        }
    }

    void UpdateSettleState()
    {
        // Logger.Log($"[Laser] Settle → Idle");
        currentState = LaserState.Idle;
        nextLaserTime = Time.time + laserCooldown;
    }
    #endregion

    #region State Transitions
    void StartAnticipation()
    {
        currentState = LaserState.Anticipation;
        stateEndTime = Time.time + laserAnticipationTime;

        if (enemyBase.Target != null)
        {
            Transform playerTransform = enemyBase.Target.transform;
            laserDirection = (playerTransform.position - transform.position).normalized;

            // Logger.Log($"[StartAnticipation] 방향 고정: {laserDirection}");
        }
        else
        {
            // Logger.LogWarning("[StartAnticipation] player가 null입니다!");
            laserDirection = Vector2.right;
        }

        if (anim != null)
        {
            anim.SetBool("LaserAntic", true);
        }
    }

    void StartFire()
    {
        currentState = LaserState.Fire;
        stateEndTime = Time.time + laserFireDuration;

        // Logger.Log($"[StartFire] 레이저 발사! 방향: {laserDirection}");

        // TODO: Step 4에서 데미지 판정 추가

        if (anim != null)
        {
            anim.SetBool("LaserAntic", false);
            anim.SetBool("LaserFire", true);
        }
    }

    void StartSettle()
    {
        currentState = LaserState.Settle;

        // Logger.Log($"[StartSettle] 레이저 종료");

        if (anim != null)
        {
            anim.SetBool("LaserFire", false);
        }
    }
    #endregion

    #region Public Methods
    public bool IsUsingLaser()
    {
        return currentState != LaserState.Idle;
    }

    public LaserState GetCurrentState()
    {
        return currentState;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (!isInitialized)
            return;

        Gizmos.color = new Color(1f, 0f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, laserRange);

        Gizmos.color = new Color(1f, 0f, 1f, 0.8f);
        DrawWireCircle(transform.position, laserRange);

        if (Application.isPlaying)
        {
            switch (currentState)
            {
                case LaserState.Anticipation:
                    Gizmos.color = Color.red;
                    Vector2 endPos = (Vector2)transform.position + laserDirection * laserRange;
                    Gizmos.DrawLine(transform.position, endPos);

#if UNITY_EDITOR
                    float remainingAntic = stateEndTime - Time.time;
                    UnityEditor.Handles.Label(
                        transform.position + Vector3.up * 2f,
                        $"ANTICIPATION!\n남은 시간: {remainingAntic:F2}s"
                    );
#endif
                    break;

                case LaserState.Fire:
                    Gizmos.color = Color.yellow;
                    Vector2 fireEndPos = (Vector2)transform.position + laserDirection * laserRange;
                    Gizmos.DrawLine(transform.position, fireEndPos);

#if UNITY_EDITOR
                    float remainingFire = stateEndTime - Time.time;
                    UnityEditor.Handles.Label(
                        transform.position + Vector3.up * 2f,
                        $"FIRING!\n남은 시간: {remainingFire:F2}s"
                    );
#endif
                    break;

                case LaserState.Idle:
#if UNITY_EDITOR
                    float timeUntilLaser = nextLaserTime - Time.time;
                    if (finishedSpawn && timeUntilLaser > 0)
                    {
                        UnityEditor.Handles.Label(
                            transform.position + Vector3.up * 2f,
                            $"다음 레이저: {timeUntilLaser:F1}초"
                        );
                    }
#endif
                    break;
            }
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