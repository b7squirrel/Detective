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
    
    [Header("Grade Names")]
    public string[] gradeNames = new string[]
    {
        "일반", "희귀", "고급", "전설", "신화"
    };
    
    [Header("UI Labels")]
    public string level = "레벨";
    public string atk = "공격력";
    public string hp = "체력";
    
    [Header("Launch Panel")]
    public string tabToSelectLead = "탭해서 리드 오리 선택.";
    public string startButton = "시작!";

    [Header("Equip Panel")]
    public string upgrade = "업그레이드";
    public string equip = "장착";
    public string unequip = "장비 해제";
    public string duck = "오리";
    public string head = "머리";
    public string chest ="가슴";
    public string face = "얼굴";
    public string hand = "손";

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

    [Header("Game Hints")]
    [Tooltip("로딩 중 표시될 힌트들")]
    public string[] gameHints = new string[]
    {
        "팁: 체력이 낮을 때는\n상자에서 달콤우유가\n 나올 확률이\n높아집니다.",
        "팁: 월계수는 잠시동안 오리를\n무적으로 만들어 줍니다.",
        "팁: 각 오리들은 시너지 스킬을 얻으면\n시너지 오리로 변신할 수 있습니다.",
        "팁: 동료 오리들이 리드 오리를\n둘러싸도록 해보세요.\n적들의 공격을 막아 줄거에요.",
        "팁: 달콤 우유는 우주에서 가장 맛있는\n음료수입니다. 체력까지 채워줍니다!",
        "팁: 스테이지 클리어를 계속 실패한다면\n카드를 업그레이드 시켜보세요.",
    };
    
    // 랜덤 힌트 가져오기
    public string GetRandomHint()
    {
        if (gameHints.Length == 0) return "";
        return gameHints[Random.Range(0, gameHints.Length)];
    }
}