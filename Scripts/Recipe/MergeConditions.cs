using UnityEngine;

/// <summary>
/// 카드 합성(머지) 시 재료로 사용 가능한지 판단하는 공통 조건.
/// GetCardsAvailableForMat(화면 노출)과 CheckIsEquipped(클릭 시 최종검증)가
/// 반드시 이 메서드 하나만 사용하도록 하여, 두 곳의 조건이 어긋나는 것을 방지한다.
/// </summary>
public static class MergeConditions
{
    public static bool IsValidMaterial(CardData candidate, CardData upCard, out string reason)
    {
        reason = null;

        if (candidate.StartingMember == StartingMember.Zero.ToString())
        {
            reason = "리드 오리는 재료 카드로 사용할 수 없습니다.";
            return false;
        }
        if (candidate.Grade != upCard.Grade)
        {
            reason = "같은 등급을 합쳐줘야 합니다";
            return false;
        }
        if (candidate.Type != upCard.Type)
        {
            reason = "같은 종류(오리/아이템)의 카드만 합성할 수 있습니다.";
            return false;
        }
        if (candidate.EvoStage != upCard.EvoStage)
        {
            reason = "같은 합성 등급의 카드만 합성이 가능합니다.";
            return false;
        }
        if (candidate.Level != StaticValues.MaxLevel)
        {
            reason = "최고 레벨의 카드만 합성이 가능합니다";
            return false;
        }
        return true;
    }
}