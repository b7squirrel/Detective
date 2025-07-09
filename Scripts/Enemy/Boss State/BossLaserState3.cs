using UnityEngine;

public class BossLaserState3 : MonoBehaviour
{
    [Header("반사 레이저 설정")]
    public GameObject bouncingLaserPrefab;  // 반사 레이저 프리팹
    public Transform shootPoint;            // 레이저 발사 위치
    public float laserSpeed = 15f;          // 레이저 속도
    public float laserLifetime = 10f;       // 레이저 생존 시간
    public float fireRate = 3f;             // 발사 간격 (초)
    public int maxBounces = 5;              // 최대 반사 횟수
    public float damage = 15f;              // 데미지
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
    }
    void InitState3Update()
    {
        if (Time.time >= nextFireTime && player != null)
        {
            FireBouncingLaser();
            nextFireTime = Time.time + fireRate;
        }
    }
    void InitState3Exit()
    {

    }
    
    void FireBouncingLaser()
    {
        // 플레이어 방향으로 발사
        Vector3 direction = (player.position - shootPoint.position).normalized;
        
        // 레이저 생성
        GameObject laser = Instantiate(bouncingLaserPrefab, shootPoint.position, Quaternion.identity);
        
        // 레이저 설정
        EnemyBouncingLaserProjectile laserScript = laser.GetComponent<EnemyBouncingLaserProjectile>();
        if (laserScript == null)
        {
            laserScript = laser.AddComponent<EnemyBouncingLaserProjectile>();
        }
        
        laserScript.Initialize(direction, laserSpeed, damage, maxBounces, laserLifetime, playerLayer, wallLayer);
        
        Debug.Log("반사 레이저 발사!");
    }
}
