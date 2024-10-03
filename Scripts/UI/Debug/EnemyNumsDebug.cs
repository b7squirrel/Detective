using UnityEngine;

public class EnemyNumsDebug : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI enemyNums;
    Spawner spawner;
    private void Start()
    {
        if (Init()) return;
        Init();
    }
    private void Update()
    {
        //enemyNums.text = spawner.GetCurrentEnemyNums().ToString();
    }

    bool Init()
    {
        spawner = FindObjectOfType<Spawner>();
        return true;
    }
}