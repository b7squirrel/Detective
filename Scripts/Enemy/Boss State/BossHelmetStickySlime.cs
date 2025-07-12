using System.Collections;
using UnityEngine;

/// <summary>
/// 헬멧 슬라임 구슬 발사
/// </summary>
public class BossHelmetStickySlime : MonoBehaviour
{
    EnemyBoss enemyBoss;
    [SerializeField] GameObject jellyPrefab;
    [SerializeField] int projectileNums;
    [SerializeField] int jellyDamage;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    [SerializeField] float stateDuration; // 상태 지속 시간
    float stateTimer;

    Transform shootPoint;
    Coroutine co;
    bool isAttackDone;
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
    #region 상태 함수들
    void InitState2Enter()
    {
        Debug.Log("State2 Enter");
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();
        isAttackDone = false; // 공격을 할 수 있도록 초기화
        enemyBoss.DisplayCurrentState("헬멧 슬라임 구슬 발사 상태");

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);
    }
    void InitState2Update()
    {
        if (stateTimer < stateDuration)
        {
            stateTimer += Time.deltaTime;
        }
        else
        {
            stateTimer = 0f;
            enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
        if (isAttackDone) return;

        co = StartCoroutine(ShootCo());
        Debug.Log("State2 Update");
    }
    void InitState2Exit()
    {
        enemyBoss.SetMovable(true);
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
