using UnityEngine;

public class BossDrillWalk : MonoBehaviour
{
    [Header("드릴 걷기")]
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState1Enter += InitState1Enter;
        EnemyBoss.OnState1Update += InitState1Update;
        EnemyBoss.OnState1Exit += InitState1Exit;
    }
    void OnDisable()
    {
        EnemyBoss.OnState1Enter -= InitState1Enter;
        EnemyBoss.OnState1Update -= InitState1Update;
        EnemyBoss.OnState1Exit -= InitState1Exit;
    }
    #endregion
    
    void InitState1Enter()
    {
        Debug.Log("State1 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        enemyBoss.DisplayCurrentState("드릴 슬라임 걷기 상태");
    }
    void InitState1Update()
    {
        if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
        enemyBase.Flip();
        enemyBase.ApplyMovement();
        enemyBoss.ShootTimer();
        enemyBoss.SlimeDropTimer(timeToDropSlime);
        Debug.Log("State1 Update");
    }
    void InitState1Exit()
    {
        Debug.Log("State1 Exit");
    }
}
