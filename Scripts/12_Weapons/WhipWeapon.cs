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
    [SerializeField] private HammerBoltConfig hammerBoltConfig;
    [SerializeField] private float boltBaseRange = 4f;
    [SerializeField] private float boltDuration = 0.5f;
    [SerializeField] private int synergyBoltCount = 2;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;

    // ✅ NonAlloc용 버퍼
    readonly Collider2D[] whipHitBuffer = new Collider2D[20];

    private HashSet<Collider2D> hitEnemiesThisAttack = new HashSet<Collider2D>();

    private bool isAttacking = false;
    private bool attackFacingRight = true;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱

        if (weapon != null)
            weapon.SetActive(true);

        player = GetComponentInParent<Player>();
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
        if (InitialWeapon && player != null && player.InputVec != Vector2.zero)
            currentDir = player.InputVec;

        base.Update();

        if (isAttacking)
        {
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
            angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
        else
            base.SetAngle();
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
            attackFacingRight = currentDir.x >= 0;
        else
            attackFacingRight = weaponContainerAnim != null ? weaponContainerAnim.FacingRight : dir.x >= 0;
    }

    void LockAttackDirection()
    {
        if (weaponContainerAnim == null) return;

        bool parentFacingRight = weaponContainerAnim.FacingRight;
        bool needsFlip = attackFacingRight != parentFacingRight;
        transform.localEulerAngles = new Vector3(0, needsFlip ? 180f : 0f, 0);
    }

    void EndAttack()
    {
        isAttacking = false;
        transform.localEulerAngles = Vector3.zero;
    }

    /// <summary>
    /// 범위 공격 (애니메이션 이벤트에서 호출)
    /// </summary>
    void AttackAtHitPoint()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning("[WhipWeapon] hitPoint가 설정되지 않았습니다!");
            return;
        }

        FireLightningBolts();

        Vector2 attackPosition = hitPoint.position;
        float attackRadius = weaponStats.sizeOfArea;

        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(attackPosition, attackRadius, whipHitBuffer, enemy);

        // ✅ 캐싱된 hitEffects 사용 (매 공격마다 GetComponent 제거)
        GameObject hitEffect = hitEffectPrefab != null ? hitEffectPrefab
            : (hitEffects != null ? hitEffects.hitEffect : null);

        for (int i = 0; i < count; i++)
        {
            Collider2D collision = whipHitBuffer[i];
            if (hitEnemiesThisAttack.Contains(collision)) continue;

            if (collision.CompareTag("Enemy"))
            {
                Idamageable enemyTarget = collision.GetComponent<Idamageable>();
                if (enemyTarget != null)
                {
                    PostMessage(damage, collision.transform.position);
                    enemyTarget.TakeDamage(
                        damage, knockback, knockbackSpeedFactor,
                        collision.ClosestPoint(attackPosition), hitEffect);

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
                    prop.TakeDamage(
                        damage, knockback, knockbackSpeedFactor,
                        collision.ClosestPoint(attackPosition), hitEffect);

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

    private void FireLightningBolts()
    {
        if (hitPoint == null) return;

        Vector3 origin = hitPoint.position;
        float boltRange = boltBaseRange * weaponStats.sizeOfArea;
        Vector3 forwardDir = attackFacingRight ? Vector3.right : Vector3.left;

        if (!isSynergyWeaponActivated && !canMultiStrike)
        {
            SpawnSingleBolt(origin, origin + forwardDir * boltRange);
        }
        else
        {
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

        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffectPrefab != null ? hitEffectPrefab
            : (hitEffects != null ? hitEffects.hitEffect : null);

        HammerBolt.Create(
            hammerBoltConfig, start, end, adjustedDuration,
            damage, knockback, knockbackSpeedFactor, hitEffect, enemy);
    }

    protected override void FlipWeaponTools()
    {
        if (isAttacking) return;
        transform.localEulerAngles = Vector3.zero;
    }

    protected override void LockFlip()
    {
        if (!isAttacking)
            transform.localEulerAngles = Vector3.zero;
    }

    #region Animation Events

    void ResetHitList() => hitEnemiesThisAttack.Clear();

    void PlayHitSound()
    {
        if (hitSound != null) SoundManager.instance.Play(hitSound);
    }

    void PlayShootSound()
    {
        if (shootSound != null) SoundManager.instance.Play(shootSound);
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