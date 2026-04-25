using UnityEngine;

[CreateAssetMenu(menuName = "QuackSurvivors/SetBonusDefinition")]
public class SetBonusDefinition : ScriptableObject
{
    public string setName;           // TSV의 SetName 값과 일치해야 함. 예: "TennisLocal"
    public string bonusDescription;  // 예: "이동 속도 증가"

    // 나중에 실제 수치 추가
    // public float moveSpeedBonus;
    // public int hpBonus;
}