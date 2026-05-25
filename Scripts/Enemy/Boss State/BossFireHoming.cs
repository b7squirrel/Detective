using System.Collections;
using UnityEngine;

/// <summary>
/// State3 공격: 호밍 투사체를 원형으로 발사
/// ※ homingBallPrefab 프리팹에 PoolingKey 컴포넌트가 있어야 합니다.
/// </summary>
public class BossFireHoming : MonoBehaviour
{
    [SerializeField] float timeToDropSlime;
    EnemyBoss enemyBoss;

    [Header("호밍 설정")]
    public GameObject homingBallPrefab;   // 유도 공 프리팹 (PoolingKey 필수)
    public Transform shootPoint;
    [SerializeField] int maxProjectileNum;
    public float ballSpeed = 12f;
    [SerializeField] float homingDelay;
    [SerializeField] float randomAngleRange = 20f;
    [SerializeField] float fireInterval = .2f;
    public float ballLifetime = 7f;
    public float fireRate = 4f;
    public int damage = 20;
    public LayerMask playerLayer;
    public LayerMask wallLayer;

    [Header("타겟 설정")]
    public string playerTag = "Player";

    private Transform player;
    private float nextFireTime = 0f;
    private Coroutine co;

    [Header("사운드")]
    [SerializeField] AudioClip bombAnticReadySound;
    [SerializeField] AudioClip bombAnticSound;
    [SerializeField] AudioClip shootingBombStateSound;
    [SerializeField] AudioClip shootingBombSound;
    bool isPlayingShootingStateSound;

    #region 이벤트 등록/해제
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

        if (player == null)
            player = GameManager.instance.player.transform;

        if (shootPoint == null)
            shootPoint = enemyBoss.GetShootPoint();

        enemyBoss.SetMovable(false);
        PlayShootingFireHomingStateSound();
    }

    void InitHomingUpdate()
    {
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

        Vector3 playerDirection = (player.position - shootPoint.position).normalized;
        float playerAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;

        while (currentProjectileNums < count)
        {
            float angle = playerAngle + (360f * currentProjectileNums / count);
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // ── 풀링으로 투사체 가져오기 ──
            GameObject ball = GameManager.instance.poolManager.GetMisc(homingBallPrefab);
            if (ball == null)
            {
                // maxNum 제한에 걸렸을 때: 해당 슬롯은 건너뛰고 계속 진행
                Debug.LogWarning("[BossFireHoming] 풀 한도 초과, 투사체 생략");
                currentProjectileNums++;
                yield return new WaitForSeconds(fireInterval);
                continue;
            }

            ball.transform.position = shootPoint.position;

            EnemyHomingProjectile homingScript = ball.GetComponent<EnemyHomingProjectile>();
            if (homingScript != null)
            {
                homingScript.Initialize(damage, ballLifetime, homingDelay);
                homingScript.speed = ballSpeed;
            }
            else
            {
                Debug.LogError("[BossFireHoming] EnemyHomingProjectile 컴포넌트를 찾을 수 없습니다!");
                ball.SetActive(false);
            }

            currentProjectileNums++;
            Debug.Log($"호밍 투사체 발사 - 각도: {angle:F1}도");
            yield return new WaitForSeconds(fireInterval);
        }

        enemyBoss.GetComponent<Animator>().SetTrigger("Settle");
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
        if (shootingBombStateSound == null || isPlayingShootingStateSound) return;
        SoundManager.instance.PlayLoop(shootingBombStateSound);
        isPlayingShootingStateSound = true;
    }
    void StopShootingFireHomingStateSound()
    {
        if (shootingBombStateSound == null || !isPlayingShootingStateSound) return;
        SoundManager.instance.StopLoop(shootingBombStateSound);
        isPlayingShootingStateSound = false;
    }
    void PlayShootFireHomingSound()
    {
        if (shootingBombSound == null) return;
        SoundManager.instance.Play(shootingBombSound);
    }
    #endregion
}