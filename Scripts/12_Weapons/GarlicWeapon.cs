using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    ParticleSystem note;
    float effectRadius;
    [SerializeField] float[] effectArea = new float[4];

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            if (note != null) note.gameObject.SetActive(false);
            GameObject newNoteObj = Instantiate(equippedItem.projectilePrefab, transform);
            newNoteObj.transform.localPosition = Vector3.zero;
            note = newNoteObj.GetComponent<ParticleSystem>();
        }
        else
        {
            // ⭐ 어느 쪽이 null인지 구분
            Logger.LogWarning($"[GarlicWeapon] 기본값 사용 - equippedItem: {(equippedItem == null ? "null" : "있음")}, projectilePrefab: {(equippedItem?.projectilePrefab == null ? "null" : "있음")}");
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
