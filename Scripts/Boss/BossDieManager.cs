using System.Collections;
using UnityEngine;

public class BossDieManager : MonoBehaviour
{
    public static BossDieManager instance;
    public bool IsBossDead { get; private set; }
    GameObject deadBody;
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
    }

    // IsBossDead 없이 deadBody만 생성
    public void InitDeadBodyInfinite(GameObject deadBody, Transform boss)
    {
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
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
        Debug.Log("보스 다이 매니져에서 호출");

        MusicManager.instance.Stop();
        SoundManager.instance.StopAllSounds();

        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SetCurrentStageCleared();
        playerData.SaveResourcesBeforeQuitting();

        Time.timeScale = desiredTimeScale;

        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame();
        if (anim != null) anim.SetTrigger("Die");

        RemoveAllEnemies();
        RemoveAllWalls();

        yield return new WaitForSeconds(3f);
        if (deadBody != null) deadBody.GetComponent<BossDeadBody>().TeleportOutEffect();

        yield return new WaitForSeconds(4f);
        if (playerData.GetGameMode() == GameMode.Regular) //일반 모드일 때만 스테이지 클리어 관련 연산
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
        yield return new WaitForSeconds(3f);
        if (deadBody != null) deadBody.GetComponent<BossDeadBody>().TeleportOutEffect();
    }

    public void BossCameraOff()
    {
    }

    // ⭐ 수정: 원래 timeScale 저장 후 복구
    public void SlowMo(float _desiredTimeScale, float _duration)
    {
        StartCoroutine(SlowMoCo(_desiredTimeScale, _duration));
    }

    IEnumerator SlowMoCo(float _desiredTimeScale, float _duration)
    {
        slowMoActiveCount++;

        PauseManager pauseManager = FindObjectOfType<PauseManager>();

        // ⭐ 추가: 패널이 소유한 정지 상태라면 슬로모 값을 세팅하지 않음
        //    (watchdog이 어차피 0으로 유지시키고 있으므로 건드릴 필요 없음)
        if (pauseManager == null || !pauseManager.IsPausedByPanel)
        {
            Time.timeScale = _desiredTimeScale;
        }

        yield return new WaitForSecondsRealtime(_duration);

        slowMoActiveCount--;

        if (slowMoActiveCount <= 0)
        {
            slowMoActiveCount = 0;

            // ⭐ 추가: 패널이 열려 있는 동안이라면 UnPauseGame()을 호출하지 않음
            //    패널이 스스로 닫힐 때(VanishPanel, Revive 등) 정상적으로 복구함
            if (pauseManager != null && !pauseManager.IsPausedByPanel)
            {
                pauseManager.UnPauseGame();
                Logger.Log("[BossDieManager] 모든 SlowMo 종료");
            }
            else
            {
                Logger.Log("[BossDieManager] 패널이 열려있어 SlowMo 복구를 건너뜀 (패널이 처리함)");
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