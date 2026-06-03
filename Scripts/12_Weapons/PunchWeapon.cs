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

    Vector2 prevContainerPos;
    Transform myContainer; // 자신의 WeaponContainerAnim 오브젝트

    protected override void Awake()
    {
        base.Awake();
        boxCol = GetComponentInChildren<BoxCollider2D>();
        anim = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱
        isAttacking = false;

        // 동료 오리용: 자신의 컨테이너(WeaponContainerAnim) Transform 캐싱
        myContainer = GetComponentInParent<WeaponContainerAnim>()?.transform;
    }

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);

        // ✅ 첫 프레임 방향 오류 방지
        if (myContainer != null)
            prevContainerPos = myContainer.position;
    }

    protected override void Update()
    {
        base.Update();
        if (InitialWeapon) // 리드 오리: 기존 방식 유지
        {
            if (player.InputVec == Vector2.zero) return;
            currentDir = player.InputVec;
        }
        else // 동료 오리: 컨테이너의 이동 방향으로 currentDir 계산
        {
            if (myContainer == null) return;

            Vector2 containerPos = myContainer.position;
            Vector2 moved = containerPos - prevContainerPos;

            // 충분히 움직였을 때만 방향 업데이트 (너무 작은 떨림 무시)
            if (moved.magnitude > 0.001f)
            {
                currentDir = moved.normalized;
            }

            prevContainerPos = containerPos; // 다음 프레임을 위해 현재 위치 저장
        }
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