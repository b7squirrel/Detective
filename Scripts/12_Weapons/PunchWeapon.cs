using UnityEngine;

public class PunchWeapon : WeaponBase
{
    [SerializeField] Transform punchSpin;
    [SerializeField] Transform punchPrefab;
    BoxCollider2D boxCol;
    Player player;
    Vector2 currentDir;
    bool isAttacking;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] float SynergyKnockBackSpeedFactor;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;

    [Header("Sounds")]
    [SerializeField] AudioClip punch;
    [SerializeField] AudioClip punchSynergy;

    protected override void Awake()
    {
        base.Awake();
        boxCol = GetComponentInChildren<BoxCollider2D>();
        anim = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱
        isAttacking = false;
    }

    protected override void Update()
    {
        base.Update();
        if (player.InputVec == Vector2.zero) return;
        currentDir = player.InputVec;
    }

    protected override void SetAngle()
    {
        angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Idamageable enemy = collision.transform.GetComponent<Idamageable>();
        if (enemy != null)
        {
            // Attack()에서 damage와 knockback 값을 가져와서 저장했음
        }
    }

    public void CastDamage(Idamageable enemy, Transform enemyTrans, Vector3 contactPos)
    {
        if (enemyTrans.GetComponent<DestructableObject>() == null)
            PostMessage(damage, enemyTrans.position);

        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        if (isSynergyWeaponActivated)
            enemy.TakeDamage(damage, knockback, SynergyKnockBackSpeedFactor, transform.position, hitEffect);
        else
            enemy.TakeDamage(damage, knockback, knockbackSpeedFactor, transform.position, hitEffect);

        if (weaponData != null)
            DamageTracker.instance.RecordDamage(weaponData.DisplayName, damage);
    }

    protected override void Attack()
    {
        base.Attack();

        isAttacking = true;
        if (isSynergyWeaponActivated)
        {
            anim.SetTrigger("AttackSynergy");
            SoundManager.instance.PlaySoundWith(punchSynergy, .4f, true, .1f);
        }
        else
        {
            anim.SetTrigger("Attack");
            SoundManager.instance.Play(punch);
        }
    }

    protected override void RotateWeapon()
    {
        if (GameManager.instance.IsPaused) return;
        if (isAttacking) return;
        punchSpin.eulerAngles = new Vector3(0, 0, angle);
    }

    protected override void FlipWeaponTools()
    {
        if (GameManager.instance.IsPaused) return;
        if (isAttacking) return;

        if (currentDir.x > 0)
            sr.flipY = false;
        else if (currentDir.x < 0)
            sr.flipY = true;
    }

    public bool CheckIsAttacking()
    {
        return isAttacking;
    }

    // animation events
    void BoxColOn() => boxCol.enabled = true;
    void BoxColOff() => boxCol.enabled = false;
    void EndAttack() => isAttacking = false;
}