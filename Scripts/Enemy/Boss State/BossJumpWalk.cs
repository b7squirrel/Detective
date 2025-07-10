using UnityEngine;

public class BossJumpWalk : MonoBehaviour
{
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

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
        enemyBoss.DisplayCurrentState("점퍼 걷기 상태");
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
