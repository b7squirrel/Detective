using System.Collections;
using UnityEngine;

public class BossDrillShootBomb : MonoBehaviour
{
    [Header("드릴 애벌레 알 공격")]
    [SerializeField] GameObject eggPrefab;
    [SerializeField] int projectileNums;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    [SerializeField] float projectileSpeed; // 수평 속도
    [SerializeField] float verticalVelocity; // 수직 속도
    [SerializeField] float stateDuration; // 상태 지속 시간
    [SerializeField] float damageRadius; // 데미지 범위
    float stateTimer;

    EnemyBoss enemyBoss;
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

        // 플레이어에게 밀리지 않기
        enemyBoss.SetMovable(false);
    }
    void InitState2Update()
    {
        // 공통 Update
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
            GameObject egg = GameManager.instance.poolManager.GetMisc(eggPrefab);
            egg.transform.position = shootPoint.position;
            egg.GetComponent<BossDrillBomb>().SetDamageRadius(damageRadius);

            // 0~360도 사이의 랜덤 각도
            float randomAngle = Random.Range(0f, 360f);

            Vector2 projectileVelocity = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            ) * projectileSpeed;

            egg.GetComponent<ShadowHeightProjectile>().Initialize(projectileVelocity, verticalVelocity);
            shootCounter--;
            yield return new WaitForSeconds(.1f);
        }
    }
    #endregion
}