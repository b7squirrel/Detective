using UnityEngine;

public class BossHealthBarManager : MonoBehaviour
{
    [SerializeField] GameObject bosshealthBar;
    public void ActivateBossHealthBar()
    {
        if (bosshealthBar == null) return;
        bosshealthBar.SetActive(true);
    }
    public void DeActivateBossHealthBar()
    {
        if (bosshealthBar == null) return;
        bosshealthBar.SetActive(false);
    }
}
