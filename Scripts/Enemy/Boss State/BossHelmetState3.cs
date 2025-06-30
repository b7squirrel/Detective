using UnityEngine;

public class BossHelmetState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isDirectionSet; // 한 번 정해진 방향으로 대시하도록
    Vector2 dirVec; // 대시 방향
    [SerializeField] float dashSpeed;

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
    void InitState3Enter()
    {
        Debug.Log("State3 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        if (playerTrns == null) playerTrns = GameManager.instance.player.transform;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        isDirectionSet = false;
        enemyBoss.DisplayCurrentState("헬멧 대시 상태");
    }
    void InitState3Update()
    {
        Dash();
        Debug.Log("State3 Update");
    }
    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
    }

    #region 공격 관련 함수
    void Dash()
    {
        if (isDirectionSet == false) // 방향이 한 번 정해지면 다음 대시 전까지는 바뀌지 않도록
        {
            dirVec = playerTrns.position - transform.position;
            isDirectionSet = true;
        }

        Vector2 nextVec = dashSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    // 벽이나 반사 가능한 오브젝트에만 반응하도록 태그 체크 (선택 사항)
    if (collision.collider.CompareTag("Wall"))
    {
        // 충돌 지점의 법선 벡터 사용 (보통 첫 번째 ContactPoint 사용)
        Vector2 normal = collision.contacts[0].normal;

        // 현재 방향을 기준으로 반사 벡터 계산
        dirVec = Vector2.Reflect(dirVec.normalized, normal);

        // 반사 방향으로 계속 대시하도록 설정
        isDirectionSet = true;
    }
}
    #endregion
}
