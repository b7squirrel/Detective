using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] Transform hitPoint; // ⭐ 공격 중심점
    private bool boltAlternateToggle = false; // 시너지 두 번째 공격이 반대 방향으로 나가도록

    bool canMultiStrike;
    bool multiStrikeDone;

    [Header("Sounds")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;

    [Header("Hit Effects")]
    [SerializeField] GameObject hitEffectPrefab;

    // ─────────────────────────────────────────────────────────
    //  ⚡ 전기 볼트 설정
    // ─────────────────────────────────────────────────────────
    [Header("전기 볼트")]
    [Tooltip("기본 사거리 배수 (실제 거리 = baseRange × sizeOfArea)")]
    [SerializeField] private float boltBaseRange = 4f;

    [Tooltip("볼트 유지 시간(초)")]
    [SerializeField] private float boltDuration = 0.5f;

    [Tooltip("시너지 활성화 시 볼트 개수 (앞뒤 + 양옆 등 확장 가능)")]
    [SerializeField] private int synergyBoltCount = 2; // 앞 + 뒤

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
        boltAlternateToggle = false; // ← 사이클 시작 시 리셋

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

        // ─────────────────────────────────────────────────────
        //  ⚡ 전기 볼트 발사
        // ─────────────────────────────────────────────────────
        FireLightningBolts();

        // ⭐ hitPoint 위치에서 원형 범위 공격
        Vector2 attackPosition = hitPoint.position;
        float attackRadius = weaponStats.sizeOfArea;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRadius,
            enemy
        );

        foreach (Collider2D collision in hitColliders)
        {
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

#if UNITY_EDITOR
        Debug.DrawRay(attackPosition, Vector2.up * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.down * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.left * attackRadius, Color.red, 0.5f);
        Debug.DrawRay(attackPosition, Vector2.right * attackRadius, Color.red, 0.5f);
#endif
    }

    // ─────────────────────────────────────────────────────────────
    //  ⚡ 전기 볼트 발사 로직
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 공격 방향으로 전기 볼트를 발사합니다.
    /// 시너지 비활성화: 공격 방향 1개
    /// 시너지 활성화:   공격 방향 + 반대 방향 (앞뒤 2개)
    /// </summary>
    private void FireLightningBolts()
    {
        if (hitPoint == null) return;

        Vector3 origin = hitPoint.position;
        float boltRange = boltBaseRange * weaponStats.sizeOfArea;
        Vector3 forwardDir = attackFacingRight ? Vector3.right : Vector3.left;

        if (!isSynergyWeaponActivated)
        {
            // 일반 공격: 항상 앞으로 1개
            SpawnSingleBolt(origin, origin + forwardDir * boltRange);
        }
        else
        {
            // 시너지 공격: 호출될 때마다 앞/뒤 번갈아 1개씩
            Vector3 boltDir = boltAlternateToggle ? -forwardDir : forwardDir;
            SpawnSingleBolt(origin, origin + boltDir * boltRange);
            boltAlternateToggle = !boltAlternateToggle; // 다음 호출을 위해 토글
        }
    }

    /// <summary>
    /// LightningBolt 오브젝트를 생성하고 SpawnBolt를 호출합니다.
    /// </summary>
    private void SpawnSingleBolt(Vector3 start, Vector3 end)
    {
        HammerBolt.Create(start, end, boltDuration, damage); // damage는 WeaponBase의 protected 필드
    }

    // ─────────────────────────────────────────────────────────────

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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (hitPoint != null && weaponStats != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(hitPoint.position, weaponStats.sizeOfArea);

            // ⚡ 볼트 사거리 미리보기
            float boltRange = boltBaseRange * weaponStats.sizeOfArea;
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
            Gizmos.DrawLine(hitPoint.position, hitPoint.position + Vector3.right * boltRange);
            Gizmos.DrawLine(hitPoint.position, hitPoint.position + Vector3.left * boltRange);
        }
    }
#endif
}