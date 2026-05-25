using System.Collections;
using UnityEngine;

/// <summary>
/// State2 공격: 부채꼴 형태의 불을 좌우로 회전하며 발사.
/// 부채꼴 중심이 플레이어 방향을 실시간으로 추적하고,
/// 그 위에 진자 스윕 오프셋이 더해집니다.
/// </summary>
public class BossFireFan : MonoBehaviour
{
    EnemyBoss enemyBoss;

    [SerializeField] float timeToDropSlime;

    [Header("투사체 설정")]
    [SerializeField] GameObject fireProjectile;
    [SerializeField] float ballSpeed = 10f;
    [SerializeField] int damage = 15;

    [Header("부채꼴 형태 설정")]
    [SerializeField] int projectilesPerFan = 7;
    [SerializeField] float fanAngle = 70f;

    [Header("플레이어 추적 설정")]
    [SerializeField] float trackingSpeed = 90f;  // 초당 최대 추적 각도 (degree/s)
                                                  // 낮을수록 느리게 따라감 → 피할 여지 생김
                                                  // 권장: 60~120

    [Header("진자 스윕 설정")]
    [SerializeField] float rotateSpeed = 50f;     // 진자 속도 (degree/s)
    [SerializeField] float maxRotateAngle = 75f;  // 진자 최대 각도

    [Header("발사 타이밍")]
    [SerializeField] float burstInterval = 0.35f;
    [SerializeField] float totalAttackDuration = 6f;

    [Header("파티클 이펙트")]
    [SerializeField] GreenFireParticle muzzleFlameEffect;

    [Header("사운드")]
    [SerializeField] AudioClip stateLoopSound;
    [SerializeField] AudioClip burstFireSound;
    bool isPlayingStateSound;

    Transform shootPoint;
    Transform player;

    float currentAimAngle = 0f;   // 실시간으로 플레이어를 향하는 기준 각도
    float currentAngleOffset = 0f; // 진자 오프셋 (기준 각도에 더해짐)
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

        // 진입 시 플레이어 방향으로 즉시 조준
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)shootPoint.position).normalized;
        currentAimAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

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
        muzzleFlameEffect?.Play();

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
        muzzleFlameEffect?.Stop();
        StopStateLoopSound();
        enemyBoss.SetMovable(true);
    }

    IEnumerator FireFanCo()
    {
        isFiring = true;

        float elapsed = 0f;
        float burstTimer = 0f;

        while (elapsed < totalAttackDuration)
        {
            float dt = Time.deltaTime;

            // ── 1. 플레이어 방향으로 기준 각도를 부드럽게 회전 ──
            Vector2 toPlayer = ((Vector2)player.position - (Vector2)shootPoint.position).normalized;
            float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            // MoveTowardsAngle: 360° 경계를 자연스럽게 처리해 줌
            currentAimAngle = Mathf.MoveTowardsAngle(currentAimAngle, targetAngle, trackingSpeed * dt);

            // ── 2. 진자 오프셋 계산 ──
            currentAngleOffset += rotateDir * rotateSpeed * dt;
            if (Mathf.Abs(currentAngleOffset) >= maxRotateAngle)
            {
                rotateDir *= -1f;
                currentAngleOffset = Mathf.Clamp(currentAngleOffset, -maxRotateAngle, maxRotateAngle);
            }

            // ── 3. burstInterval 주기로 발사 ──
            burstTimer -= dt;
            if (burstTimer <= 0f)
            {
                // 기준 각도(플레이어 방향) + 진자 오프셋
                FireFanBurst(currentAimAngle + currentAngleOffset);
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

            GameObject proj = GameManager.instance.poolManager.GetMisc(fireProjectile);
            if (proj == null) continue;

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