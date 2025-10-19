using UnityEngine;
using System.Collections;

/// <summary>
/// 방향 전환을 주어진 횟수만큼 하면 상태 종료
/// </summary>
public class BossDrillUnderground : MonoBehaviour
{
    [Header("드릴 언더그라운드")]
    [SerializeField] float moveSpeed = 5f; // 이동 속도 (units per second)
    [SerializeField] float timeToDropSlime; // 1000정도로 하면 이 상태에서는 점액을 흘리지 않음
    [SerializeField] float playerCheckInterval = 2f; // 플레이어 위치 체크 간격

    [Header("상태 지속 시간")]
    [SerializeField] int maxDirChangeNum;
    int dirChangeCounter;

    [Header("파티클")]
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] ParticleSystem[] explosiveDirtsPs;

    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isMoving;
    Vector2 targetPos;
    Vector2 moveDirection; // 현재 이동 방향

    // 플레이어 추적용 변수들
    Coroutine playerTrackingCoroutine;

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState3Enter += InitState3Enter;
        EnemyBoss.OnState3Update += InitState3Update;
        EnemyBoss.OnState3Exit += InitState3Exit;
    }

    void OnDisable()
    {
        EnemyBoss.OnState3Enter -= InitState3Enter;
        EnemyBoss.OnState3Update -= InitState3Update;
        EnemyBoss.OnState3Exit -= InitState3Exit;
    }
    #endregion

    void InitState3Enter()
    {
        Debug.Log("State3 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        if (playerTrns == null) playerTrns = GameManager.instance.player.transform;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        isMoving = true;
        enemyBoss.DisplayCurrentState("점퍼 점프");

        // 초기 목표지점과 방향 설정
        targetPos = playerTrns.position;
        SetMoveDirection();

        // 착지 지점 인디케이터
        // enemyBoss.ActivateLandingIndicator(true);

        // 플레이어 추적 코루틴 시작
        if (playerTrackingCoroutine != null)
        {
            StopCoroutine(playerTrackingCoroutine);
        }
        playerTrackingCoroutine = StartCoroutine(TrackPlayerPosition());
    }

    void InitState3Update()
    {
        if (dirChangeCounter > maxDirChangeNum)
        {
            dirChangeCounter = 0;
            GetComponent<Animator>().SetTrigger("Settle");
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
        if (isMoving)
        {
            MoveInDirection();
        }
        Debug.Log("State3 Update");
    }

    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
        // enemyBoss.ActivateLandingIndicator(false);
        isMoving = false;

        // 플레이어 추적 코루틴 정지
        if (playerTrackingCoroutine != null)
        {
            StopCoroutine(playerTrackingCoroutine);
            playerTrackingCoroutine = null;
        }
    }

    // 목표지점을 향한 방향 설정
    void SetMoveDirection()
    {
        moveDirection = (targetPos - (Vector2)transform.position).normalized;
        Debug.Log($"새로운 이동 방향 설정: {moveDirection}, 목표: {targetPos}");
    }

    // 2초마다 플레이어 위치를 체크하는 코루틴
    IEnumerator TrackPlayerPosition()
    {
        while (isMoving)
        {
            yield return new WaitForSeconds(playerCheckInterval);

            // isMoving이 false가 되면 상태 종료.
            // 이 조건문이 없으면 벽에 부딪치면 아래의 isMoving && playerTrns != null을 통과하지 못해서 무한루프에 걸리게 됨 
            if (!isMoving)
            {
                Debug.Log("이동 중단됨 - 상태3 종료");
                dirChangeCounter = 0;
                GetComponent<Animator>().SetTrigger("Settle");
                yield break;
            }
        
            if (isMoving && playerTrns != null)
            {
                // 새로운 목표 지점 설정
                targetPos = playerTrns.position;
                // 새로운 방향으로 업데이트
                SetMoveDirection();
                dirChangeCounter++;
            }
        }
    }

    // 설정된 방향으로 계속 이동
    void MoveInDirection()
    {
        Vector2 newPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = newPosition;
    }

    // 벽에 부딪치면 멈추게 했음. 벽 밖으로 나가는 것을 막기 위해서
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Player"))
        {
            isMoving = false;
            rb.velocity = Vector2.zero;
            Debug.Log("벽에 충돌 - 이동 멈춤");
        }
    }

    // 애니메이션 이벤트
    public void StartParticles()
    {
        foreach (var item in particleSystems)
        {
            item.Play();
        }
    }
    public void StopParticles()
    {
        foreach (var item in particleSystems)
        {
            item.Stop();
        }
    }
    public void StartExplosiveDirtPS()
    {
        foreach (var item in explosiveDirtsPs)
        {
            item.Play();
        }
    }
    public void StopExplosiveDirtPS()
    {
        foreach (var item in explosiveDirtsPs)
        {
            item.Stop();
        }
    }
    public void ShakeCamera()
    {
        CameraShake.instance.Shake();
    }
}