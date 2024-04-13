using UnityEngine;

/// <summary>
/// 화면 상의 모든 적에게 데미지
/// </summary>
public class Skill300 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 300;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float skillCounter;
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    float realCoolDownTime;

    [SerializeField] int defaultDamage;
    int realDamage; // 디폴트 데미지에서 계산이 적용된 후의 데미지, 실제로 적에게 들어가는 데미지

    public void Init(SkillManager _skillManager, CardData _cardData)
    {
        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        realDamage = new Equation().GetSkillDamage(rate, Grade, EvoStage, defaultDamage);
    }

    public void UseSkill()
    {
        skillCounter += Time.deltaTime;

        if (skillCounter > realCoolDownTime)
        {
            skillCounter = 0;
            Debug.Log($"Skill Damage {realDamage}");
            Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
            if(allEnemies.Length != 0)
            {
                ApplyDamages(allEnemies);
            }
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
