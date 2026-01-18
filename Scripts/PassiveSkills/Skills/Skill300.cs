using UnityEngine;

/// <summary>
/// 넓은 공격 - Flash Damage
/// </summary>
public class Skill300 : SkillBase
{
    public override SkillType SkillType => SkillType.FlashDamage;
    [SerializeField] int defaultDamage;
    int realDamage;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        realDamage = new Equation().GetSkillDamage(
            rate, Grade, EvoStage, defaultDamage);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        if (skillCounter > realCoolDownTime)
        {
            skillCounter = 0;
            
            Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
            if (allEnemies.Length != 0)
            {
                ApplyDamages(allEnemies);
            }
            
            skillUi.BadgeUpAnim();
        }
    }

    void ApplyDamages(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Idamageable enemy = colliders[i].transform.GetComponent<Idamageable>();
            GameObject enemyObject = colliders[i].gameObject;
            
            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(realDamage, colliders[i].transform.position);
                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enemy.TakeDamage(realDamage, 0, 0, 
                    Player.instance.transform.position, hitEffect);
            }
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, false);
    }
}