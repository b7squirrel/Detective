using System.Collections;
using UnityEngine;

/// <summary>
/// 레이져의 지속 시간에 따라 상태의 지속시간이 결정됨
/// 양방향으로 동시에 레이저 발사
/// </summary>
public class BossLaserThickLaser : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;
    EnemyBase enemyBase;

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

    public float additionalAngle = 5f; // 해당 값만큼 매 회 추가해서 레이져가 항상 다음 번에는 더 빨라지도록 
    public LayerMask destructables;     // 파괴 가능한 레이어
    public LayerMask walls;             // 벽 레이어
    Coroutine co;

    [Header("인디케이터 설정")]
    public float indicatorDuration = 1f;    // 인디케이터 표시 시간
    public Color indicatorColor = new Color(1f, 0f, 0f, 0.3f);  // 반투명 빨강
    public Color indicatorEndColor = new Color(1f, 0f, 0f, 0.3f);  // 반투명 빨강
    public float indicatorWidth = 2f;       // 인디케이터 두께
    public float indicatorEndWidth = 2f;       // 인디케이터 두께
    public float indicatorMaxDistance = 50f; // 인디케이터 최대 거리
    [SerializeField] AudioClip[] indicatorSounds; // 인디케이터 단발성 사운드
    [SerializeField] GameObject anticEffectaPrefab; // 인디케이터가 나올 때 지지징 하면서 레이져가 나올 것이라는 것을 알 수 있도록
    GameObject anticEffect; // 인디케이터가 나올 때 지지징 하면서 레이져가 나올 것이라는 것을 알 수 있도록


    [Header("타겟 설정")]
    public string playerTag = "Player"; // 플레이어 태그
    private Transform player;
    private float nextFireTime = 0f;

    // 양방향 레이저를 위한 변수들
    private GameObject currentLaser1;   // 첫 번째 레이저
    private GameObject currentLaser2;   // 두 번째 레이저 (반대 방향)

    // 인디케이터를 위한 변수들
    private LineRenderer indicator1;    // 첫 번째 인디케이터
    private LineRenderer indicator2;    // 두 번째 인디케이터

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

        enemyBase = GetComponent<EnemyBase>();
        enemyBase.OnDeath += DestroyAllLasers;
    }
    void OnDisable()
    {
        EnemyBoss.OnState2Enter -= InitState2Enter;
        EnemyBoss.OnState2Update -= InitState2Update;
        EnemyBoss.OnState2Exit -= InitState2Exit;

        enemyBase.OnDeath -= DestroyAllLasers;
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

        // 인디케이터 초기화
        CreateIndicators();

        co = null;
    }

    void InitState2Update()
    {
        // 레이저가 활성화 상태일 때
        if (isLaserActive && currentLaser1 != null && currentLaser2 != null)
        {
            // ★ 레이저 위치를 shootPoint에 동기화
            currentLaser1.transform.position = shootPoint.position;
            currentLaser2.transform.position = shootPoint.position;

            // 회전 시작 시간 체크
            if (!isRotating && Time.time >= laserStartTime + rotationDelay)
            {
                isRotating = true;
                hasPassedTarget = false;

                // 이 시점의 플레이어 위치를 기준으로 회전 방향 결정
                if (player != null)
                {
                    Vector3 targetDirection = (player.position - shootPoint.position).normalized;
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
                        Vector3 newTargetDirection = (player.position - shootPoint.position).normalized;
                        float newTargetAngle = Mathf.Atan2(newTargetDirection.y, newTargetDirection.x) * Mathf.Rad2Deg;

                        // 새로운 회전 방향 결정
                        float newAngleDifference = Mathf.DeltaAngle(currentAngle, newTargetAngle);
                        rotationDirection = newAngleDifference >= 0 ? 1 : -1;
                        targetAngle = newTargetAngle;

                        // 재타겟팅 완료, 다시 추적 시작
                        hasPassedTarget = false;
                    }
                }

                // 회전 실행 - 두 레이저 모두 회전
                currentAngle += rotationSpeed * rotationDirection * Time.deltaTime;
                currentLaser1.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
                // 두 번째 레이저는 180도 반대 방향
                currentLaser2.transform.rotation = Quaternion.AngleAxis(currentAngle + 180f, Vector3.forward);
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
        rotationSpeed += additionalAngle; // 매번 5도씩 증가. 점점 빨라지도록

        enemyBoss.SetMovable(true);
    }

    #region 기타 함수들
    void CreateIndicators()
    {
        // 인디케이터가 이미 존재하면 다시 만들지 않음
        if (indicator1 != null && indicator2 != null) return;

        // 첫 번째 인디케이터 생성
        GameObject indicatorObj1 = new GameObject("LaserIndicator1");
        indicatorObj1.transform.parent = shootPoint;
        indicatorObj1.transform.localPosition = Vector3.zero;
        indicator1 = indicatorObj1.AddComponent<LineRenderer>();

        // 두 번째 인디케이터 생성
        GameObject indicatorObj2 = new GameObject("LaserIndicator2");
        indicatorObj2.transform.parent = shootPoint;
        indicatorObj2.transform.localPosition = Vector3.zero;
        indicator2 = indicatorObj2.AddComponent<LineRenderer>();

        // 인디케이터 설정 (공통)
        SetupIndicator(indicator1);
        SetupIndicator(indicator2);

        // 초기에는 비활성화
        indicator1.enabled = false;
        indicator2.enabled = false;

        // 앤틱 이펙트 설정
        if (anticEffect == null)
        {
            anticEffect = Instantiate(anticEffectaPrefab, transform);
            anticEffect.transform.localScale = .7f * Vector2.one;
        }
        anticEffect.SetActive(false);
    }

    void SetupIndicator(LineRenderer lineRenderer)
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = indicatorColor;
        lineRenderer.endColor = indicatorEndColor;
        lineRenderer.startWidth = indicatorWidth;
        lineRenderer.endWidth = indicatorEndWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingLayerName = "Effect";
        lineRenderer.sortingOrder = 0;
    }

    IEnumerator ShowIndicatorsCo(float angle)
    {
        // 인디케이터 활성화
        indicator1.enabled = true;
        indicator2.enabled = true;

        // 인디케이터 위치 설정
        Vector3 startPos = shootPoint.position;

        // 첫 번째 방향 (플레이어 방향)
        Vector3 direction1 = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        Vector3 endPos1 = startPos + direction1 * indicatorMaxDistance;

        // 두 번째 방향 (반대 방향)
        Vector3 direction2 = -direction1;
        Vector3 endPos2 = startPos + direction2 * indicatorMaxDistance;

        indicator1.SetPosition(0, startPos);
        indicator1.SetPosition(1, endPos1);

        indicator2.SetPosition(0, startPos);
        indicator2.SetPosition(1, endPos2);

        // 인디케이터 사운드
        foreach (var item in indicatorSounds)
        {
            SoundManager.instance.Play(item);
        }

        //앤틱 이펙트
        anticEffect.transform.position = shootPoint.position;
        anticEffect.SetActive(true);

        // 인디케이터 표시 시간 동안 대기
        // 깜빡임 속도 설정 (빠르게 깜빡이게)
        float blinkInterval = 0.02f; // 0.1초마다 깜빡임
        float elapsedTime = 0f;

        while (elapsedTime < indicatorDuration)
        {
            // 깜빡이기
            indicator1.enabled = !indicator1.enabled;
            indicator2.enabled = !indicator2.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // 인디케이터 비활성화
        indicator1.enabled = false;
        indicator2.enabled = false;

        // 앤틱 이펙트 비활성화
        anticEffect.SetActive(false);
    }

    IEnumerator FireLaserCo()
    {
        if (isLaserActive) yield break;

        // 플레이어 방향으로 초기 각도 설정
        Vector3 direction = (player.position - shootPoint.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 인디케이터 표시
        yield return StartCoroutine(ShowIndicatorsCo(currentAngle));

        // 첫 번째 레이저 생성 (플레이어 방향)
        currentLaser1 = Instantiate(laserProjectile, shootPoint.position, Quaternion.AngleAxis(currentAngle, Vector3.forward));

        // 두 번째 레이저 생성 (반대 방향, 180도 회전)
        currentLaser2 = Instantiate(laserProjectile, shootPoint.position, Quaternion.AngleAxis(currentAngle + 180f, Vector3.forward));

        // 첫 번째 레이저 설정
        EnemyLaserProjectile laserScript1 = currentLaser1.GetComponent<EnemyLaserProjectile>();
        if (laserScript1 == null)
        {
            laserScript1 = currentLaser1.AddComponent<EnemyLaserProjectile>();
        }
        laserScript1.Initialize(damage, destructables, walls, laserWidth);

        // 두 번째 레이저 설정
        EnemyLaserProjectile laserScript2 = currentLaser2.GetComponent<EnemyLaserProjectile>();
        if (laserScript2 == null)
        {
            laserScript2 = currentLaser2.AddComponent<EnemyLaserProjectile>();
        }
        laserScript2.Initialize(damage, destructables, walls, laserWidth);

        // 레이저 상태 초기화
        isLaserActive = true;
        laserStartTime = Time.time;
        isRotating = false;
        hasPassedTarget = false;



        // 레이저 지속 시간 후 비활성화
        yield return new WaitForSeconds(laserDuration);

        // 두 레이저 모두 제거
        DestroyAllLasers();

        // 상태 초기화
        isLaserActive = false;
        isRotating = false;
        hasPassedTarget = false;
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }

    void DestroyAllLasers()
    {
        // 두 레이저 모두 제거
        if (currentLaser1 != null)
        {
            currentLaser1.SetActive(false);
            Destroy(currentLaser1);
        }

        if (currentLaser2 != null)
        {
            currentLaser2.SetActive(false);
            Destroy(currentLaser2);
        }
    }
    #endregion
}