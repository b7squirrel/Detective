using UnityEngine;

public class InitPanel : MonoBehaviour
{
    [SerializeField] GameObject[] otherPanels;

    public void OnEnable()
    {
        for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(true);
    }

    public void OnDisable()
    {
        for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(false);
    }
}