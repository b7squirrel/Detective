using System.Collections;
using UnityEngine;

public class BossDrillState2 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    [SerializeField] GameObject jellyPrefab;
    [SerializeField] int projectileNums;
    [SerializeField] int jellyDamage;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

    Transform shootPoint;
    Coroutine co;
    bool isAttackDone;

    #region 액션 이벤트
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
    #endregion
    
    #region 상태 함수들
    void InitState2Enter()
    {
        Debug.Log("State2 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        isAttackDone = false; // 공격을 할 수 있도록 초기화
        enemyBoss.DisplayCurrentState("드릴 알 쏘기");
    }
    void InitState2Update()
    {
        enemyBoss.SlimeDropTimer(timeToDropSlime);
        if (isAttackDone) return;

        co = StartCoroutine(ShootCo());
        Debug.Log("State2 Update");
    }
    void InitState2Exit()
    {
        Debug.Log("State3 Exit");
    }
    #endregion

    #region 공격 관련 함수들
    IEnumerator ShootCo()
    {
        isAttackDone = true;

        if (shootPoint == null) shootPoint = enemyBoss.GetShootPoint();

        int shootCounter = projectileNums;
        while (shootCounter > 0)
        {
            GameObject powder = GameManager.instance.poolManager.GetMisc(jellyPrefab);
            powder.transform.position = shootPoint.position;
            powder.GetComponent<EnemyProjectile>().Init(jellyDamage, GetProjectileDir());
            shootCounter--;
            yield return new WaitForSeconds(.1f);
        }
    }
    Vector2 GetProjectileDir()
    {
        // shooter에서 player를 바라보는 방향 벡터 계산
        Vector2 directionToPlayer =
            (GameManager.instance.player.transform.position - transform.position).normalized;

        // 발사체 궤도를 랜덤하게 하기 위해 -30도에서 +30도 사이의 랜덤 각도 생성
        float randomAngle = Random.Range(-30f, 30f);

        // directionToPlayer 벡터를 Z축 기준으로 randomAngle만큼 회전하는 Quaternion 생성
        Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

        // shooter에서 player를 바라보는 방향에 randomRotation 적용
        Vector2 randomDirection = randomRotation * directionToPlayer;

        return randomDirection;
    }
    #endregion
}
