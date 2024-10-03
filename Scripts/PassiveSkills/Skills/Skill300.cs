using UnityEngine;

/// <summary>
/// Flash Damage, 화면 상의 모든 적에게 데미지
/// </summary>
public class Skill300 : SkillBase
{
    [SerializeField] int defaultDamage;
    int realDamage; // 디폴트 데미지에서 계산이 적용된 후의 데미지, 실제로 적에게 들어가는 데미지
    private void Awake()
    {
        Name = 300;
        CoolDownTime = 5f;
    }

    public override void Init(SkillManager _skillManager, CardData _cardData)
    {
        base.Init(_skillManager, _cardData);

        realDamage = new Equation().GetSkillDamage(rate, Grade, EvoStage, defaultDamage);
    }

    public override void UseSkill()
    {
        base.UseSkill(); // 스킬 카운터 증가. 증가분을 UI 슬라이더에 반영

        if (skillCounter > realCoolDownTime)
        {
            skillCounter = 0;
            Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
            if(allEnemies.Length != 0)
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
                enemy.TakeDamage(realDamage,
                                 0,
                                 0,
                                 Player.instance.transform.position,
                                 hitEffect);
            }
        }
    }
    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, false);
    }
}
