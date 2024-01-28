using UnityEngine;

public class DisplayGemNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI gemNumbers;
    GemManager gemManager;
    

    private void Awake()
    {
        gemManager = GetComponent<GemManager>();
        gemManager.OnGemNumberChange += updateGemNumberDisp;
    }

    void updateGemNumberDisp()
    {
        gemNumbers.text = gemManager.GetGemNumbers().ToString();
    }

}