using UnityEngine;

public class BossHelmetState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isDirectionSet; // 한 번 정해진 방향으로 대시하도록
    Vector2 dirVec; // 대시 방향
    [SerializeField] float dashSpeed;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

    // Bouncing back
    bool isBouncing = false;
    Vector2 bounceStartPos;
    Vector2 bounceTargetPos;
    float bounceTime = 0f;
    float bounceDuration = 0.2f; // 튕겨나기 애니메이션 시간

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
        isBouncing = false;
        enemyBoss.DisplayCurrentState("헬멧 대시 상태");
    }
    void InitState3Update()
    {
        // 슬라임 떨어뜨리는 타이머는 계속 작동
        enemyBoss.SlimeDropTimer(timeToDropSlime);
        BounceBack();
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
        if (isBouncing) return; // 뒤로 튕겨나는 중이면 대시하지 않음
        if (isDirectionSet == false) // 방향이 한 번 정해지면 다음 대시 전까지는 바뀌지 않도록
        {
            dirVec = playerTrns.position - transform.position;
            isDirectionSet = true;
        }

        Vector2 nextVec = dashSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }
    void BounceBack()
    {
        // 반동 중이면 대시 중단, 대신 Lerp로 이동 처리
        if (isBouncing)
        {
            bounceTime += Time.deltaTime;
            float t = Mathf.Clamp01(bounceTime / bounceDuration);
            rb.MovePosition(Vector2.Lerp(bounceStartPos, bounceTargetPos, t));

            if (t >= 1f)
            {
                isBouncing = false;
                enemyBoss.SetTriggerTo("Settle");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(dirVec.normalized, normal);
            float bounceDistance = 0.5f;

            bounceStartPos = rb.position;
            bounceTargetPos = rb.position + reflectDir * bounceDistance;
            bounceTime = 0f;
            isBouncing = true;

            rb.velocity = Vector2.zero;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Hit");

            Vector2 awayFromPlayer = (transform.position - playerTrns.position).normalized;
            float bounceDistance = 0.3f;

            bounceStartPos = rb.position;
            bounceTargetPos = rb.position + awayFromPlayer * bounceDistance;
            bounceTime = 0f;
            isBouncing = true;

            rb.velocity = Vector2.zero;
        }
    }
    #endregion
}
