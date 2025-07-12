using System.Collections;
using UnityEngine;

// 투사체를 모두 발사하면 상태 종료
public class BossLaserReflectionLaser : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;
    [Header("반사 공 설정")]
    public GameObject bouncingBallPrefab;   // 반사 공 프리팹
    public Transform shootPoint;            // 공 발사 위치
    [SerializeField] int maxProjectileNum;  // 공 개수 
    public float ballSpeed = 12f;           // 공 속도
    [SerializeField] float randomAngleRange = 20f; // 무작위 각도 범위 (±도 단위)
    public float ballLifetime = 15f;        // 공 생존 시간
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
    #endregion

    void InitState3Enter()
    {
        enemyBoss = GetComponent<EnemyBoss>();
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("플레이어를 찾을 수 없습니다!");
        }

        // shootPoint가 설정되지 않은 경우 자신의 위치 사용
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);
    }
    void InitState3Update()
    {
        if (Time.time >= nextFireTime && player != null)
        {
            StartCoroutine(FireBouncingBallCo());
            nextFireTime = Time.time + fireRate;
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }
    void InitState3Exit()
    {
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireBouncingBallCo()
    {
        int currentProjectileNums = maxProjectileNum;
        while (currentProjectileNums >= 0)
        {
            Vector3 baseDirection = (player.position - shootPoint.position).normalized;

            // 무작위 각도 회전
            float randomAngle = Random.Range(-randomAngleRange, randomAngleRange);
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);

            // 회전된 방향 적용
            Vector3 direction = rotation * baseDirection;

            // 공 생성
            GameObject ball = Instantiate(bouncingBallPrefab, shootPoint.position, Quaternion.identity);

            // 공 설정
            EnemyBouncingLaserProjectile ballScript = ball.GetComponent<EnemyBouncingLaserProjectile>();
            if (ballScript == null)
            {
                ballScript = ball.AddComponent<EnemyBouncingLaserProjectile>();
            }

            ballScript.Initialize(direction, ballSpeed, damage, ballLifetime);

            currentProjectileNums--;

            yield return new WaitForSeconds(.03f);
        }
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }
    void FireBouncingBall()
    {
        // 플레이어 방향으로 발사
        Vector3 direction = (player.position - shootPoint.position).normalized;

        // 공 생성
        GameObject ball = Instantiate(bouncingBallPrefab, shootPoint.position, Quaternion.identity);

        // 공 설정
        EnemyBouncingLaserProjectile ballScript = ball.GetComponent<EnemyBouncingLaserProjectile>();
        if (ballScript == null)
        {
            ballScript = ball.AddComponent<EnemyBouncingLaserProjectile>();
        }

        ballScript.Initialize(direction, ballSpeed, damage, ballLifetime);

        Debug.Log("반사 공 발사!");
    }
}
