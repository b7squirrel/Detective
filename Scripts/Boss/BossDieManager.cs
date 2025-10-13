using System.Collections;
using UnityEngine;

public class BossDieManager : MonoBehaviour
{
    public static BossDieManager instance;
    public bool IsBossDead { get; private set; } // 죽은 후부터는 player의 takeDamage가 작동 안하도록
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
        IsBossDead = true;
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
        //this.amountOfCoins = amountOfCoins;
    }
    public void DieEvent(float desiredTimeScale, float waitingTime)
    {
        StartCoroutine(DieEventCo(desiredTimeScale, waitingTime));
    }
    IEnumerator DieEventCo(float desiredTimeScale, float waitingTime)
    {
        Debug.Log("보스 다이 매니져에서 호출");

        MusicManager.instance.Stop();

        // 스테이지와 동전 저장
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();

        // 스테이지가 클리어 된 것을 기록
        playerData.SetCurrentStageCleared(); 
        playerData.SaveResourcesBeforeQuitting();

        Time.timeScale = desiredTimeScale;

        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame();
        if (anim != null) anim.SetTrigger("Die");

        //모든 적과 벽 제거
        RemoveAllEnemies();
        RemoveAllWalls();

        //dropCoins.Init(amountOfCoins, deadBody.transform.position);
        yield return new WaitForSeconds(3f);
        if (deadBody != null) deadBody.GetComponent<BossDeadBody>().TeleportOutEffect();

        yield return new WaitForSeconds(4f);
        FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
    }
    
    public void BossCameraOff()
    {
        //StartCoroutine(PlayerCamera());
    }
    //IEnumerator PlayerCamera()
    //{
    //    yield return new WaitForSeconds(2f);
    //    Player.instance.ShouldBeStill = false;
    //}

    //IEnumerator WinMessage()
    //{
    //    yield return new WaitForSeconds(7f);
    //    GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    //}
    
    public void SlowMo(float _desiredTimeScale, float _duration)
    {
        StartCoroutine(SlowMoCo(_desiredTimeScale, _duration));
    }
    IEnumerator SlowMoCo(float _desiredTimeScale, float _duration)
    {
        Time.timeScale = _desiredTimeScale;
        yield return new WaitForSecondsRealtime(_duration);
        Time.timeScale = 1f;
    }

    void RemoveAllEnemies()
    {
        LayerMask enemyLayer = LayerMask.NameToLayer("Enmey"); // 이상하게 GetMask가 안됨

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
        LayerMask wallLayer = LayerMask.GetMask("Wall"); // 이상하게 NameToLayer가 안됨

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
