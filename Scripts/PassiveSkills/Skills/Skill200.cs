using UnityEngine;

/// <summary>
/// Sluggish Slumber. 적을 느리게 만드는 스킬
/// </summary>
public class Skill200 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 200;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    float cooldownCounter;
    float realCoolDownTime;
    [SerializeField] float defaultDuration; // 인스펙터에서 입력
    float realDuration;
    float durationTImer;

    float slownessFactor; // 적의 속도 앞에 곱해줘서 속도를 낮추는 역할을 하는 팩터

    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTImer;
    public void Init(SkillManager _skillManager, CardData _cardData)
    {
        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultDuration);
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
        Debug.Log($"Slowness Factor = {slownessFactor}");
    }

    public void UseSkill()
    {
        //DebugValues();
        if (cooldownCounter > realCoolDownTime)
        {
            Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
            if (allEnemies == null) return;
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if(allEnemies[i].GetComponent<EnemyBase>() != null)
                {
                    EnemyBase enemy = allEnemies[i].GetComponent<EnemyBase>();
                    if (enemy.IsSlowed) continue;

                    enemy.IsSlowed = true;
                    enemy.Stats.speed += enemy.Stats.speed * slownessFactor;
                }
            }
        }
        cooldownCounter += Time.deltaTime;
    }
    void DebugValues()
    {
        _cooldownCounter = cooldownCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTImer = durationTImer;
    }
}
