using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    ParticleSystem note;
    float effectRadius;
    [SerializeField] float[] effectArea = new float[4];

    protected override void Awake()
    {
        base.Awake();
        note = GetComponentInChildren<ParticleSystem>();
        
    }
    
    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);

        if (InitialWeapon)
        {
            Item equippedItem = GetEssentialEquippedItem();

            if (equippedItem != null && equippedItem.projectilePrefab != null)
            {
                // 기존 기본 파티클 비활성화
                if (note != null)
                    note.gameObject.SetActive(false);

                // 장착 아이템의 파티클 프리팹을 자식으로 생성
                GameObject newNoteObj = Instantiate(equippedItem.projectilePrefab, transform);
                newNoteObj.transform.localPosition = Vector3.zero;
                note = newNoteObj.GetComponent<ParticleSystem>();

                Logger.Log($"[GarlicWeapon] 리드 오리 - 장착 파티클 사용: {equippedItem.Name}");
            }
            else
            {
                Logger.LogWarning("[GarlicWeapon] 리드 오리 - 장착된 파티클이 없어서 기본값 사용");
            }
        }
        else
        {
            Logger.Log("[GarlicWeapon] 동료 오리 - 기본 파티클 사용");
        }
    }
    
    protected override void Attack()
    {
        base.Attack();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, effectArea[(int)weaponStats.sizeOfArea]);

        effectRadius = weaponStats.sizeOfArea;

        note.GetComponent<Animator>().SetTrigger((weaponStats.sizeOfArea).ToString());
        note.Play();

        ApplyDamage(colliders);
    }

    private void ApplyDamage(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Idamageable enemy = colliders[i].transform.GetComponent<Idamageable>();

            if (enemy != null)
            {
                PostMessage(damage, colliders[i].transform.position);

                Vector2 enemyDir = colliders[i].transform.position - transform.position;
                Vector2 offsetDir = -(enemyDir.normalized);
                Vector2 hitPoint = (Vector2)colliders[i].transform.position + (offsetDir * 2f); // 대략 적 콜라이더의 반정도

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enemy.TakeDamage(damage, knockback, knockbackSpeedFactor, hitPoint, hitEffect);

                // ✨ 데미지 기록 추가
                // ✨ weaponData.DisplayName 사용
                DamageTracker.instance.RecordDamage(weaponData.DisplayName, damage);
            }
        }
    }

    void SetNoteParticle()
    {
        
    }

    protected override void FlipWeaponTools()
    {
        // Debug.Log("Garlic");
    }
}
