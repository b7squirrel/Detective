using UnityEngine;

[CreateAssetMenu(fileName = "GameTexts", menuName = "Localization/Game Texts")]
public class GameTexts : ScriptableObject
{
    [Header("Loading Messages")]
    public string loadingCardData = "오리 친구들 준비 중...";
    public string loadingPlayerData = "리드 오리 준비 중...";
    public string loadingEquipment = "장비 준비 중...";
    public string loadingComplete = "출동 준비 중...";
    public string loading = "로딩 중";
    public string loadingBackToLobby = "집으로 돌아가는 중...";

    [Header("Grade Names")]
    public string[] gradeNames = new string[]
    {
        "일반", "희귀", "고급", "전설", "신화"
    };

    [Header("UI Labels")]
    public string level = "레벨";
    public string atk = "공격력";
    public string hp = "체력";

    [Header("UI Buttons")]
    public string buttonUpgrade = "업그레이드";
    public string buttonEquip = "장착";
    public string buttonUnequip = "장착 해제";
    public string buttonStart = "출동!";
    public string buttonBack = "뒤로";
    public string buttonConfirm = "확인";
    public string buttonCancel = "취소";
    public string buttonSettings = "설정";
    public string buttonMainMenu = "메인 메뉴";
    public string buttonMerge = "합성";
    public string duckTab = "오리";
    public string itemTab = "아이템";
    public string tabToContinue = "화면을 터치해서 계속하기";
    public string sortBy = "정렬";
    public string sortByName = "이름";
    public string sortByLevel = "등급";
    public string sortByGrade = "레벨";

    [Header("Shop Panel")]
    public string Shop = "상점";
    public string ShopTab = "상점"; // 샵 패널의 대문자와 구분. 탭의 글자는 앞글자만 대문자
    public string beginnerPack = "초보자 지원 세트";
    public string ExpertPack = "전문가 지원 세트";
    public string luckyBox = "행운 상자";
    public string duckCard = "오리 카드";
    public string itemCard = "아이템 카드";
    public string coin = "코인";
    public string cristal = "크리스탈";
    public string singleDraw = "1회 뽑기";
    public string tenXDraw = "10회 뽑기";
    public string watchAdToDraw = "광고 보고 뽑기";

    [Header("Launch Panel")]
    public string tabToSelectLead = "탭해서 리드 오리 선택.";
    public string startButton = "시작!";
    public string hint = "힌트";

    [Header("Equip Panel")]
    public string duck = "오리";
    public string head = "머리";
    public string chest = "가슴";
    public string face = "얼굴";
    public string hand = "손";
    public string equipped = "장착 중";

    [Header("Merge Panel")]
    [TextArea(2, 4)]
    public string unequipBeforeMerging = "머지를 진행하면 재료 카드에 장착된 장비가 자동으로 해제됩니다.";
    public string fuseTheseTwo = "두 카드를 합칠까요?";
    public string UseForMerge = "사용";
    public string Cancel = "취소";
    public string Warning = "경고";

    [Header("기타 패널 제목")]
    public string Gear = "장비";
    public string Homework = "숙제";

    [Header("Achievements Panel")]
    public string achievementsTitle = "업적";
    public string dailyQuestsTitle = "일일 임무";
    public string weeklyQuestsTitle = "주간 임무";
    public string tapToClaim = "탭 해서 보상 받기";

    [Header("Daily Reward Panel")]
    public string daily = "일일 보상";
    public string dailyRewardTitle = "일일 출석";
    public string claimReward = "보상 받기";
    public string alreadyClaimed = "오늘은 이미 받았습니다";
    public string day1 = "1일차";
    public string day2 = "2일차";
    public string day3 = "3일차";
    public string day4 = "4일차";
    public string day5 = "5일차";
    public string day6 = "6일차";
    public string day7 = "7일차";

    [Header("Sell Cards")]
    public string sellCards = "카드 판매";
    [Header("Encyclopedia")]
    public string encyclopedia = "카드 도감";

    [Header("무한 모드")]
    public string duckChallenge = "도전 오리!!";
    public string wave = "웨이브";
    public string survivalTime = "생존 시간";
    public string currentRecord = "현재 기록";
    public string bestWave = "최고 기록";
    public string nextTarget = "다음 목표";
    public string stage = "스테이지";
    public string dailyChallenge = "일일 도전";
    public string weeklyChallenge = "주간 도전";
    public string challenge = "도전!";

    [Header("Field")]
    public string composedBy = "작곡";
    public string coinFrenzy = "코인 파티!";
    public string exp = "경험치";

    [Header("Result")]
    public string congratulations = "축하해요!";
    public string failed = "실패...";
    public string challengeResult = "도전 결과";
    public string killBonus = "적 처치 보상";
    public string waveBonus = "웨이브 보상";
    public string stageReward = "스테이지 보상!";
    public string greatJob = "참 잘했어요!";
    public string soClose = "아쉬워요...";

