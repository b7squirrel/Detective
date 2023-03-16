using System.Collections;
using UnityEngine;

public class BossDieManager : MonoBehaviour
{
    public static BossDieManager instance;
    GameObject deadBody;
    int amountOfCoins;
    Animator anim;
    DropCoins dropCoins;

    void Awake()
    {
        instance = this;
    }
    public void Init(GameObject deadBody, Transform boss, int amountOfCoins)
    {
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
        dropCoins.Init(amountOfCoins, deadBody.transform.position);

        StartCoroutine(WinMessage());
    }

    IEnumerator WinMessage()
    {
        yield return new WaitForSeconds(7f);
        GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    }
    
    public void RemoveAllEnemies()
    {
        LayerMask enemyLayer = LayerMask.NameToLayer("Enmey");

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
}
