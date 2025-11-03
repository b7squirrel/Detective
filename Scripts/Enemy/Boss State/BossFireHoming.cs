using System.Collections;
using UnityEngine;

public class BossFireHoming : MonoBehaviour
{
    [SerializeField] float timeToDropSlime; // 슬라임을 떨어트릴 주기
    EnemyBoss enemyBoss;

    [Header("호밍 설정")]
    public GameObject homingBallPrefab;   // 유도 공 프리팹
    public Transform shootPoint;          // 공 발사 위치
    [SerializeField] int maxProjectileNum; // 공 개수 
    public float ballSpeed = 12f;         // 공 속도
    [SerializeField] float homingDelay;   // 발사된 후 바로 따라가지 않고 일정 시간동안 초기 방향으로 날아가기
    [SerializeField] float randomAngleRange = 20f; // 무작위 각도 범위 (±도 단위)
    [SerializeField] float fireInterval = .2f;     // 발사 시간 간격
    public float ballLifetime = 7f;      // 공 생존 시간
    public float fireRate = 4f;          // 발사 간격 (초)
    public int damage = 20;              // 데미지
    public LayerMask playerLayer;        // 플레이어 레이어
    public LayerMask wallLayer;          // 벽 레이어

    [Header("타겟 설정")]
    public string playerTag = "Player";  // 플레이어 태그

    private Transform player;
    private float nextFireTime = 0f;
    private Coroutine co;                // 현재 실행 중인 코루틴

    [Header("사운드")]
    [SerializeField] AudioClip bombAnticReadySound;
    [SerializeField] AudioClip bombAnticSound;
    [SerializeField] AudioClip shootingBombStateSound;
    [SerializeField] AudioClip shootingBombSound;
    bool isPlayingShootingStateSound; // shooting state sound는 루프로 돌릴 예정

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
        if (player == null)
            player = GameManager.instance.player.transform;

        // shootPoint가 설정되지 않은 경우 보스에서 가져오기
        if (shootPoint == null)
            shootPoint = enemyBoss.GetShootPoint();

        // 플레이어에게 밀리지 않도록
        enemyBoss.SetMovable(false);

        PlayShootingFireHomingStateSound();
    }

    void InitHomingUpdate()
    {
        // 다음 발사 시점이 되었고, 플레이어가 존재하며, 현재 발사 중이 아닐 때만 실행
        if (Time.time >= nextFireTime && player != null && co == null)
        {
            co = StartCoroutine(FireCirclePatternCo(10));
            nextFireTime = Time.time + fireRate;
        }

        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }

    void InitHomingExit()
    {
        StopShootingFireHomingStateSound();
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireCirclePatternCo(int count)
    {
        int currentProjectileNums = 0;

        // 플레이어 방향 계산
        Vector3 playerDirection = (player.position - shootPoint.position).normalized;
        float playerAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

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
                homingScript.Initialize(damage, ballLifetime, homingDelay);
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

        // 모든 투사체 발사 후 다음 상태로 전환
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");

        // 코루틴 종료 시점에 null 처리 (중복 실행 방지용)
        co = null;
    }

    #region 사운드
    public void PlayFireHomingAnticReadySound()
    {
        if (bombAnticReadySound == null) return;
        SoundManager.instance.Play(bombAnticReadySound);
    }
    public void PlayFireHomingAnticSound()
    {
        if (bombAnticSound == null) return;
        SoundManager.instance.Play(bombAnticSound);
    }
    void PlayShootingFireHomingStateSound()
    {
        if (shootingBombStateSound == null) return;
        if (isPlayingShootingStateSound) return;
        SoundManager.instance.PlayLoop(shootingBombStateSound);
        isPlayingShootingStateSound = true;
    }
    void StopShootingFireHomingStateSound()
    {
        if (shootingBombStateSound == null) return;
        if (isPlayingShootingStateSound)
        {
            SoundManager.instance.StopLoop(shootingBombStateSound);
            isPlayingShootingStateSound = false;
        }
    }
    void PlayShootFireHomingSound()
    {
        if (shootingBombSound == null) return;
        SoundManager.instance.Play(shootingBombSound);
    }
    #endregion
}