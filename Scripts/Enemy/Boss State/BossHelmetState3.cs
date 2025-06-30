using UnityEngine;

public class BossHelmetState3 : MonoBehaviour
{
    EnemyBoss enemyBoss;
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
        enemyBoss.DisplayCurrentState("헬멧 대시 상태");
    }
    void InitState3Update()
    {
        Debug.Log("State3 Update");
    }
    void InitState3Exit()
    {
        Debug.Log("State3 Exit");
    }
}
