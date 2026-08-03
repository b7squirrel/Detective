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
        MusicManager.instance.Stop();
        SoundManager.instance.StopAllSounds();

        // 1) 슬로우모션으로 처치 순간 강조
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);

        // 2) 정상 속도로 복귀
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        Time.timeScale = pauseManager.NormalTimeScale;

        // 3) 사망 애니메이션 + 필드 정리
        if (anim != null) anim.SetTrigger("Die");

        RemoveAllEnemies();
        RemoveAllWalls();

        yield return new WaitForSeconds(3f);
        if (deadBody != null) deadBody.GetComponent<BossDeadBody>().TeleportOutEffect();

        yield return new WaitForSeconds(4f);

        // 4) ⭐ 몇 초 후 저장 + 클리어 패널 표시 (내부에서 PauseGame() 호출됨)
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SetCurrentStageCleared();
        playerData.SaveResourcesBeforeQuitting();

        if (playerData.GetGameMode() == GameMode.Regular) // 일반 모드일 때만 스테이지 클리어 관련 연산
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