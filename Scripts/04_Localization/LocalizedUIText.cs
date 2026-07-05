using UnityEngine;
using TMPro;

public class LocalizedUIText : MonoBehaviour
{
    [Header("Text Key")]
    [SerializeField] private UITextKey textKey;
    
    private TMP_Text textComponent;
    
    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
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
            case UITextKey.StartButton:
                return LocalizationManager.Game.startButton;
            case UITextKey.Shop:
                return LocalizationManager.Game.Shop;
            case UITextKey.DailyMission:
                return LocalizationManager.Game.dailyQuestsTitle;
            case UITextKey.Achievement:
                return LocalizationManager.Game.achievementsTitle;
            case UITextKey.ShopTab:
                return LocalizationManager.Game.ShopTab;
            case UITextKey.Stage:
                return LocalizationManager.Game.stage;
            case UITextKey.Pause:
                return LocalizationManager.Game.pause;
            case UITextKey.Continue:
                return LocalizationManager.Game.Continue;
            case UITextKey.WeeklyMission:
                return LocalizationManager.Game.weeklyQuestsTitle;
            case UITextKey.DailyChallenge:
                return LocalizationManager.Game.dailyChallenge;
            case UITextKey.WeeklyChallenge:
                return LocalizationManager.Game.weeklyChallenge;
            case UITextKey.Challenge:
                return LocalizationManager.Game.challenge;
            case UITextKey.SellCards:
                return LocalizationManager.Game.sellCards;
            case UITextKey.Encyclopedia:
                return LocalizationManager.Game.encyclopedia;
            case UITextKey.Hint:
                return LocalizationManager.Game.hint;
            case UITextKey.Equipped:
                return LocalizationManager.Game.equipped;
            case UITextKey.TapToClaim:
                return LocalizationManager.Game.tapToClaim;
            case UITextKey.ComposedBy:
                return LocalizationManager.Game.composedBy;
            case UITextKey.CoinFrenzy:
                return LocalizationManager.Game.coinFrenzy;
            case UITextKey.EXP:
                return LocalizationManager.Game.exp;
            case UITextKey.Loading:
                return LocalizationManager.Game.loading;
            case UITextKey.Congratulations:
                return LocalizationManager.Game.congratulations;
            case UITextKey.Failed:
                return LocalizationManager.Game.failed;
            case UITextKey.KillBonus:
                return LocalizationManager.Game.killBonus;
            case UITextKey.StageReward:
                return LocalizationManager.Game.stageReward;
            case UITextKey.GreatJob:
                return LocalizationManager.Game.greatJob;
            case UITextKey.SoClose:
                return LocalizationManager.Game.soClose;
            case UITextKey.FuseTheseTwo:
                return LocalizationManager.Game.fuseTheseTwo;
            case UITextKey.MaxLevel:
                return LocalizationManager.Game.maxLevel;
            case UITextKey.NoItemsToEquip:
                return LocalizationManager.Game.noItemsToEquip;
            case UITextKey.Warning:
                return LocalizationManager.Game.Warning;
            case UITextKey.NewFriend:
                return LocalizationManager.Game.newFriend;
            case UITextKey.Tap:
                return LocalizationManager.Game.tap;
            case UITextKey.RookieDuck:
                return LocalizationManager.Game.rookieDuck;
            case UITextKey.BraveDuck:
                return LocalizationManager.Game.braveDuck;
            case UITextKey.SuperDuck:
                return LocalizationManager.Game.superDuck;
            case UITextKey.ChooseASkill:
                return LocalizationManager.Game.chooseASkill;
            case UITextKey.Synergy:
                return LocalizationManager.Game.synergy;
            case UITextKey.SynergyItem:
                return LocalizationManager.Game.synergyItem;
            case UITextKey.GetBackUp:
                return LocalizationManager.Game.getBackUp;
            case UITextKey.WatchAdnRevive:
                return LocalizationManager.Game.watchAdnRevive;
            case UITextKey.GiveUp:
                return LocalizationManager.Game.giveUp;
            case UITextKey.Coin:
                return LocalizationManager.Game.coin;
            case UITextKey.Attack:
                return LocalizationManager.Game.buffDamageBoost;
            case UITextKey.CardDeckFull:
                return LocalizationManager.Game.cardDeckFull;
            case UITextKey.EnemiesIncoming:
                return LocalizationManager.Game.enemiesIncoming;
            case UITextKey.Haptic:
                return LocalizationManager.Game.haptic;
            case UITextKey.AllClearTitle:
                return LocalizationManager.Game.allClearTitle;
            case UITextKey.AllClearDescription:
                return LocalizationManager.Game.allClearDescription;
            case UITextKey.WeeklyQuestReward:
                return LocalizationManager.Game.weeklyQuestsReward;
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
    Homework,
    StartButton,
    //업적
    DailyMission,
    Achievement,
    ShopTab,
    //스테이지
    Stage,
    Pause,
    Continue,
    WeeklyMission,
    DailyChallenge,
    WeeklyChallenge,
    Challenge,
    SellCards,
    Encyclopedia,
    Hint,
    Equipped,
    TapToClaim,
    ComposedBy,
    CoinFrenzy,
    EXP,
    Loading,
    Congratulations,
    Failed,
    KillBonus,
    StageReward,
    GreatJob,
    SoClose,
    FuseTheseTwo,
    MaxLevel,
    NoItemsToEquip,
    NewFriend,
    Tap,
    RookieDuck,
    BraveDuck,
    SuperDuck,
    ChooseASkill,
    Synergy,
    SynergyItem,
    GetBackUp,
    WatchAdnRevive,
    GiveUp,
    Coin,
    Attack,
    CardDeckFull,
    EnemiesIncoming,
    Haptic,
    AllClearTitle,
    AllClearDescription,
    WeeklyQuestReward
}