using UnityEngine;

public class BossHelmetState2 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    void OnEnable()
    {
        EnemyBoss.OnState2Enter += InitState2Enter;
        EnemyBoss.OnState2Update += InitState2Update;
        EnemyBoss.OnState2Exit += InitState2Exit;
    }
    void OnDisable()
    {
        EnemyBoss.OnState2Enter -= InitState2Enter;
        EnemyBoss.OnState2Update -= InitState2Update;
        EnemyBoss.OnState2Exit -= InitState2Exit;
    }
    void InitState2Enter()
    {
        Debug.Log("State2 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        enemyBoss.DisplayCurrentState("헬멧 슬라임 구슬 발사 상태");
    }
    void InitState2Update()
    {
        Debug.Log("State2 Update");
    }
    void InitState2Exit()
    {
        Debug.Log("State3 Exit");
    }
}
