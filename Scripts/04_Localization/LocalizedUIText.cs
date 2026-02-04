using UnityEngine;
using TMPro;

public class LocalizedUIText : MonoBehaviour
{
    [Header("Text Key")]
    [SerializeField] private UITextKey textKey;
    
    private TextMeshProUGUI textComponent;
    
    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError($"TextMeshProUGUI component not found on {gameObject.name}");
            return;
        }
        
        // 언어 변경 이벤트 구독
        LocalizationManager.OnLanguageChanged += UpdateText;

        UpdateText();
    }
    
    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }
    
    void UpdateText()
    {
        if (textComponent == null || LocalizationManager.Game == null) return;
        
        textComponent.text = GetLocalizedText(textKey);
    }
    
    private string GetLocalizedText(UITextKey key)
    {
        switch (key)
        {
            case UITextKey.Upgrade:
                return LocalizationManager.Game.buttonUpgrade;
            case UITextKey.Equip:
                return LocalizationManager.Game.buttonEquip;
            case UITextKey.Unequip:
                return LocalizationManager.Game.buttonUnequip;
            case UITextKey.Start:
                return LocalizationManager.Game.buttonStart;
            case UITextKey.Back:
                return LocalizationManager.Game.buttonBack;
            case UITextKey.Confirm:
                return LocalizationManager.Game.buttonConfirm;
            case UITextKey.Cancel:
                return LocalizationManager.Game.buttonCancel;
            case UITextKey.Settings:
                return LocalizationManager.Game.buttonSettings;
            case UITextKey.MainMenu:
                return LocalizationManager.Game.buttonMainMenu;
            case UITextKey.TapToSelectLead:
                return LocalizationManager.Game.tabToSelectLead;
            case UITextKey.Merge:
                return LocalizationManager.Game.buttonMerge;
            case UITextKey.DuckTab:
                return LocalizationManager.Game.duckTab;
            case UITextKey.ItemTab:
                return LocalizationManager.Game.itemTab;
            case UITextKey.TabToContinue:
                return LocalizationManager.Game.tabToContinue;
            case UITextKey.SortBy:
                return LocalizationManager.Game.sortBy;
            case UITextKey.SortByName:
                return LocalizationManager.Game.sortByName;
            case UITextKey.SortByLevel:
                return LocalizationManager.Game.sortByLevel;
            case UITextKey.SortByGrade:
                return LocalizationManager.Game.sortByGrade;
            case UITextKey.Language:
                return LocalizationManager.Game.language;
            case UITextKey.Sound:
                return LocalizationManager.Game.sound;
            case UITextKey.Music:
                return LocalizationManager.Game.music;
            case UITextKey.DuckChallenge:
                return LocalizationManager.Game.duckChallenge;
            case UITextKey.Daily:
                return LocalizationManager.Game.daily;
            case UITextKey.DailyRewardTitle:
                return LocalizationManager.Game.dailyRewardTitle;
            case UITextKey.ClaimReward:
                return LocalizationManager.Game.claimReward;
            case UITextKey.AlreadyClaimed:
                return LocalizationManager.Game.alreadyClaimed;
            case UITextKey.Day1:
                return LocalizationManager.Game.day1;
            case UITextKey.Day2:
                return LocalizationManager.Game.day2;
            case UITextKey.Day3:
                return LocalizationManager.Game.day3;
            case UITextKey.Day4:
                return LocalizationManager.Game.day4;
            case UITextKey.Day5:
                return LocalizationManager.Game.day5;
            case UITextKey.Day6:
                return LocalizationManager.Game.day6;
            case UITextKey.Day7:
                return LocalizationManager.Game.day7;
            case UITextKey.Wave:
                return LocalizationManager.Game.wave;
            case UITextKey.SurvivalTime:
                return LocalizationManager.Game.survivalTime;
            case UITextKey.BestWave:
                return LocalizationManager.Game.bestWave;
            case UITextKey.NextTarget:
                return LocalizationManager.Game.nextTarget;
            case UITextKey.UnequipBeforeMerging:
                return LocalizationManager.Game.unequipBeforeMerging;
            case UITextKey.Gear:
                return LocalizationManager.Game.Gear;
            case UITextKey.Homework:
                return LocalizationManager.Game.Homework;
            default:
                return key.ToString();
        }
    }
}

public enum UITextKey
{
    Upgrade,
    Equip,
    Unequip,
    Start,
    Back,
    Confirm,
    Cancel,
    Settings,
    MainMenu,
    TapToSelectLead,
    Shop,
    BeginnerPack,
    ExpertPack,
    LuckyBox,
    DuckCard,
    ItemCard,
    SingleDraw,
    tenXDraw,
    WatchAdToDraw,
    Merge,
    DuckTab,
    ItemTab,
    TabToContinue,
    SortBy,
    SortByName,
    SortByLevel,
    SortByGrade,
    Language,
    Sound,
    Music,
    DuckChallenge,
    DailyRewardTitle,
    Daily,
    ClaimReward,
    AlreadyClaimed,
    Day1,
    Day2,
    Day3,
    Day4,
    Day5,
    Day6,
    Day7,
    Wave,
    SurvivalTime,
    BestWave,
    NextTarget,
    Warning,
    UnequipBeforeMerging,
    Gear,
    Homework
}