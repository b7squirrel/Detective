using UnityEngine;

public class BossDrillState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isDirectionSet; // 한 번 정해진 방향으로 대시하도록
    bool isMoving; // 현재 이동 중인지 확인
    Vector2 dirVec; // 대시 방향
    [SerializeField] float dashSpeed;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

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
        
        isDirectionSet = false;
        isMoving = true;
        enemyBoss.DisplayCurrentState("드릴 터널 이동");
    }

    void InitState3Update()
    {
        enemyBoss.SlimeDropTimer(timeToDropSlime);
        
        if (isMoving)
        {
            UndergroundDash();
        }
        
        Debug.Log("State3 Update");
    }

    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
        isMoving = false;
    }

    #region 공격 관련 함수
    void UndergroundDash()
    {
        if (isDirectionSet == false) // 방향이 한 번 정해지면 다음 대시 전까지는 바뀌지 않도록
        {
            dirVec = (playerTrns.position - transform.position).normalized;
            isDirectionSet = true;
        }

        if (isMoving)
        {
            Vector2 nextVec = dashSpeed * Time.fixedDeltaTime * dirVec;
            rb.MovePosition((Vector2)rb.transform.position + nextVec);
            rb.velocity = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽에 충돌하면 이동 멈춤
        if (collision.collider.CompareTag("Wall"))
        {
            isMoving = false;
            rb.velocity = Vector2.zero;
            Debug.Log("벽에 충돌 - 이동 멈춤");
        }

        // 플레이어와 충돌
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
            if (Time.frameCount % 3 == 0) 
            {
                GameManager.instance.character.TakeDamage(enemyBoss.Stats.damage, EnemyType.Melee);
            }
        }
    }
    #endregion
}