using System.Collections;
using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    ParticleSystem note;
    float effectRadius;
    [SerializeField] float[] effectArea = new float[4];

    HitEffects hitEffects;

    readonly Collider2D[] garlicHitBuffer = new Collider2D[30];

    // ✅ 범위 표시용 - 오브젝트와 그 안의 스프라이트를 분리해서 참조
    [SerializeField] Transform damageIndicator;        // On/Off, Scale 대상
    [SerializeField] SpriteRenderer damageIndicatorSprite; // 원본 크기 계산용 (damageIndicator의 자식)
    [SerializeField] float indicatorDuration = 0.12f;

    float nativeDiameter;
    WaitForSeconds cachedWait;
    Coroutine indicatorCoroutine;

    protected override void Awake()
    {
        base.Awake();
        hitEffects = GetComponent<HitEffects>();

        // ✅ 자식 스프라이트에서 원본 지름만 읽어옴 (부모 스케일과 무관)
        nativeDiameter = damageIndicatorSprite.sprite.bounds.size.x;
        cachedWait = new WaitForSeconds(indicatorDuration);
        damageIndicator.gameObject.SetActive(false);
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

        float radius = effectArea[(int)weaponStats.sizeOfArea];

        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            radius,
            garlicHitBuffer);

        effectRadius = weaponStats.sizeOfArea;

        note.GetComponent<Animator>().SetTrigger((weaponStats.sizeOfArea).ToString());
        note.Play();

        ShowRangeIndicator(radius);

        ApplyDamage(count);
    }

    // ✅ damageIndicator(부모 오브젝트)를 스케일/On-Off
    private void ShowRangeIndicator(float radius)
    {
        float scaleFactor = (radius * 2f) / nativeDiameter;
        damageIndicator.localScale = Vector3.one * scaleFactor;

        if (indicatorCoroutine != null)
            StopCoroutine(indicatorCoroutine);

        indicatorCoroutine = StartCoroutine(FlashIndicator());
    }

    private IEnumerator FlashIndicator()
    {
        damageIndicator.gameObject.SetActive(true);
        yield return cachedWait;
        damageIndicator.gameObject.SetActive(false);
    }

    private void ApplyDamage(int count)
    {
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