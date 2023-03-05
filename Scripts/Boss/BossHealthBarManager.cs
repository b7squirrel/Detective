using UnityEngine;

public class BossHealthBarManager : MonoBehaviour
{
    [SerializeField] GameObject bosshealthBar;
    public void ActivateBossHealthBar()
    {
        bosshealthBar.SetActive(true);
    }
    public void DeActivateBossHealthBar()
    {
        bosshealthBar.SetActive(false);
    }
}
