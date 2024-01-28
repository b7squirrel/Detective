using UnityEngine;

public class Kills : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI killsCountText;

    void Start()
    {
        Add(0);
    }

    public void Add(int killAmount)
    {
        dataContainer.kills += killAmount;
        killsCountText.text = dataContainer.kills.ToString();
    }
}
