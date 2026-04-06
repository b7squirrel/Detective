using UnityEngine;

public class GoldRewardManager : SingletonBehaviour<GoldRewardManager>
{
    int killGoldAccumulated; // 처치 골드 누적값 (게임 중 저장 안 함)

    protected override void Init()
    {
        base.Init();
        killGoldAccumulated = 0;
        Logger.Log("[GoldRewardManager] 초기화 완료");
    }

    // EnemyBase.Die()에서 호출
    public void AddKillGold(int amount)
    {
        killGoldAccumulated += amount;
    }

    // 처치 골드 합계 반환
    public int GetKillGold() => killGoldAccumulated;

    // 클리어 보너스 계산
    public int GetClearBonus(int stageNum)
    {
        int[] baseBonus = { 0, 100, 150, 350, 600, 900, 1500 }; // 인덱스 0은 미사용
        float[] setMultiplier = { 1f, 2.5f, 6f, 14f, 32f };

        int stageInSet = ((stageNum - 1) % 6) + 1; // 세트 내 순서 (1~6)
        int setIndex = (stageNum - 1) / 6;          // 세트 번호 (0~4)
        setIndex = Mathf.Clamp(setIndex, 0, setMultiplier.Length - 1);

        float multiplier = setMultiplier[setIndex];
        int bonus = Mathf.RoundToInt(baseBonus[stageInSet] * multiplier);

        Logger.Log($"[GoldRewardManager] Stage {stageNum} → 세트 내 {stageInSet}번, 배율 x{multiplier}, 클리어 보너스 {bonus}골드");
        return bonus;
    }

    // 결과 패널 호출 후 PlayerDataManager에 반영
    public void ApplyGoldToPlayer(int killGold, int clearBonus)
    {
        int total = killGold + clearBonus;
        PlayerDataManager.Instance.AddCoin(total);
        Logger.Log($"[GoldRewardManager] 처치 골드 {killGold} + 클리어 보너스 {clearBonus} = 총 {total}골드 저장");
    }

    // 스테이지 시작 시 초기화
    public void ResetKillGold()
    {
        killGoldAccumulated = 0;
    }
}