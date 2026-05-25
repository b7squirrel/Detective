using System.Collections;
using UnityEngine;

/// <summary>
/// State2 공격: 부채꼴 형태의 불을 좌우로 천천히 회전하며 발사
/// BossFireRing을 대체합니다. Boss 오브젝트에서 BossFireRing 컴포넌트를 이것으로 교체하세요.
/// ※ fireProjectile 프리팹에 PoolingKey 컴포넌트가 있어야 합니다.
/// </summary>
public class BossFireFan : MonoBehaviour
{
    EnemyBoss enemyBoss;

    [SerializeField] float timeToDropSlime;

    [Header("투사체 설정")]
    [SerializeField] GameObject fireProjectile;   // EnemyFireProjectile 프리팹 (PoolingKey 필수)
    [SerializeField] float ballSpeed = 10f;
    [SerializeField] int damage = 15;

    [Header("부채꼴 형태 설정")]
    [SerializeField] int projectilesPerFan = 7;   // 한 번에 발사하는 투사체 수 (홀수 권장)
    [SerializeField] float fanAngle = 70f;         // 부채꼴 전체 각도 (degree)

    [Header("회전(스윕) 설정")]
    [SerializeField] float rotateSpeed = 50f;      // 초당 회전 속도 (degree/s)
    [SerializeField] float maxRotateAngle = 75f;   // 중심으로부터 최대 회전 각도 (degree)

    [Header("발사 타이밍")]
    [SerializeField] float burstInterval = 0.35f;   // 부채꼴 한 번 발사 주기 (초)
    [SerializeField] float totalAttackDuration = 6f; // State2 전체 지속 시간 (초)

    [Header("사운드")]
    [SerializeField] AudioClip stateLoopSound;
    [SerializeField] AudioClip burstFireSound;
    bool isPlayingStateSound;

    Transform shootPoint;
    Transform player;

    float currentAngleOffset = 0f;
    float rotateDir = 1f;

    bool isFiring = false;
    Coroutine fireCoroutine;

    #region 이벤트 등록/해제
    void OnEnable()
    {
        EnemyBoss.OnState2Enter += InitFanEnter;
        EnemyBoss.OnState2Update += InitFanUpdate;
        EnemyBoss.OnState2Exit += InitFanExit;
    }
    void OnDisable()
    {
        EnemyBoss.OnState2Enter -= InitFanEnter;
        EnemyBoss.OnState2Update -= InitFanUpdate;
        EnemyBoss.OnState2Exit -= InitFanExit;
    }
    #endregion

    void InitFanEnter()
    {
        enemyBoss = GetComponent<EnemyBoss>();
        player = GameManager.instance.player.transform;

        if (shootPoint == null)
            shootPoint = enemyBoss.GetShootPoint();

        currentAngleOffset = 0f;
        rotateDir = 1f;
        isFiring = false;

        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }

        enemyBoss.SetMovable(false);
        PlayStateLoopSound();
        fireCoroutine = StartCoroutine(FireFanCo());
    }

    void InitFanUpdate()
    {
        enemyBoss.SlimeDropTimer(timeToDropSlime);
    }

    void InitFanExit()
    {
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
        isFiring = false;
        StopStateLoopSound();
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireFanCo()
    {
        isFiring = true;

        // 진입 시점의 플레이어 방향을 기준 각도로 고정
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)shootPoint.position).normalized;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        float elapsed = 0f;
        float burstTimer = 0f;

        while (elapsed < totalAttackDuration)
        {
            float dt = Time.deltaTime;

            // 부채꼴 중심 각도를 진자처럼 좌우 회전
            currentAngleOffset += rotateDir * rotateSpeed * dt;
            if (Mathf.Abs(currentAngleOffset) >= maxRotateAngle)
            {
                rotateDir *= -1f;
                currentAngleOffset = Mathf.Clamp(currentAngleOffset, -maxRotateAngle, maxRotateAngle);
            }

            // burstInterval 주기로 부채꼴 발사
            burstTimer -= dt;
            if (burstTimer <= 0f)
            {
                FireFanBurst(baseAngle + currentAngleOffset);
                burstTimer = burstInterval;
            }

            elapsed += dt;
            yield return null;
        }

        isFiring = false;
        fireCoroutine = null;
        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
    }

    void FireFanBurst(float centerAngle)
    {
        if (fireProjectile == null)
        {
            Debug.LogWarning("[BossFireFan] fireProjectile 프리팹이 Inspector에 할당되지 않았습니다.");
            return;
        }

        PlayBurstFireSound();

        float halfFan = fanAngle / 2f;

        for (int i = 0; i < projectilesPerFan; i++)
        {
            float t = projectilesPerFan > 1
                ? (float)i / (projectilesPerFan - 1)
                : 0.5f;

            float angle = centerAngle - halfFan + fanAngle * t;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            // ── 풀링으로 투사체 가져오기 ──
            GameObject proj = GameManager.instance.poolManager.GetMisc(fireProjectile);
            if (proj == null) continue; // maxNum 제한에 걸렸을 때는 건너뜀

            proj.transform.position = shootPoint.position;

            EnemyFireProjectile fireProj = proj.GetComponent<EnemyFireProjectile>();
            if (fireProj != null)
            {
                fireProj.Initialize(damage, ballSpeed, dir);
            }
            else
            {
                Debug.LogWarning("[BossFireFan] EnemyFireProjectile 컴포넌트를 찾을 수 없습니다.");
                proj.SetActive(false);
            }
        }
    }

    #region 사운드
    void PlayStateLoopSound()
    {
        if (stateLoopSound == null || isPlayingStateSound) return;
        SoundManager.instance.PlayLoop(stateLoopSound);
        isPlayingStateSound = true;
    }
    void StopStateLoopSound()
    {
        if (stateLoopSound == null || !isPlayingStateSound) return;
        SoundManager.instance.StopLoop(stateLoopSound);
        isPlayingStateSound = false;
    }
    void PlayBurstFireSound()
    {
        if (burstFireSound == null) return;
        SoundManager.instance.Play(burstFireSound);
    }
    #endregion
}