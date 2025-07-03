using UnityEngine;

public class BossJumpState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isMoving;

    [SerializeField] float inAirDuration; // 이동할 거리와 상관없이 같은 시간으로 이동
    [SerializeField] float timeToDropSlime;
    float elapsedTIme = 0;

    Vector2 startPos, targetPos;

    int enemyLayer;
    int inAirLayer;

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState3Enter += InitState3Enter;
        EnemyBoss.OnState3Update += InitState3Update;
        EnemyBoss.OnState3Exit += InitState3Exit;

        enemyLayer = LayerMask.NameToLayer("Enemy");
        inAirLayer = LayerMask.NameToLayer("InAir");
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

        elapsedTIme = 0f;

        // 레이어를 "InAir"로 변경. 벽과만 충돌을 하는 레이어
        gameObject.layer = inAirLayer;

        // 시작지점, 도착지점
        startPos = transform.position;
        targetPos = playerTrns.position;

        // 착지 지점 인디케이터
        enemyBoss.ActivateLandingIndicator(true);
    }

    void InitState3Update()
    {
        enemyBoss.SlimeDropTimer(timeToDropSlime);

        if (isMoving)
        {
            InAirMoving();
        }

        Debug.Log("State3 Update");
    }

    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
        enemyBoss.ActivateLandingIndicator(false);

        isMoving = false;
    }

    void InAirMoving()
    {
        if (elapsedTIme < inAirDuration)
        {
            float t = Mathf.Clamp01(elapsedTIme / inAirDuration);
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            elapsedTIme += Time.deltaTime;
        }
        else
        {
            // StateSettle로 들어가야 함
            // animator.SetTrigger("IsClose");
        }
    }

    // 벽에 부딪치면 멈추게 했음. 벽 밖으로 나가는 것을 막기 위해서
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            isMoving = false;
            rb.velocity = Vector2.zero;

            // 레이어를 "Enemy"로 되돌림
            gameObject.layer = enemyLayer;

            Debug.Log("벽에 충돌 - 이동 멈춤");
        }
    }
}