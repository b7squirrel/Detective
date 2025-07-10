using UnityEngine;

public class BossHelmetDashSettle : MonoBehaviour
{
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;
    Rigidbody2D rb;

    // Bouncing back
    Vector2 bounceStartPos;
    Vector2 bounceTargetPos;
    float bounceTime = 0f;
    float bounceDuration = .3f; // 튕겨나기 애니메이션 시간
    [SerializeField] float bounceDistance;
    bool isBouncingDone; 

    #region 상태 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState3SettleEnter += InitState3SettleEnter;
        EnemyBoss.OnState3SettleUpdate += InitState3SettleUpdate;
        EnemyBoss.OnState3SettleExit += InitState3settleExit;
    }
    void OnDisable()
    {
        EnemyBoss.OnState3SettleEnter -= InitState3SettleEnter;
        EnemyBoss.OnState3SettleUpdate -= InitState3SettleUpdate;
        EnemyBoss.OnState3SettleExit -= InitState3settleExit;
    }
    #endregion

    void InitState3SettleEnter()
    {
        Debug.Log("State1 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        bounceStartPos = rb.position;
        bounceTargetPos = rb.position + enemyBoss.GetPrevDir() * bounceDistance;
        bounceTime = 0f;
        isBouncingDone = false;

        enemyBoss.DisplayCurrentState("헬멧 슬라임 걷기 상태");
    }
    void InitState3SettleUpdate()
    {
        BounceBack();
        Debug.Log("State1 Update");
    }
    void InitState3settleExit()
    {
        Debug.Log("State1 Exit");
    }

    #region 기타 함수들
    void BounceBack()
    {
        // if (isBouncingDone) return;

        bounceTime += Time.deltaTime;
        float t = Mathf.Clamp01(bounceTime / bounceDuration);
        rb.MovePosition(Vector2.Lerp(bounceStartPos, bounceTargetPos, t));

        // if (t >= 1f)
        // {
        //     isBouncingDone = true;
        // }
    }
    #endregion
}