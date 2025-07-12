using System.Collections;
using UnityEngine;

/// <summary>
/// 레이져의 지속 시간에 따라 상태의 지속시간이 결정됨
/// </summary>
public class BossLaserThickLaser : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;

    [Header("레이저 설정")]
    public GameObject laserProjectile;  // 레이저 프리팹
    public Transform shootPoint;        // 레이저 발사 위치
    public float laserDuration = 5f;    // 레이저 지속 시간
    public float fireRate = 3f;         // 발사 간격 (초)
    public float rotationSpeed = 40f;   // 회전 속도 (도/초)
    public float rotationDelay = 2f;    // 회전 시작 지연 시간 (초)
    public float retargetDelay = 2f;    // 재타겟팅 대기 시간 (초)
    public float laserWidth = 2f;       // 레이저 두께
    public int damage = 10;          // 데미지
    public LayerMask destructables;     // 파괴 가능한 레이어
    public LayerMask walls;             // 벽 레이어
    Coroutine co;

    [Header("타겟 설정")]
    public string playerTag = "Player"; // 플레이어 태그
    private Transform player;
    private float nextFireTime = 0f;
    private GameObject currentLaser;
    private bool isLaserActive = false;
    private float currentAngle = 0f;    // 현재 레이저 발사 각도
    private float laserStartTime = 0f;  // 레이저 발사 시작 시간
    private bool isRotating = false;    // 회전 중인지 확인
    private int rotationDirection = 1;  // 회전 방향 (1: 시계방향, -1: 반시계방향)
    private bool hasPassedTarget = false;   // 타겟을 지나쳤는지 여부
    private float targetAngle = 0f;         // 목표 각도
    private float passedTargetTime = 0f;    // 타겟을 지나친 시간

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
    void InitState2Enter()
    {
        if (enemyBoss == null) enemyBoss = GetComponent<EnemyBoss>();

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

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);

        // shootPoint가 설정되지 않은 경우 자신의 위치 사용
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
        co = null;
    }
    void InitState2Update()
    {
        // 레이저가 활성화 상태일 때
        if (isLaserActive && currentLaser != null)
        {
            // 회전 시작 시간 체크
            if (!isRotating && Time.time >= laserStartTime + rotationDelay)
            {
                isRotating = true;
                hasPassedTarget = false;
                
                // 이 시점의 플레이어 위치를 기준으로 회전 방향 결정
                if (player != null)
                {
                    Vector3 targetDirection = (player.position - currentLaser.transform.position).normalized;
                    targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                    
                    // 더 짧은 경로로 회전하도록 방향 결정
                    float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
                    rotationDirection = angleDifference >= 0 ? 1 : -1;
                }
            }

            // 회전 중일 때
            if (isRotating)
            {
                // 타겟을 지나쳤는지 확인
                if (!hasPassedTarget)
                {
                    float angleToTarget = Mathf.DeltaAngle(currentAngle, targetAngle);
                    
                    // 회전 방향에 따라 타겟을 지나쳤는지 판단
                    bool passedTarget = (rotationDirection > 0 && angleToTarget <= 0) || 
                                       (rotationDirection < 0 && angleToTarget >= 0);
                    
                    if (passedTarget)
                    {
                        hasPassedTarget = true;
                        passedTargetTime = Time.time;
                    }
                }
                
                // 타겟을 지나친 후 재타겟팅 시간이 지났는지 확인
                if (hasPassedTarget && Time.time >= passedTargetTime + retargetDelay)
                {
                    // 새로운 플레이어 위치로 재타겟팅
                    if (player != null)
                    {
                        Vector3 newTargetDirection = (player.position - currentLaser.transform.position).normalized;
                        float newTargetAngle = Mathf.Atan2(newTargetDirection.y, newTargetDirection.x) * Mathf.Rad2Deg;
                        
                        // 새로운 회전 방향 결정
                        float newAngleDifference = Mathf.DeltaAngle(currentAngle, newTargetAngle);
                        rotationDirection = newAngleDifference >= 0 ? 1 : -1;
                        targetAngle = newTargetAngle;
                        
                        // 재타겟팅 완료, 다시 추적 시작
                        hasPassedTarget = false;
                    }
                }
                
                // 회전 실행
                currentAngle += rotationSpeed * rotationDirection * Time.deltaTime;
                currentLaser.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
            }
        }

        // 레이저가 비활성화 상태에서만 쿨타임 진행
        if (!isLaserActive)
        {
            if (Time.time >= nextFireTime && player != null)
            {
                co = StartCoroutine(FireLaserCo());
                nextFireTime = Time.time + fireRate;
            }
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }
    void InitState2Exit()
    {
        rotationSpeed += 10f; // 매번 10도씩 증가. 점점 빨라지도록

        enemyBoss.SetMovable(true);
    }

    #region 기타 함수들
    IEnumerator FireLaserCo()
    {
        if (isLaserActive) yield break;

        // 플레이어 방향으로 초기 각도 설정
        Vector3 direction = (player.position - shootPoint.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 너무 즉시 플레이어에게 레이져가 나가지 않도록
        yield return new WaitForSeconds(.4f);

        // 레이저 생성
        currentLaser = Instantiate(laserProjectile, shootPoint.position, Quaternion.AngleAxis(currentAngle, Vector3.forward));

        // 레이저 설정
        EnemyLaserProjectile laserScript = currentLaser.GetComponent<EnemyLaserProjectile>();
        if (laserScript == null)
        {
            laserScript = currentLaser.AddComponent<EnemyLaserProjectile>();
        }

        laserScript.Initialize(damage, destructables, walls, laserDuration, laserWidth);

        // 레이저 상태 초기화
        isLaserActive = true;
        laserStartTime = Time.time;
        isRotating = false;
        hasPassedTarget = false;

        // 레이저 지속 시간 후 비활성화
        yield return new WaitForSeconds(laserDuration);

        if (currentLaser != null)
        {
            currentLaser.SetActive(false);
            Destroy(currentLaser);
        }

        // 상태 초기화
        isLaserActive = false;
        isRotating = false;
        hasPassedTarget = false;
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }
    #endregion
}