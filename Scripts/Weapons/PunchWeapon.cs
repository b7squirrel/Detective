using UnityEngine;

public class PunchWeapon : WeaponBase
{
    [SerializeField] Transform punchSpin;
    [SerializeField] Transform punchPrefab;
    BoxCollider2D boxCol;
    Player player; // 무기 방향을 정하는 InputVec을 가져오기 위해
    Vector2 currentDir; // 정지해 있을 때의 방향을 정하기 위해
    bool isAttacking; // 공격 중일 떄는 무기가 회전하지 않도록 하기 위해
    [SerializeField] SpriteRenderer sr;
    [SerializeField] float SynergyKnockBackSpeedFactor;

    [Header("Sounds")]
    [SerializeField] AudioClip punch;
    [SerializeField] AudioClip punchSynergy;
    

    protected override void Awake()
    {
        base.Awake();
        boxCol = GetComponentInChildren<BoxCollider2D>();
        anim = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
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
            // Attck 할 때 damge와 knockback 값을 가져와서 저장했음
        }
    }

    public void CastDamage(Idamageable enemy, Transform enemyTrans, Vector3 contactPos)
    {
        if (enemyTrans.GetComponent<DestructableObject>() == null)
            PostMessage(damage, enemyTrans.position);

        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        if (isSynergyWeaponActivated)
        {
            enemy.TakeDamage(damage, knockback, SynergyKnockBackSpeedFactor, transform.position, hitEffect);
        }
        else
        {
            enemy.TakeDamage(damage, knockback, knockbackSpeedFactor, transform.position, hitEffect);
        }
    }

    // 공격을 할 동안은 무기의 회전이나 Flip이 없어야 함
    protected override void Attack()
    {
        base.Attack();

        isAttacking = true;
        if(isSynergyWeaponActivated)
        {
            anim.SetTrigger("AttackSynergy");
            SoundManager.instance.PlaySoundWith(punchSynergy, .4f, true);
        }
        else
        {
            anim.SetTrigger("Attack");
            SoundManager.instance.Play(punch);
        }
    }

    // IEnumerator AttackCo()
    // {

    // }

    protected override void RotateWeapon()
    {
        if (GameManager.instance.IsPaused) return;

        if (isAttacking) return;

        // Quaternion targetAngle = Quaternion.Euler(0, 0, angle);

        // punchSpin.rotation = Quaternion.Slerp(punchSpin.rotation, targetAngle, .6f);

        punchSpin.eulerAngles = new Vector3(0, 0, angle);
    }

    protected override void FlipWeaponTools()
    {
        if(GameManager.instance.IsPaused) return;
        if (isAttacking) return;

        if (currentDir.x > 0)
        {
            sr.flipY = false;
        }
        else if (currentDir.x < 0)
        {
            sr.flipY = true;
        }
    }

    // Essential Container로 Essectial Weapon을 넣을 타이밍을 보기 위해
    public bool CheckIsAttacking()
    {
        return isAttacking;
    }

    // animation events
    void BoxColOn() => boxCol.enabled = true;
    void BoxColOff() => boxCol.enabled = false;
    void EndAttack() => isAttacking = false;
}
