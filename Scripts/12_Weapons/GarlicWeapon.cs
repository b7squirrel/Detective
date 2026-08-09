using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    ParticleSystem note;
    float effectRadius;
    [SerializeField] float[] effectArea = new float[4];

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;

    // ✅ NonAlloc용 버퍼
    readonly Collider2D[] garlicHitBuffer = new Collider2D[30];

    protected override void Awake()
    {
        base.Awake();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱
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
            Logger.LogWarning($"[GarlicWeapon] 기본값 사용 - equippedItem: {(equippedItem == null ? "null" : "있음")}, projectilePrefab: {(equippedItem?.projectilePrefab == null ? "null" : "있음")}");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            effectArea[(int)weaponStats.sizeOfArea],
            garlicHitBuffer);

        effectRadius = weaponStats.sizeOfArea;

        note.GetComponent<Animator>().SetTrigger((weaponStats.sizeOfArea).ToString());
        note.Play();

        ApplyDamage(count);
    }

    private void ApplyDamage(int count)
    {
        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        for (int i = 0; i < count; i++)
        {
            Idamageable enemy = garlicHitBuffer[i].GetComponent<Idamageable>();
            if (enemy == null) continue;

            PostMessage(damage, garlicHitBuffer[i].transform.position);

            Vector2 enemyDir = garlicHitBuffer[i].transform.position - transform.position;
            Vector2 offsetDir = -enemyDir.normalized;
            Vector2 hitPoint = (Vector2)garlicHitBuffer[i].transform.position + offsetDir * 2f;

            enemy.TakeDamage(damage, knockback, knockbackSpeedFactor, hitPoint, hitEffect);

            DamageTracker.instance.RecordDamage(weaponData.DisplayName, damage);
        }
    }

    protected override void FlipWeaponTools()
    {
        // Garlic은 뒤집기 불필요
    }
}