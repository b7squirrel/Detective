using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] Transform hitPoint;
    private bool boltAlternateToggle = false;

    bool canMultiStrike;
    bool multiStrikeDone;

    // 리드 오리일 때 플레이어 방향 참조
    private Player player;
    private Vector2 currentDir;

    [Header("Effect")]
    [SerializeField] GameObject elecHitEffect;

    [Header("Sounds")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip electricitySound;

    [Header("Hit Effects")]
    [SerializeField] GameObject hitEffectPrefab;

    [Header("전기 볼트")]
    [Tooltip("HammerBolt 설정 에셋 (Project 창에서 생성 후 연결)")]
    [SerializeField] private HammerBoltConfig hammerBoltConfig;

    [Tooltip("기본 사거리 배수 (실제 거리 = baseRange × sizeOfArea)")]
    [SerializeField] private float boltBaseRange = 4f;

    [Tooltip("볼트 유지 시간(초)")]
    [SerializeField] private float boltDuration = 0.5f;

    [Tooltip("시너지 활성화 시 볼트 개수")]
    [SerializeField] private int synergyBoltCount = 2;

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

        player = GetComponentInParent<Player>(); // 동료일 땐 null
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
        // base보다 먼저 currentDir 업데이트 → FlipWeaponTools에 즉시 반영
        if (InitialWeapon && player != null && player.InputVec != Vector2.zero)
            currentDir = player.InputVec;

        base.Update();

        if (isAttacking)
        {
            // animation event 대신 Animator 상태로 공격 종료 감지
            var state = anim.GetCurrentAnimatorStateInfo(0);
            bool stillInAttack = state.IsName("Hammer Attack")
                              || state.IsName("Hammer SynergyAttack")
                              || state.IsName("Hammer BackAttack")
                              || state.IsName("Hammer MultiAttack");

            if (!stillInAttack)
            {
                EndAttack();
                return;
            }

            LockAttackDirection();
        }
    }

    protected override void SetAngle()
    {
        if (InitialWeapon)
        {
            // 리드 오리: 플레이어 입력 방향 사용
            angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
        }
        else
        {
            // 동료 오리: 기존 적 탐색 방향 사용
            base.SetAngle();
        }
    }

    protected override void Attack()
    {
        base.Attack();

        hitEnemiesThisAttack.Clear();
        multiStrikeDone = false;
        canMultiStrike = weaponStats.numberOfAttacks >= 2;
        boltAlternateToggle = false;

        StartAttack();

        if (isSynergyWeaponActivated)
            anim.SetTrigger("SAttack");
        else if (canMultiStrike)
            anim.SetTrigger("MultiAttack");
        else
            anim.SetTrigger("Attack");
    }

    void StartAttack()
    {
        isAttacking = true;

        if (InitialWeapon)
        {
            attackFacingRight = currentDir.x >= 0;
        }
        else
        {
            attackFacingRight = weaponContainerAnim != null
                ? weaponContainerAnim.FacingRight
                : dir.x >= 0;
        }
    }

    // 공격 중: 부모가 뒤집혔다면 로컬 Y=180으로 보정해서 월드 방향 고정
    void LockAttackDirection()
    {
        if (weaponContainerAnim == null) return;

        bool parentFacingRight = weaponContainerAnim.FacingRight;
        bool needsFlip = (attackFacingRight != parentFacingRight);
        transform.localEulerAngles = new Vector3(0, needsFlip ? 180f : 0f, 0);
    }

    // 공격 종료: 로컬 리셋 → 부모 방향이 자동 반영
    void EndAttack()
    {
        isAttacking = false;
        transform.localEulerAngles = Vector3.zero;
    }

    /// <summary>
    /// 범위 공격 (애니메이션 이벤트에서 호출)
    /// hitPoint 위치에서 sizeOfArea 반지름의 원 안에 있는 모든 적 공격
    /// </summary>
    void AttackAtHitPoint()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning("[WhipWeapon] hitPoint가 설정되지 않았습니다!");
            return;
        }

        // 전기 볼트 발사
        FireLightningBolts();

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
                        DamageTracker.instance.RecordDamage(weaponName, damage);

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
    //  전기 볼트 발사 로직
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 공격 방향으로 전기 볼트를 발사합니다.
    /// 일반 공격:        앞으로 1개
    /// 멀티/시너지 공격: AttackAtHitPoint 호출 때마다 앞/뒤 번갈아 1개씩
    /// </summary>
    private void FireLightningBolts()
    {
        if (hitPoint == null) return;

        Vector3 origin = hitPoint.position;
        float boltRange = boltBaseRange * weaponStats.sizeOfArea;
        Vector3 forwardDir = attackFacingRight ? Vector3.right : Vector3.left;

        if (!isSynergyWeaponActivated && !canMultiStrike)
        {
            // 일반 공격: 앞으로 1개
            SpawnSingleBolt(origin, origin + forwardDir * boltRange);
        }
        else
        {
            // 멀티/시너지 공격: 호출될 때마다 앞/뒤 번갈아
            Vector3 boltDir = boltAlternateToggle ? -forwardDir : forwardDir;
            SpawnSingleBolt(origin, origin + boltDir * boltRange);
            boltAlternateToggle = !boltAlternateToggle;
        }
    }

    private void SpawnSingleBolt(Vector3 start, Vector3 end)
    {
        if (hammerBoltConfig == null)
        {
            Debug.LogWarning("[WhipWeapon] HammerBoltConfig가 연결되지 않았습니다!");
            return;
        }

        float adjustedDuration = (isSynergyWeaponActivated || canMultiStrike)
            ? boltDuration * 1.3f
            : boltDuration;

        GameObject hitEffect = hitEffectPrefab;
        if (hitEffect == null)
        {
            HitEffects hitEffects = GetComponent<HitEffects>();
            if (hitEffects != null)
                hitEffect = hitEffects.hitEffect;
        }

        HammerBolt.Create(
            hammerBoltConfig,
            start,
            end,
            adjustedDuration,
            damage,
            knockback,
            knockbackSpeedFactor,
            hitEffect,
            enemy
        );
    }

    // 평상시: 부모 flip을 그대로 따라감
    protected override void FlipWeaponTools()
    {
        if (isAttacking) return;
        transform.localEulerAngles = Vector3.zero;
    }

    // 기본 클래스 LockFlip도 로컬로
    protected override void LockFlip()
    {
        if (!isAttacking)
            transform.localEulerAngles = Vector3.zero;
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

    void PlayElectricitySound()
    {
        if (electricitySound != null)
            SoundManager.instance.PlaySoundWith(electricitySound, 0.5f, true, .1f);
    }

    void GenElecHitEffect()
    {
        if (elecHitEffect == null)
        {
            Logger.LogWarning("[WhipWeapon] elecHitEffect 프리팹이 없습니다!");
            return;
        }

        GameObject effect = GameManager.instance.poolManager.GetMisc(elecHitEffect);

        if (effect == null)
        {
            Logger.LogWarning("[WhipWeapon] GetMisc가 null을 반환했습니다!");
            return;
        }

        effect.transform.position = hitPoint.position;

        float diameter = weaponStats.sizeOfArea * 2f;
        effect.transform.localScale = new Vector3(diameter, diameter, 1f);

        Logger.Log($"[WhipWeapon] 이펙트 위치: {hitPoint.position}, 스케일: {diameter}");
    }

    #endregion

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (hitPoint != null && weaponStats != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(hitPoint.position, weaponStats.sizeOfArea);

            float boltRange = boltBaseRange * weaponStats.sizeOfArea;
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
            Gizmos.DrawLine(hitPoint.position, hitPoint.position + Vector3.right * boltRange);
            Gizmos.DrawLine(hitPoint.position, hitPoint.position + Vector3.left * boltRange);
        }
    }
#endif
}