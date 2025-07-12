using System.Collections;
using UnityEngine;

public class BossFireHoming : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;

    [Header("호밍 설정")]
    public GameObject homingBallPrefab;   // 유도 공 프리팹
    public Transform shootPoint;            // 공 발사 위치
    [SerializeField] int maxProjectileNum;  // 공 개수 
    public float ballSpeed = 12f;           // 공 속도
    [SerializeField] float homingDelay; // 발사된 후 바로 따라가지 않고 일정 시간동안 초기 방향으로 날아가기
    [SerializeField] float randomAngleRange = 20f; // 무작위 각도 범위 (±도 단위)
    [SerializeField] float fireInterval=.5f; // 발사 시간 간격
    public float ballLifetime = 7f;        // 공 생존 시간
    public float fireRate = 4f;             // 발사 간격 (초)
    public int damage = 20;                 // 데미지
    public LayerMask playerLayer;           // 플레이어 레이어
    public LayerMask wallLayer;             // 벽 레이어

    [Header("타겟 설정")]
    public string playerTag = "Player";     // 플레이어 태그

    private Transform player;
    private float nextFireTime = 0f;

    #region 액션 이벤트
    void OnEnable()
    {
        EnemyBoss.OnState3Enter += InitHomingEnter;
        EnemyBoss.OnState3Update += InitHomingUpdate;
        EnemyBoss.OnState3Exit += InitHomingExit;
    }

    void OnDisable()
    {
        EnemyBoss.OnState3Enter -= InitHomingEnter;
        EnemyBoss.OnState3Update -= InitHomingUpdate;
        EnemyBoss.OnState3Exit -= InitHomingExit;
    }
    #endregion

    void InitHomingEnter()
    {
        enemyBoss = GetComponent<EnemyBoss>();
        // 플레이어 찾기
        if (player == null) player = GameManager.instance.player.transform;

        // shootPoint가 설정되지 않은 경우 자신의 위치 사용
        if (shootPoint == null)
        {
            shootPoint = enemyBoss.GetShootPoint();
        }

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);
    }
    void InitHomingUpdate()
    {
        if (Time.time >= nextFireTime && player != null)
        {
            StartCoroutine(FireCirclePatternCo(10));
            nextFireTime = Time.time + fireRate;
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }
    void InitHomingExit()
    {
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireCirclePatternCo(int count)
    {
        // 플레이어 방향 계산
        Vector3 playerDirection = (player.position - shootPoint.position).normalized;
        float playerAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

        int currentProjectileNums = 0;
        while (currentProjectileNums < count)
        {
            // 플레이어 방향을 기준으로 균등한 원형 패턴
            float angle = playerAngle + (360f * currentProjectileNums / count);

            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

            GameObject ball = Instantiate(homingBallPrefab, shootPoint.position, Quaternion.identity);
            
            // EnemyHomingProjectile 컴포넌트 사용
            EnemyHomingProjectile homingScript = ball.GetComponent<EnemyHomingProjectile>();
            if (homingScript != null)
            {
                // 초기 방향과 함께 초기화
                homingScript.Initialize(damage, ballLifetime, homingDelay);
                
                // 속도 설정 (EnemyHomingProjectile의 speed 값 덮어쓰기)
                homingScript.speed = ballSpeed;
            }
            else
            {
                Debug.LogError("EnemyHomingProjectile 컴포넌트를 찾을 수 없습니다!");
            }

            currentProjectileNums++;
            Debug.Log($"호밍 투사체 발사 - 각도: {angle:F1}도");
            yield return new WaitForSeconds(fireInterval);
        }

        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }
}