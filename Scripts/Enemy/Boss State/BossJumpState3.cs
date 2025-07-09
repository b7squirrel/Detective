using System.Collections;
using UnityEngine;

public class BossJumpState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isMoving;
    bool hasTriggeredSettle = false; // Settle이 이미 호출되었는지 확인

    [SerializeField] float inAirDuration; // 이동할 거리와 상관없이 같은 시간으로 이동
    [SerializeField] float timeToDropSlime; // 1000정도로 하면 이 상태에서는 점액을 흘리지 않음
    float elapsedTime = 0;
    Coroutine co;

    Vector2 startPos, targetPos;

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
        hasTriggeredSettle = false; // 초기화
        enemyBoss.DisplayCurrentState("점퍼 점프");

        elapsedTime = 0f; // 오타 수정

        // 시작지점, 도착지점
        startPos = transform.position;
        targetPos = playerTrns.position;

        // 착지 지점 인디케이터
        enemyBoss.ActivateLandingIndicator(true);

        // 기존 코루틴이 있다면 중지
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
    }

    void InitState3Update()
    {
        enemyBoss.SlimeDropTimer(timeToDropSlime);

        if (isMoving && !hasTriggeredSettle)
        {
            InAirMoving();
        }

        Debug.Log($"State3 Update - isMoving: {isMoving}, hasTriggeredSettle: {hasTriggeredSettle}, elapsedTime: {elapsedTime:F2}");
    }

    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
        enemyBoss.ActivateLandingIndicator(false);

        isMoving = false;
        hasTriggeredSettle = false;
        
        // 코루틴이 실행 중이면 중지
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
    }

    void InAirMoving()
    {
        if (elapsedTime < inAirDuration)
        {
            float t = Mathf.Clamp01(elapsedTime / inAirDuration);
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            
            Debug.Log($"Moving: t={t:F2}, position={transform.position}");
        }
        else
        {
            // 목표 지점에 정확히 도달
            transform.position = targetPos;
            isMoving = false;
            
            if (!hasTriggeredSettle)
            {
                hasTriggeredSettle = true;
                co = StartCoroutine(SettleCo());
                Debug.Log("목표 지점 도달 - Settle 트리거 시작");
            }
        }
    }
    
    IEnumerator SettleCo()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (enemyBoss != null)
        {
            Debug.Log("Settle 트리거 실행");
            enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
        }
        
        co = null;
    }

    // 벽에 부딪치면 멈추게 했음. 벽 밖으로 나가는 것을 막기 위해서
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("벽에 충돌 - 강제로 Settle 트리거");

            isMoving = false;
            rb.velocity = Vector2.zero;

            // 벽에 부딪치면 그냥 멈춰 있도록 했음. 그렇지 않으면 벽에서 빠져나오지 못해서 계속 점프만 계속하게 됨
            // // 벽에 충돌했을 때도 Settle 트리거
            // if (!hasTriggeredSettle)
            // {
            //     hasTriggeredSettle = true;
            //     co = StartCoroutine(SettleCo());
            // }
        }
    }
}