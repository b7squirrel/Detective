using System.Collections;
using UnityEngine;
using DG;

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
    public void Init(GameObject deadBody, Transform boss, int amountOfCoins)
    {
        IsBossDead = true;
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
        this.amountOfCoins = amountOfCoins;

        dropCoins = GetComponent<DropCoins>();

        MusicManager.instance.Stop();
        GameManager.instance.GetComponent<BossHealthBarManager>().DeActivateBossHealthBar();

        BossCameraOn();

        StartCoroutine(DieEvent(.1f, 2f));
    }

    IEnumerator DieEvent(float desiredTimeScale, float waitingTime)
    {
        Time.timeScale = desiredTimeScale;

        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame();
        anim.SetTrigger("Die");

        RemoveAllEnemies();
        RemoveAllWalls();
        //dropCoins.Init(amountOfCoins, deadBody.transform.position);

        FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
    }

    public void BossCameraOn()
    {
        Player.instance.ShouldBeStill = true;
        // Camera.main.transform.GetComponent<CameraController>().CameraToTarget(deadBody.transform.position, true);
    }
    public void BossCameraOff()
    {
        StartCoroutine(PlayerCamera());
    }
    IEnumerator PlayerCamera()
    {
        yield return new WaitForSeconds(2f);
        Player.instance.ShouldBeStill = false;
        // Camera.main.transform.GetComponent<CameraController>().CameraToTarget(Player.instance.transform.position, false);
    }

    //IEnumerator WinMessage()
    //{
    //    yield return new WaitForSeconds(7f);
    //    GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    //}
    
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
