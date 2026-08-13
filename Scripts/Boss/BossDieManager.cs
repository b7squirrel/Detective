using System.Collections;
using UnityEngine;

public class BossDieManager : MonoBehaviour
{
    public static BossDieManager instance;
    public bool IsBossDead { get; private set; }
    GameObject deadBody;
    BossDeadBody deadBodyScript; // ⭐ 추가: 이벤트 구독/해제를 위해 캐싱
    int amountOfCoins;
    Animator anim;
    DropCoins dropCoins;
    [SerializeField] LayerMask testLayer;
    int slowMoActiveCount = 0; // 현재 활성 SlowMo 수

    void Awake()
    {
        instance = this;
        IsBossDead = false;
    }

    public void InitDeadBody(GameObject deadBody, Transform boss, int amountOfCoins)
    {
        SetIsBossDead(true);
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
        deadBodyScript = this.deadBody.GetComponent<BossDeadBody>(); // ⭐ 추가
    }

    // IsBossDead 없이 deadBody만 생성
    public void InitDeadBodyInfinite(GameObject deadBody, Transform boss)
    {
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
        deadBodyScript = this.deadBody.GetComponent<BossDeadBody>(); // ⭐ 추가
    }

    public void SetIsBossDead(bool isDead)
    {
        IsBossDead = isDead;
    }

    public void DieEvent(float desiredTimeScale, float waitingTime)
    {
        StartCoroutine(DieEventCo(desiredTimeScale, waitingTime));
    }

    IEnumerator DieEventCo(float desiredTimeScale, float waitingTime)
    {
        MusicManager.instance.Stop();
        SoundManager.instance.StopAllSounds();

        // 1) 슬로우모션으로 처치 순간 강조
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);

        // 2) 정상 속도로 복귀
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        Time.timeScale = pauseManager.NormalTimeScale;

        // 3) 사망 애니메이션 재생
        //    텔레포트 연출(OnDieAnimationFinished → TeleportOutEffect)은
        //    Die 애니메이션 클립의 Animation Event가 자동으로 트리거함.
        //    텔레포트 연출이 "완전히" 끝나면 OnTeleportOutFinished 이벤트가 발생하고,
        //    HandleTeleportOutFinished에서 이어서 처리함.
        if (deadBodyScript != null)
        {
            deadBodyScript.OnTeleportOutFinished += HandleTeleportOutFinished;
        }
        else
        {
            // ⭐ 추가: 서브보스가 스테이지 보스로 등장한 경우 deadBody가 생성되지 않으므로
            //         deadBodyScript가 null. 텔레포트 대기 없이 바로 스테이지 클리어 처리로 진행.
            Logger.Log("[BossDieManager] deadBodyScript가 없음 (서브보스 케이스) - 텔레포트 대기 없이 바로 스테이지 클리어 처리");
            StartCoroutine(WaitCoinsThenFinalizeCo()); // ⭐ 변경: HandleTeleportOutFinished() 직접 호출 대신
        }
        if (anim != null) anim.SetTrigger("Die");

        RemoveAllEnemies();
        RemoveAllWalls();

        FieldItemEffect.instance.RemoveAllGems();
        FieldItemEffect.instance.RemoveAllChests();

        // ⭐ 매직넘버 대기(3f, 4f) 완전히 제거됨
    }

    // ⭐ 추가: 텔레포트 연출이 실제로 끝났을 때만 호출됨
    // 텔레포트 연출이 실제로 끝났을 때만 호출됨
    void HandleTeleportOutFinished()
    {
        if (deadBodyScript != null)
        {
            deadBodyScript.OnTeleportOutFinished -= HandleTeleportOutFinished; // 중복 구독 방지
        }

        StartCoroutine(WaitCoinsThenFinalizeCo()); // ⭐ 변경: 즉시 처리 대신 코인 대기 후 처리
    }

    // 화면에 날아가는 중인 코인/크리스탈이 모두 도착할 때까지 대기한 후 스테이지 클리어 처리
    IEnumerator WaitCoinsThenFinalizeCo()
    {
        float maxWaitTime = 5f; // ⭐ 추가: 코인 연출이 비정상적으로 안 끝날 경우를 대비한 최대 대기 시간
        float elapsed = 0f;

        while (MoveToUI.ActiveFlyingCount > 0)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= maxWaitTime)
            {
                Logger.LogWarning($"[BossDieManager] 코인 UI 이동 대기 타임아웃({maxWaitTime}초) - ActiveFlyingCount={MoveToUI.ActiveFlyingCount}로 강제 진행");
                break;
            }
            yield return null;
        }

        FinalizeStageClear();
    }

    // ⭐ 추가: 기존 HandleTeleportOutFinished 안에 있던 로직을 분리
    void FinalizeStageClear()
    {
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SetCurrentStageCleared();
        playerData.SaveResourcesBeforeQuitting();

        if (playerData.GetGameMode() == GameMode.Regular)
        {
            FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
        }
    }


    public void DieEventInfinite(float desiredTimeScale, float waitingTime)
    {
        StartCoroutine(DieEventInfiniteCo(desiredTimeScale, waitingTime));
    }
    IEnumerator DieEventInfiniteCo(float desiredTimeScale, float waitingTime)
    {
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame(); // timeScale 복구

        if (anim != null) anim.SetTrigger("Die");
        // ⭐ 텔레포트 연출은 Die 애니메이션 이벤트가 자동으로 처리하므로
        //    무한 모드에서는 별도로 기다리거나 스테이지 클리어 처리를 할 필요가 없음
    }

    public void BossCameraOff()
    {
    }

    public void SlowMo(float _desiredTimeScale, float _duration)
    {
        StartCoroutine(SlowMoCo(_desiredTimeScale, _duration));
    }

    IEnumerator SlowMoCo(float _desiredTimeScale, float _duration)
    {
        slowMoActiveCount++;

        PauseManager pauseManager = FindObjectOfType<PauseManager>();

        if (pauseManager == null || !pauseManager.IsPausedByPanel)
        {
            Time.timeScale = _desiredTimeScale;
        }

        yield return new WaitForSecondsRealtime(_duration);

        slowMoActiveCount--;

        if (slowMoActiveCount <= 0)
        {
            slowMoActiveCount = 0;
            if (pauseManager != null && !pauseManager.IsPausedByPanel)
            {
                pauseManager.UnPauseGame();
            }
        }
    }

    void RemoveAllEnemies()
    {
        LayerMask enemyLayer = LayerMask.NameToLayer("Enmey");

        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(Player.instance.transform.position, 1000f, enemyLayer);

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyBase enemyBase = enemies[i].GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.DieWithoutDrop();
            }
        }
    }

    void RemoveAllWalls()
    {
        LayerMask wallLayer = LayerMask.GetMask("Wall");

        Collider2D[] walls =
            Physics2D.OverlapCircleAll(Player.instance.transform.position, 1000f, wallLayer);

        for (int i = 0; i < walls.Length; i++)
        {
            Bouncer bouncer = walls[i].GetComponent<Bouncer>();
            if (bouncer != null)
            {
                bouncer.DeactivateWall();
            }
        }
    }
}