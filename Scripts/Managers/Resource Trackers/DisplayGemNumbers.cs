using UnityEngine;

public class DisplayGemNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI gemNumbers;
    GemManager gemManager;
    

    private void Awake()
    {
        gemManager = GetComponent<GemManager>();
        gemManager.OnGemNumberChange += UpdateGemNumberDisp;
    }

    void UpdateGemNumberDisp()
    {
        gemNumbers.text = gemManager.GetGemNumbers().ToString();
    }

}