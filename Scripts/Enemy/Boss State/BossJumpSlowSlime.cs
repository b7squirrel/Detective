using System.Collections;
using UnityEngine;

public class BossJumpSlowSlime : MonoBehaviour
{
    EnemyBoss enemyBoss;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] int projectileNums;
    [SerializeField] int jellyDamage;
    [SerializeField] float projectilSpeed;
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    [SerializeField] float projectileOffset; // 슬라임이 착지할 지점 offset
    [SerializeField] float stateDuration; // 상태 지속 시간
    float stateTimer;

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
        enemyBoss.DisplayCurrentState("점퍼 슈팅");

        playerTrns = GameManager.instance.player.transform;

        co = StartCoroutine(ShootCo());
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

        Debug.Log("State2 Update");
    }
    void InitState2Exit()
    {
        StopShooting();
        Debug.Log("State3 Exit");
    }
    #endregion

    #region 공격 관련 함수들
    IEnumerator ShootCo()
    {
        int projectileCounter = projectileNums;
        while (projectileCounter > 0)
        {
            GameObject projectile = Instantiate(projectilePrefab);

            float offsetX = UnityEngine.Random.Range(-1 * projectileOffset, projectileOffset);
            float offsetY = UnityEngine.Random.Range(-1 * projectileOffset, projectileOffset);

            Vector2 offsetPos = (Vector2)playerTrns.position + new Vector2(offsetX, offsetY);
            projectile.GetComponent<SlimeDropProjectile>().InitProjectile(transform.position, offsetPos, projectilSpeed);
            SoundManager.instance.Play(projectileSFX);
            projectileCounter--;
            yield return new WaitForSeconds(.1f);
        }
    }

    public void StopShooting()
    {
        if (co == null)
            return;
        StopCoroutine(co);
        co = null;
    }
    #endregion
}
