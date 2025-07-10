using UnityEngine;

public class BossFireWalk : MonoBehaviour
{
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState1Enter += InitWalkEnter;
        EnemyBoss.OnState1Update += InitWalkUpdate;
        EnemyBoss.OnState1Exit += InitWalkExit;
    }
    void OnDisable()
    {
        EnemyBoss.OnState1Enter -= InitWalkEnter;
        EnemyBoss.OnState1Update -= InitWalkUpdate;
        EnemyBoss.OnState1Exit -= InitWalkExit;
    }
    #endregion
    
    void InitWalkEnter()
    {
        Debug.Log("State1 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        enemyBoss.DisplayCurrentState("점퍼 걷기 상태");
    }
    void InitWalkUpdate()
    {
        if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
        enemyBase.Flip();
        enemyBase.ApplyMovement();
        enemyBoss.ShootTimer();
        enemyBoss.SlimeDropTimer(timeToDropSlime);
        Debug.Log("State1 Update");
    }
    void InitWalkExit()
    {
        Debug.Log("State1 Exit");
    }
}
