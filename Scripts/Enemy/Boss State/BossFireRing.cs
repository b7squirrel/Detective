using System.Collections;
using UnityEngine;

public class BossFireRing : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;

    [Header("반사 공 설정")]
    public GameObject fireProjectile;   // 반사 공 프리팹
    Transform shootPoint;            // 공 발사 위치
    [SerializeField] int maxProjectileNum;  // 공 개수

    public float ballSpeed = 12f;           // 공 속도
    [SerializeField] float fireInterval = .2f; // 공을 쏘는 시간 간격
    [SerializeField] float randomAngleRange = 20f; // 무작위 각도 범위 (±도 단위)
    public float ballLifetime = 15f;        // 공 생존 시간
    public float fireRate = 100f;             // 발사 간격 (초)
    public int damage = 20;                 // 데미지
    public LayerMask playerLayer;           // 플레이어 레이어
    public LayerMask wallLayer;             // 벽 레이어
    private Transform player;
    private float nextFireTime = 0f;
    Coroutine co;

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState2Enter += InitFireRingEnter;
        EnemyBoss.OnState2Update += InitFireRingUpdate;
        EnemyBoss.OnState2Exit += InitFireRingExit;
    }

    void OnDisable()
    {
        EnemyBoss.OnState2Enter -= InitFireRingEnter;
        EnemyBoss.OnState2Update -= InitFireRingUpdate;
        EnemyBoss.OnState2Exit -= InitFireRingExit;
    }
    #endregion

    void InitFireRingEnter()
    {
        enemyBoss = GetComponent<EnemyBoss>();
        // 플레이어 찾기
        player = GameManager.instance.player.transform;
        nextFireTime = 0;
        if (shootPoint == null) shootPoint = enemyBoss.GetShootPoint();

        if (co != null)
        {
            StopCoroutine(co);
        }

        // 플레이어에게 밀리지 않도록
            enemyBoss.SetMovable(false);
    }

    void InitFireRingUpdate()
    {
        if (Time.time >= nextFireTime)
        {
            co = StartCoroutine(FireCirclePatternCo(10));
            nextFireTime = Time.time + fireRate;
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }

    void InitFireRingExit()
    {
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireCirclePatternCo(int count)
    {
        // 3번 왕복하기
        for (int i = 0; i < 3; i++)
        {
            float leftRightdir = -1f;
            // 좌우 방향
            leftRightdir = Mathf.Pow(leftRightdir, i + 1);
            // 플레이어 방향 계산
            Vector3 playerDirection = (player.position - shootPoint.position).normalized;
            float playerAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

            int currentProjectileNums = 0;
            while (currentProjectileNums < count)
            {
                // 플레이어 방향을 기준으로 균등한 원형 패턴
                float angle = playerAngle + (leftRightdir * (360f * currentProjectileNums / count));

                Vector3 newDir = Quaternion.Euler(0, 0, angle) * Vector3.right;

                GameObject ball = Instantiate(fireProjectile, shootPoint.position, Quaternion.identity);
                ball.GetComponent<EnemyFireProjectile>().Initialize(damage, ballSpeed, newDir);
                currentProjectileNums++;
                Debug.Log($"투사체 발사 - 각도: {angle:F1}도");
                yield return new WaitForSeconds(fireInterval);
            }
            yield return new WaitForSeconds(.2f);
        }
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }
}