    [Header("Buff Messages")]
    public string buffSpeedBoost = "빠른 발!";
    public string buffDamageBoost = "공격력 UP!";
    public string buffExpPrefix = "경험치";
    public string buffExpSuffix = "x!";

    [Header("Options")]
    public string language = "언어";
    public string sound = "사운드";
    public string music = "음악";
    public string pause = "일시 정지";
    public string Continue = "계속 하기";
    // 헬퍼 메서드
    public string GetDayText(int day)
    {
        switch (day)
        {
            case 1: return day1;
            case 2: return day2;
            case 3: return day3;
            case 4: return day4;
            case 5: return day5;
            case 6: return day6;
            case 7: return day7;
            default: return $"{day}일차";
        }
    }

    [Header("Stage Name")]
    public string[] stageBossName = new string[]
    {
        "몰랑이 왕자",
        "힘쎈이 왕자",
        "날쎈이 왕자",
        "거대미 왕자",
        "파닥이 왕자",
        "뽀글뽀글 여왕",
        "몰랑이 왕자",
        "힘쎈이 왕자",
        "날쎈이 왕자",
        "거대미 왕자",
        "파닥이 왕자",
        "데굴데굴 여왕",
        "몰랑이 왕자",
        "힘쎈이 왕자",
        "날쎈이 왕자",
        "거대미 왕자",
        "파닥이 왕자",
        "드릴드릴 여왕",
        "몰랑이 왕자",
        "힘쎈이 왕자",
        "날쎈이 왕자",
        "거대미 왕자",
        "파닥이 왕자",
        "삐용삐용 여왕",
        "몰랑이 왕자",
        "힘쎈이 왕자",
        "날쎈이 왕자",
        "거대미 왕자",
        "파닥이 왕자",
        "아뜨아뜨 여왕"
    };

    [Header("Stage Ground Descriptions")]
    [Tooltip("StageGroundType enum 순서와 일치: GreenForest(0), OrangeDesert(1), GreyStone(2), BlueIce(3), GreyLava(4)")]
    [TextArea(2, 4)]
    public string[] stageGroundDescriptions = new string[]
{
    // GreenForest (0)
    "평화로운 숲 스테이지입니다.",
    // OrangeDesert (1)
    "사막의 모래 폭풍은 오리들의 이동을 방해합니다.",
    // GreyStone (2)
    "돌산의 지진이 오리들의 이동속도를 느리게 합니다.",
    // BlueIce (3)
    "얼음판이 미끄러워 이동을 컨트롤하기 어렵습니다.",
    // GreyLava (4)
    "하늘에서 화염구가 떨어져 아군과 적군 모두에게 피해를 줍니다."
};

    // 헬퍼 메서드 - StageGroundType으로 설명 반환
    public string GetStageGroundDescription(StageGroundType groundType)
    {
        int index = (int)groundType;
        if (stageGroundDescriptions != null && index >= 0 && index < stageGroundDescriptions.Length)
            return stageGroundDescriptions[index];
        return "";
    }

    [Header("Game Warning")]
    [TextArea(1, 2)]
    public string[] cardLimitWarnings = new string[]
    {
        "카드가 가득 찼어요",
        "카드를 판매하거나 합성해서 공간을 정리해 주세요"
    };

    [Header("Game Hints")]
    [Tooltip("로딩 중 표시될 힌트들")]
    [TextArea]
    public string[] gameHints = new string[]
    {
        "팁: 체력이 낮을 때는\n상자에서 달콤우유가\n 나올 확률이\n높아집니다.",
        "팁: 월계수는 잠시동안 오리를\n무적으로 만들어 줍니다.",
        "팁: 각 오리들은 시너지 스킬을 얻으면\n시너지 오리로 변신할 수 있습니다.",
        "팁: 동료 오리들이 리드 오리를\n둘러싸도록 해보세요.\n적들의 공격을 막아 줄거에요.",
        "팁: 달콤 우유는 우주에서 가장 맛있는\n음료수입니다. 체력까지 채워줍니다!",
        "팁: 스테이지 클리어를 계속 실패한다면\n카드를 업그레이드 시켜보세요.",
        "팁: 헬멧 적은 단단합니다.\n 더 많은 공격이 필요합니다.",
        "팁: 눈이 빛나는 적은 공격 속도가 빠릅니다.",
        "팁: 심지가 달린 적에게 닿으면 큰 데미지를 입습니다.",
    };

    // 랜덤 힌트 가져오기
    public string GetRandomHint()
    {
        if (gameHints.Length == 0) return "";
        return gameHints[Random.Range(0, gameHints.Length)];
    }
}