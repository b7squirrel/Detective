using UnityEngine;
using TMPro;

public class LoadingSwipeHint : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("디버깅")]
    [SerializeField] private bool isDebugging;
    [SerializeField] private int hintIndex;

    private void Start()
    {
        UpdateHint();
        LocalizationManager.OnLanguageChanged += UpdateHint;
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateHint;
    }

    private void UpdateHint()
    {
        if (!LocalizationManager.IsInitialized || LocalizationManager.Game == null)
            return;

        if (isDebugging)
        {
            hintText.text = LocalizationManager.Game.GetHint(hintIndex);
        }
        else
        {
            hintText.text = LocalizationManager.Game.GetRandomHint();
        }
    }
}