using UnityEngine;

public class PitcherWeapon : WeaponBase
{
    [SerializeField] GameObject weaponBall;  // 야구공 프리팹 (폴백)
    [SerializeField] AudioClip shoot;
    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // ⭐ 런타임에 결정되는 프로젝타일
    GameObject currentBallPrefab;

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentBallPrefab = equippedItem.projectilePrefab;
        }
        else
        {
            currentBallPrefab = weaponBall;
        }

        // ⭐ weaponTools를 통해 Bridge를 찾아서 자신을 등록
        if (weaponTools != null)
        {
            WeaponAnimEventBridge bridge = weaponTools.GetComponentInChildren<WeaponAnimEventBridge>();
            if (bridge != null)
            {
                bridge.SetWeapon(this);
            }
            else
            {
                Debug.LogWarning("[PitcherWeapon] Bridge를 찾을 수 없습니다!");
            }
        }
    }

    protected override void Attack()
    {
        if (dir == Vector2.zero) return;

        // ⭐ 데미지/넉백 파라미터를 미리 계산해둠
        GetAttackParameters();

        // ⭐ 애니메이션만 재생. ThrowBall()은 Animation Event가 호출
        AnimShoot();
    }

    // ⭐ Animation Event → Bridge → 여기 호출
    public override void OnAnimEvent()
    {
        ThrowBall();
    }

    public void ThrowBall()
    {
        if (currentBallPrefab == null)
        {
            Logger.LogError("[PitcherWeapon] currentBallPrefab이 null입니다!");
            return;
        }

        SoundManager.instance.Play(shoot);

        // 머즐 이펙트
        Transform muzzleEffect = GameManager.instance.poolManager.GetMisc(muzzleFlash).transform;
        muzzleEffect.transform.position = ShootPoint.position;

        // ⭐ currentBallPrefab 사용
        GameObject ball = GameManager.instance.poolManager.GetMisc(currentBallPrefab);
        if (ball == null)
        {
            Debug.LogWarning("공을 풀에서 가져올 수 없습니다!");
            return;
        }

        ball.transform.position = ShootPoint.position;

        // 발사체 설정
        ProjectileBase projectile = ball.GetComponent<ProjectileBase>();
        projectile.Speed = weaponStats.projectileSpeed;
        projectile.Direction = dir;                      // WeaponBase에서 계산된 방향 사용
        projectile.Damage = damage;                      // WeaponBase 필드 사용
        projectile.IsCriticalDamageProj = isCriticalDamage;
        projectile.KnockBackChance = knockback;          // WeaponBase 필드 사용
        projectile.TimeToLive = 1.5f;                    // 필요시 조정

        // ✨ 투사체에 무기 이름 전달
        projectile.WeaponName = weaponData.DisplayName;
    }

    protected override void FlipWeaponTools()
    {
        if (weaponTools == null) return;

        if (flip)
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}