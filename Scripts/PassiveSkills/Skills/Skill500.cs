using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 오리들의 공격력 증가.
/// </summary>
public class Skill500 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 500;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float skillCounter;
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    float realCoolDownTime;

    [SerializeField] int defaultDamageBonus;
    int realDamageBonus; // 디폴트 데미지에서 계산이 적용된 후의 데미지, 실제로 적에게 드러가는 데미지

    public void UseSkill()
    {
        if (skillCounter > new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime))
        {
            realDamageBonus += 20;

            Debug.Log($"Skill Damage Bonus  {realDamageBonus}");
        }
    }
}
