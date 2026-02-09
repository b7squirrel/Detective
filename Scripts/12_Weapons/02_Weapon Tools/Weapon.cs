using UnityEngine;

/// <summary>
/// 무기의 시각적 표현 및 발사/이펙트 위치 로케이터
/// - ShootPoint: 발사체가 시작되는 위치 (오리의 눈 부위 등)
/// - EffectPoint: Muzzle flash 등 이펙트가 표시될 위치
/// - IsDirectional: 무기가 적을 향해 회전하는지 여부 (true: 총/활, false: 펀치/주변 무기)
/// WeaponManager에서 WeaponBase에 자동 할당되어 오리의 신체 부위에 부착됨
/// </summary>
public class Weapon : MonoBehaviour
{
    public Transform shootPoint;
    public Transform effectPoint;
    [SerializeField] SpriteRenderer weaponSprite;
    [field: SerializeField] public bool IsDirectional { get; private set; }

    public SpriteRenderer GetWeaponSprite()
    {
        if (weaponSprite == null) return null;

        return weaponSprite;
    }
}
