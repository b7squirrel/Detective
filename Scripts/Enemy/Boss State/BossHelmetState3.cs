using UnityEngine;

public class BossHelmetState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;
    Transform playerTrns;
    Rigidbody2D rb;
    bool isDirectionSet; // 한 번 정해진 방향으로 대시하도록
    Vector2 dirVec; // 대시 방향
    [SerializeField] float dashSpeed;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

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
        if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
        if (playerTrns == null) playerTrns = GameManager.instance.player.transform;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        isDirectionSet = false;
        enemyBoss.DisplayCurrentState("헬멧 대시 상태");
    }
    void InitState3Update()
    {
        // 슬라임 떨어뜨리는 타이머는 계속 작동
        enemyBoss.SlimeDropTimer(timeToDropSlime);
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
            Debug.Log("방향 설정");
            isDirectionSet = true;

            enemyBase.Flip();
        }

        Vector2 nextVec = dashSpeed * Time.fixedDeltaTime * dirVec.normalized;
        rb.MovePosition((Vector2)rb.transform.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(dirVec.normalized, normal);
            enemyBoss.SetPrevDir(reflectDir); // enemyBoss의 prevDir변수에 저장해 두고 Settle 상태에서 불러다가 사용.

            rb.velocity = Vector2.zero;
            enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Hit");

            Vector2 awayFromPlayer = (transform.position - playerTrns.position).normalized;
            enemyBoss.SetPrevDir(awayFromPlayer); // enemyBoss의 prevDir변수에 저장해 두고 Settle 상태에서 불러다가 사용.

            rb.velocity = Vector2.zero;
            enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
            return;
        }
    }
    #endregion
}
