using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] Transform hitPoint; // ⭐ 공격 중심점
    
    bool canMultiStrike;
    bool multiStrikeDone;

    [Header("Sounds")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;

    [Header("Hit Effects")]
    [SerializeField] GameObject hitEffectPrefab;

    private HashSet<Collider2D> hitEnemiesThisAttack = new HashSet<Collider2D>();

    // 공격 방향 고정용
    private bool isAttacking = false;
    private bool attackFacingRight = true;

    protected override void Awake()
    {
        base.Awake();
        
        anim = GetComponentInChildren<Animator>();
        
        if (weapon != null)
            weapon.SetActive(true);
    }

    public override void SetData(WeaponData wd)
    {
        base.SetData(wd);
        weaponName = wd.DisplayName;
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Logger.Log($"[WhipWeapon] 시너지 활성화!");
    }

    protected override void Update()
    {
        base.Update();
        
        if (isAttacking)
        {
            LockAttackDirection();
        }
    }

    protected override void Attack()
    {
        base.Attack();
        
        hitEnemiesThisAttack.Clear();
        multiStrikeDone = false;
        canMultiStrike = weaponStats.numberOfAttacks >= 2;

        StartAttack();

        if (!isSynergyWeaponActivated)
        {
            anim.SetTrigger("Attack");
        }
        else
        {
            anim.SetTrigger("SAttack");
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        
        if (weaponContainerAnim != null)
        {
            attackFacingRight = weaponContainerAnim.FacingRight;
        }
        else
        {
            attackFacingRight = dir.x >= 0;
        }
    }

    void LockAttackDirection()
    {
        float yRotation = attackFacingRight ? 0f : 180f;
        transform.eulerAngles = new Vector3(0, yRotation, 0);
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    /// <summary>
    /// ⭐ 범위 공격 (애니메이션 이벤트에서 호출)
    /// hitPoint 위치에서 sizeOfArea 반지름의 원 안에 있는 모든 적 공격
    /// </summary>
    void AttackAtHitPoint()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning("[WhipWeapon] hitPoint가 설정되지 않았습니다!");
            return;
        }

        // ⭐ hitPoint 위치에서 원형 범위 공격
        Vector2 attackPosition = hitPoint.position;
        float attackRadius = weaponStats.sizeOfArea;

        // 범위 내 모든 콜라이더 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            attackPosition, 
            attackRadius, 
            enemy // WeaponBase의 enemy LayerMask 사용
        );

        // 각 적에게 데미지
        foreach (Collider2D collision in hitColliders)
        {
            // 이미 타격한 적은 건너뛰기
            if (hitEnemiesThisAttack.Contains(collision))
                continue;

            if (collision.CompareTag("Enemy"))
            {
                Idamageable enemyTarget = collision.GetComponent<Idamageable>();
                if (enemyTarget != null)
                {
                    PostMessage(damage, collision.transform.position);
                    
                    GameObject hitEffect = hitEffectPrefab;
                    if (hitEffect == null)
                    {
                        HitEffects hitEffects = GetComponent<HitEffects>();
                        if (hitEffects != null)
                            hitEffect = hitEffects.hitEffect;
                    }

                    enemyTarget.TakeDamage(
                        damage,
                        knockback,
                        knockbackSpeedFactor,
                        collision.ClosestPoint(attackPosition),
                        hitEffect
                    );

                    if (!string.IsNullOrEmpty(weaponName))
                    {
                        DamageTracker.instance.RecordDamage(weaponName, damage);
                    }

                    hitEnemiesThisAttack.Add(collision);
                }
            }
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
                        collision.ClosestPoint(attackPosition),
                        hitEffect
                    );

                    hitEnemiesThisAttack.Add(collision);
                }
            }
        }

        // ⭐ 디버그용: 공격 범위 시각화 (선택사항)
        #if UNITY_EDITOR
        Debug.DrawRay(attackPosition, Vector2.up * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.down * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.left * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.right * attackRadius, Color.red, 0.5f);
        #endif
    }

    IEnumerator AttackCo(float firstAttackDirection)
    {
        yield return new WaitForSeconds(.1f);

        GetAttackParameters();
        hitEnemiesThisAttack.Clear();

        StartAttack();

        if (!isSynergyWeaponActivated)
        {
            anim.SetTrigger("Attack");
        }
        else
        {
            anim.SetTrigger("SAttack");
        }
        
        multiStrikeDone = true;
    }

    protected override void FlipWeaponTools()
    {
        if (!isAttacking && weaponContainerAnim != null)
        {
            float yRotation = weaponContainerAnim.FacingRight ? 0f : 180f;
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }

    protected override void LockFlip()
    {
        if (!isAttacking)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    #region Animation Events
    // AttackAtHitPoint 사용
    
    void ResetHitList()
    {
        hitEnemiesThisAttack.Clear();
    }

    void PlayHitSound()
    {
        if (hitSound != null)
            SoundManager.instance.Play(hitSound);
    }
    
    void PlayShootSound()
    {
        if (shootSound != null)
            SoundManager.instance.Play(shootSound);
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

    // ⭐ 디버그용: Scene 뷰에서 공격 범위 표시
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (hitPoint != null && weaponStats != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(hitPoint.position, weaponStats.sizeOfArea);
        }
    }
    #endif
}