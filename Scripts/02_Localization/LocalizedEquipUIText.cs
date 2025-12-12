using UnityEngine;
using TMPro;

public class LocalizedEquipUIText : MonoBehaviour
{
    [Header("Equip Panel")]
    [SerializeField] TextMeshProUGUI ori;
    [SerializeField] TextMeshProUGUI head;
    [SerializeField] TextMeshProUGUI chest;
    [SerializeField] TextMeshProUGUI face;
    [SerializeField] TextMeshProUGUI hand;
    [SerializeField] TextMeshProUGUI itemGradeInfo;
    [SerializeField] TextMeshProUGUI itemTitleInfo;
    [SerializeField] TextMeshProUGUI itemSkillTitleInfo;
    [SerializeField] TextMeshProUGUI itemSkillDescriptionInfo;

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
        ori.text = LocalizationManager.Game.duck;
        head.text = LocalizationManager.Game.head;
        chest.text = LocalizationManager.Game.chest;
        face.text = LocalizationManager.Game.face;
        hand.text = LocalizationManager.Game.hand;

        // itemGradeInfo.text = LocalizationManager.Game.hand;
        // itemTitleInfo.text = LocalizationManager.Item.hand;
        // itemSkillTitleInfo.text = LocalizationManager.Item.hand;
        // itemSkillDescriptionInfo.text = LocalizationManager.Item.hand;
    }
}
