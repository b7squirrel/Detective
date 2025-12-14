using TMPro;
using UnityEngine;

public class LocalizedMergeUIText : MonoBehaviour
{
    [Header("Equip Panel")]
    [SerializeField] TextMeshProUGUI useEquippedOneForMerge;
    [SerializeField] TextMeshProUGUI UseForMerge;
    [SerializeField] TextMeshProUGUI Cancel;
    [SerializeField] TextMeshProUGUI Warning;

    void Awake()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
    }
    void Start()
    {
        UpdateText();
    }
    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }
    void UpdateText()
    {
        useEquippedOneForMerge.text = LocalizationManager.Game.useEquippedOneForMerge;
        UseForMerge.text = LocalizationManager.Game.UseForMerge;
        Cancel.text = LocalizationManager.Game.Cancel;
        Warning.text = LocalizationManager.Game.Warning;

    }
}
