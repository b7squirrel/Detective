using UnityEngine;

[CreateAssetMenu(menuName = "QuackSurvivors/SetBonusDefinition")]
public class SetBonusDefinition : ScriptableObject
{
    public string setName;           // TSV의 SetName 값과 일치해야 함. 예: "TennisLocal"
    public string bonusDescription;  // 예: "이동 속도 증가"

    [Header("등급별 퍼센트 보너스 (인덱스 0=일반 1=희귀 2=고급 3=전설 4=신화)")]
    public float[] moveSpeedBonus = new float[5];
    public float[] attackBonus = new float[5];
    public float[] maxHpBonus = new float[5];
    public float[] hpRegenBonus = new float[5];
    public float[] magnetSizeBonus = new float[5];
    public float[] knockBackBonus = new float[5];

    [Header("고정값 보너스")]
    public float[] armorBonus = new float[5]; // 절대값 (예: 3 = Armor +3)
    public float[] cooldownBonus = new float[5]; // 초 단위 (예: 0.3 = 쿨타임 0.3초 감소), 감소이므로 양수로 입력 (0.1 = 10% 감소) 
    public float[] criticalChanceBonus = new float[5]; // 0~100 범위 (예: 5 = 크리티컬 +5%)

    void OnValidate()
    {
        ResizeIfNeeded(ref moveSpeedBonus);
        ResizeIfNeeded(ref attackBonus);
        ResizeIfNeeded(ref armorBonus);
        ResizeIfNeeded(ref maxHpBonus);
        ResizeIfNeeded(ref cooldownBonus);
        ResizeIfNeeded(ref criticalChanceBonus);
        ResizeIfNeeded(ref hpRegenBonus);
        ResizeIfNeeded(ref magnetSizeBonus);
        ResizeIfNeeded(ref knockBackBonus);
    }

    void ResizeIfNeeded(ref float[] array)
    {
        if (array == null || array.Length != 5)
        {
            float[] newArray = new float[5];
            if (array != null)
            {
                for (int i = 0; i < Mathf.Min(array.Length, 5); i++)
                    newArray[i] = array[i];
            }
            array = newArray;
        }
    }
}

