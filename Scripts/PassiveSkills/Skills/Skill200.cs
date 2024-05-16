using UnityEngine;

/// <summary>
/// Sluggish Slumber. 적을 느리게 만드는 스킬
/// </summary>
public class Skill200 : SkillBase
{
    [SerializeField] float defaultDuration; // 인스펙터에서 입력
    float realDuration;
    float durationTImer;

    float slownessFactor; // 적의 속도 앞에 곱해줘서 속도를 낮추는 역할을 하는 팩터

    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTImer;
    private void Awake()
    {
        Name = 200;
        CoolDownTime = 5f;
    }
    public override void Init(SkillManager _skillManager, CardData _cardData)
    {
        base.Init(_skillManager, _cardData);

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultDuration);
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
    }

    public override void UseSkill()
    {
        base.UseSkill();
        //DebugValues();
        if (skillCounter > realCoolDownTime)
        {
            if (durationTImer > realDuration)
            {
                skillCounter = 0;
                durationTImer = 0;
                Collider2D[] allSlowEnemies = EnemyFinder.instance.GetAllEnemies();
                if (allSlowEnemies == null) return;
                for (int i = 0; i < allSlowEnemies.Length; i++)
                {
                    if (allSlowEnemies[i].GetComponent<EnemyBase>() != null)
                    {
                        EnemyBase enemy = allSlowEnemies[i].GetComponent<EnemyBase>();
                        if (enemy.IsSlowed == false) continue; // 느려지지 않은 적은 건너뛰기

                        enemy.IsSlowed = false; 
                        enemy.ResetCurrentSpeedToDefault();
                    }
                }
            }
            else
            {
                // duration 쿨타임이 끝나지 않았다면 계속 느리게 만들기
                Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
                if (allEnemies == null) return;
                for (int i = 0; i < allEnemies.Length; i++)
                {
                    if (allEnemies[i].GetComponent<EnemyBase>() != null)
                    {
                        EnemyBase enemy = allEnemies[i].GetComponent<EnemyBase>();
                        if (enemy.IsSlowed) continue;

                        enemy.IsSlowed = true;
                        enemy.CastSlownessToEnemy(slownessFactor);
                    }
                }
                durationTImer += Time.deltaTime;
                return;
            }
        }
    }
    void DebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTImer = durationTImer;
    }
}
