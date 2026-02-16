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
        if(playerData.GetGameMode() == GameMode.Regular) //일반 모드일 때만 스테이지 클리어 관련 연산
        {
            FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
        }
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
        // ⭐ 현재 timeScale 저장
        float originalTimeScale = Time.timeScale;
        
        Time.timeScale = _desiredTimeScale;
        yield return new WaitForSecondsRealtime(_duration);
        
        // ⭐ 저장된 값으로 복구
        Time.timeScale = originalTimeScale;
        
        Logger.Log($"[BossDieManager] SlowMo done, restored timeScale to {originalTimeScale}");
    }

    void RemoveAllEnemies()
    {
        LayerMask enemyLayer = LayerMask.NameToLayer("Enmey");

        Collider2D[] enemies = 
            Physics2D.OverlapCircleAll(Player.instance.transform.position, 1000f, enemyLayer);

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyBase enemyBase = enemies[i].GetComponent<EnemyBase>();
            if(enemyBase != null)
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
            if(bouncer != null)
            {
                bouncer.DeactivateWall();
            }
        }
    }
}