using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    BoxCollider2D boxCol;
    Player player;
    bool canMultiStrike;
    bool multiStrikeDone;

    [Header("Sounds")]
    [SerializeField] AudioClip punch;

    [Header("Hit Effects")]
    [SerializeField] GameObject hitEffectPrefab; // Inspector에서 직접 할당

    // 중복 타격 방지
    private HashSet<Collider2D> hitEnemiesThisAttack = new HashSet<Collider2D>();

    protected override void Awake()
    {
        base.Awake();
        boxCol = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        player = FindObjectOfType<Player>();
        
        if (boxCol != null)
            boxCol.enabled = false; // 초기에는 꺼둠
        
        if (weapon != null)
            weapon.SetActive(true); // 망치는 항상 보임 (Idle 상태)
    }

    public override void SetData(WeaponData wd)
    {
        base.SetData(wd);
        weaponName = wd.DisplayName; // ⭐ DamageTracker용 weaponName 설정
    }

    protected override void Attack()
    {
        base.Attack(); // GetAttackParameters() 호출 (damage, knockback 계산)
        
        // 중복 타격 방지 리스트 초기화
        hitEnemiesThisAttack.Clear();
        multiStrikeDone = false;

        // 멀티 스트라이크 가능 여부 체크
        canMultiStrike = weaponStats.numberOfAttacks >= 2;

        // 방향에 따른 애니메이션 재생
        if (player.FacingDir < 0)
        {
            anim.SetTrigger("PunchL");
        }
        else
        {
            anim.SetTrigger("PunchR");
        }

        SoundManager.instance.Play(punch);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 타격한 적은 건너뛰기
        if (hitEnemiesThisAttack.Contains(collision))
            return;

        // Enemy 태그 체크
        if (collision.CompareTag("Enemy"))
        {
            Idamageable enemy = collision.GetComponent<Idamageable>();
            if (enemy != null)
            {
                // 타격 처리
                PostMessage(damage, collision.transform.position);
                
                // 히트 이펙트
                GameObject hitEffect = hitEffectPrefab;
                if (hitEffect == null)
                {
                    // Fallback: HitEffects 컴포넌트 사용
                    HitEffects hitEffects = GetComponent<HitEffects>();
                    if (hitEffects != null)
                        hitEffect = hitEffects.hitEffect;
                }

                enemy.TakeDamage(
                    damage,
                    knockback,
                    knockbackSpeedFactor,
                    collision.ClosestPoint(transform.position),
                    hitEffect
                );

                // ✨ 데미지 기록 (Enemy에게의 공격에만)
                if (!string.IsNullOrEmpty(weaponName))
                {
                    DamageTracker.instance.RecordDamage(weaponName, damage);
                }

                // 타격한 적 기록
                hitEnemiesThisAttack.Add(collision);
            }
        }
        // Props나 다른 타겟도 처리하려면 여기에 추가
        else if (collision.CompareTag("Props"))
        {
            Idamageable prop = collision.GetComponent<Idamageable>();
            if (prop != null)
            {
                GameObject hitEffect = hitEffectPrefab;
                if (hitEffect == null)
                {
                    HitEffects hitEffects = GetComponent<HitEffects>();
                    if (hitEffects != null)
                        hitEffect = hitEffects.hitEffect;
                }

                prop.TakeDamage(
                    damage,
                    knockback,
                    knockbackSpeedFactor,
                    collision.ClosestPoint(transform.position),
                    hitEffect
                );

                // Props는 데미지 추적하지 않음
                hitEnemiesThisAttack.Add(collision);
            }
        }
    }

    IEnumerator AttackCo(float firstAttackDirection)
    {
        yield return new WaitForSeconds(.1f);

        // ⭐ 두 번째 공격도 새로운 damage/knockback 계산
        GetAttackParameters();
        
        // 두 번째 공격은 새로운 타격 리스트 사용
        hitEnemiesThisAttack.Clear();

        // 반대 방향 애니메이션
        if (firstAttackDirection < 0)
        {
            anim.SetTrigger("PunchR");
        }
        else
        {
            anim.SetTrigger("PunchL");
        }

        SoundManager.instance.Play(punch);
        multiStrikeDone = true;
    }

    protected override void FlipWeaponTools()
    {
        // Whip은 Player의 Flip에 따라 자동으로 회전하므로 별도 처리 불필요
    }

    #region Animation Events
    void BoxColOn()
    {
        if (boxCol != null)
            boxCol.enabled = true;
    }

    void BoxColOff()
    {
        if (boxCol != null)
            boxCol.enabled = false;
    }

    void MultiAttack(float firstAttackDirection)
    {
        if (multiStrikeDone)
            return;

        if (canMultiStrike)
        {
            StartCoroutine(AttackCo(firstAttackDirection));
        }
    }
    #endregion

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Logger.LogError($"[WhipWeapon] 시너지 활성화");
    }
}