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
    public void Init(GameObject deadBody, Transform boss, int amountOfCoins)
    {
        IsBossDead = true;
        this.deadBody = Instantiate(deadBody, boss.position, boss.rotation);
        anim = this.deadBody.GetComponent<Animator>();
        this.amountOfCoins = amountOfCoins;

        dropCoins = GetComponent<DropCoins>();

        MusicManager.instance.Stop();
        GameManager.instance.GetComponent<BossHealthBarManager>().DeActivateBossHealthBar();

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
        dropCoins.Init(amountOfCoins, deadBody.transform.position);

        StartCoroutine(WinMessage());
    }

    IEnumerator WinMessage()
    {
        yield return new WaitForSeconds(7f);
        GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    }
    
    void RemoveAllEnemies()
    {
        LayerMask enemyLayer = LayerMask.NameToLayer("Enmey"); // 이상하게 GetMask가 안됨

        Collider2D[] enemies = 
            Physics2D.OverlapCircleAll(Player.instance.transform.position, 1000f, enemyLayer);

        foreach (var item in enemies)
        {
            EnemyBase enemyBase = item.GetComponent<EnemyBase>();
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


        Debug.Log("number of bouncers = " + walls.Length);
        foreach (var item in walls)
        {
            Bouncer bouncer = item.GetComponent<Bouncer>();
            if(bouncer != null)
            {
                bouncer.DeactivateWall();
            }
        }
    }
}
