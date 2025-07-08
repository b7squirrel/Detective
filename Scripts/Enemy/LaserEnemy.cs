using System.Collections;
using UnityEngine;

public class LaserEnemy : MonoBehaviour
{
    [Header("레이저 설정")]
    public GameObject laserProjectile;  // 레이저 프리팹
    public Transform shootPoint;        // 레이저 발사 위치
    public float laserDuration = 2f;    // 레이저 지속 시간
    public float fireRate = 3f;         // 발사 간격 (초)
    public float rotationSpeed = 60f;   // 회전 속도 (도/초)
    public float damage = 10f;          // 데미지
    public LayerMask destructables;     // 파괴 가능한 레이어
    public LayerMask walls;             // 벽 레이어
    
    [Header("타겟 설정")]
    public string playerTag = "Player"; // 플레이어 태그
    
    private Transform player;
    private float nextFireTime = 0f;
    private GameObject currentLaser;
    private bool isLaserActive = false;
    private float currentAngle = 0f;    // 현재 레이저 발사 각도
    
    void Start()
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
    
    void Update()
    {
        // 레이저가 활성화 상태일 때 레이저 방향 회전
        if (isLaserActive && currentLaser != null)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            currentLaser.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        }
        
        // 레이저가 비활성화 상태에서만 쿨타임 진행
        if (!isLaserActive)
        {
            if (Time.time >= nextFireTime && player != null)
            {
                FireLaser();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    
    void FireLaser()
    {
        if (isLaserActive) return;
        
        // 플레이어 방향으로 초기 각도 설정
        Vector3 direction = (player.position - shootPoint.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 레이저 생성
        currentLaser = Instantiate(laserProjectile, shootPoint.position, Quaternion.AngleAxis(currentAngle, Vector3.forward));
        
        // 레이저 설정
        EnemyLaserProjectile laserScript = currentLaser.GetComponent<EnemyLaserProjectile>();
        if (laserScript == null)
        {
            laserScript = currentLaser.AddComponent<EnemyLaserProjectile>();
        }
        
        laserScript.Initialize(damage, destructables, walls, laserDuration);
        isLaserActive = true;
        
        // 레이저 지속 시간 후 비활성화
        StartCoroutine(DisableLaserAfterDuration());
    }
    
    IEnumerator DisableLaserAfterDuration()
    {
        yield return new WaitForSeconds(laserDuration);
        
        if (currentLaser != null)
        {
            currentLaser.SetActive(false);
            Destroy(currentLaser);
        }
        
        isLaserActive = false;
    }
}