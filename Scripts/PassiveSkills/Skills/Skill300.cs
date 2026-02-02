using System.Collections;
using UnityEngine;

/// <summary>
/// 넓은 공격 - Flash Damage
/// </summary>
public class Skill300 : SkillBase
{
    public override SkillType SkillType => SkillType.FlashDamage;
    
    [SerializeField] int defaultDamage;
    int realDamage;
    
    [Header("Duration Upgrade - Multi Hit")]
    [SerializeField] int additionalHitsPerLevel = 1; // 레벨당 추가 공격 횟수
    [SerializeField] float hitInterval = 0.3f; // 추가 공격 간격 (초)
    
    [Header("Debug")]
    [SerializeField] int _durationUpgradeLevel;
    [SerializeField] int _totalHits;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        realDamage = new Equation().GetSkillDamage(rate, Grade, EvoStage, defaultDamage);
    }

    // ⭐ 지속시간 업그레이드 오버라이드
    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        
        int totalHits = 1 + (durationUpgradeLevel * additionalHitsPerLevel);
        Debug.Log($"[Skill300] 💥 다회 공격 업그레이드 LV{level} - 공격 횟수: {totalHits}회");
    }

    public override void UseSkill()
    {
        base.UseSkill();
        _durationUpgradeLevel = durationUpgradeLevel;
        
        if (skillCounter > realCoolDownTime)
        {
            skillCounter = 0;
            
            Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
            if (allEnemies.Length != 0)
            {
                // ⭐ 업그레이드 레벨에 따라 여러 번 공격
                int totalHits = 1 + (durationUpgradeLevel * additionalHitsPerLevel);
                _totalHits = totalHits;
                
                StartCoroutine(MultiHitAttack(allEnemies, totalHits));
            }
            
            skillUi.BadgeUpAnim();
        }
    }

    // ⭐ 다회 공격 코루틴
    IEnumerator MultiHitAttack(Collider2D[] enemies, int hitCount)
    {
        for (int hit = 0; hit < hitCount; hit++)
        {
            // 매 공격마다 현재 살아있는 적들을 다시 찾기
            Collider2D[] currentEnemies = EnemyFinder.instance.GetAllEnemies();
            
            if (currentEnemies.Length > 0)
            {
                ApplyDamages(currentEnemies);
                
                if (hit == 0)
                {
                    Debug.Log($"[Skill300] ⚡ 첫 번째 공격!");
                }
                else
                {
                    Debug.Log($"[Skill300] 💥 추가 공격 {hit}번째!");
                }
            }
            
            // 마지막 공격이 아니면 대기
            if (hit < hitCount - 1)
            {
                yield return new WaitForSeconds(hitInterval);
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