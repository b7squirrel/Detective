using System.Collections;
using UnityEngine;

public class BossJumpState2 : MonoBehaviour
{
    EnemyBoss enemyBoss;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] int projectileNums;
    [SerializeField] int jellyDamage;
    [SerializeField] float projectilSpeed;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기

    [Header("SFX")]
    [SerializeField] AudioClip projectileSFX;

    Coroutine co;
    bool isAttackDone;
    Transform playerTrns;

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
        enemyBoss.DisplayCurrentState("헬멧 슬라임 구슬 발사 상태");

        playerTrns = GameManager.instance.player.transform;
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
        for (int i = 0; i < projectileNums; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.GetComponent<SlimeDropProjectile>().InitProjectile(transform.position, playerTrns.position, projectilSpeed);
            SoundManager.instance.Play(projectileSFX);
            yield return new WaitForSeconds(.1f);
        }
        co = StartCoroutine(ShootFinishedCo());
    }

    IEnumerator ShootFinishedCo()
    {
        yield return new WaitForSeconds(1f);
        // State2Settle 로 가야함
        // anim.SetBool("ShootFinished", true);
    }
    public void StopShooting()
    {
        if (co == null)
            return;
        StopCoroutine(co);
    }
    #endregion
}
