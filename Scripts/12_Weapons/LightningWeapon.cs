using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningWeapon : WeaponBase
{
    [SerializeField] GameObject lightning;
    [SerializeField] GameObject lightningSynergy;
    [SerializeField] float duration = 0.4f;

    List<Vector2> targets;
    [SerializeField] bool isClean;

    [Header("Synergy")]
    [SerializeField] float synergyInterval = 0.1f;

    [Header("Sound")]
    [SerializeField] AudioClip shoot, strike;

    [Header("Effects")]
    [SerializeField] Transform strikeEffect;
    [SerializeField] GameObject sparkEffect;

    Vector2 startPosition, endPosition;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    HitEffects hitEffects;

    // ✅ NonAlloc용 버퍼
    static readonly Collider2D[] lightningHitBuffer = new Collider2D[20];
    static readonly Collider2D[] areaBuffer = new Collider2D[50];

    // ✅ FindLandingPositions에서 매번 new List 방지
    readonly List<Collider2D> candidatesBuffer = new List<Collider2D>(20);
    readonly List<Collider2D> recurringPoolBuffer = new List<Collider2D>(20);

    // ✅ SecondaryAttack에 전달할 타겟 목록 재사용
    readonly List<Vector2> secondShootPointBuffer = new List<Vector2>(10);

    protected override void Awake()
    {
        base.Awake();
        hitEffects = GetComponent<HitEffects>(); // ✅ 캐싱
    }

    protected override void Attack()
    {
        base.Attack();
        FindLandingPositions();

        if (isClean)
        {
            isClean = false;
            return;
        }

        Transform effect = Instantiate(strikeEffect, ShootPoint.position, Quaternion.identity);
        effect.SetParent(ShootPoint);
        SoundManager.instance.PlaySoundWith(strike, 1f, true, 0);

        for (int i = 0; i < targets.Count; i++)
        {
            endPosition = targets[i];

            // ✅ NonAlloc으로 GC 방지
            int count = Physics2D.OverlapCircleNonAlloc(endPosition, weaponStats.sizeOfArea, lightningHitBuffer);
            ApplyDamage(count);

            GameObject bolt = GameManager.instance.poolManager.GetMisc(lightning);
            if (bolt != null)
            {
                LightningBolt boltScript = bolt.GetComponent<LightningBolt>();
                if (boltScript != null)
                {
                    boltScript.SetDamage(damage);
                    boltScript.Activate(ShootPoint, endPosition, duration);
                }
            }
        }

        if (!isSynergyWeaponActivated) return;
        if (isClean) { isClean = false; return; }

        // ✅ new List 대신 버퍼 재사용
        secondShootPointBuffer.Clear();
        secondShootPointBuffer.AddRange(targets);
        StartCoroutine(SecondaryAttack(secondShootPointBuffer));
    }

    IEnumerator SecondaryAttack(List<Vector2> _secondShootPoint)
    {
        yield return new WaitForSeconds(synergyInterval);

        FindLandingPositions();
        SoundManager.instance.PlaySoundWith(strike, 1f, true, 0);

        for (int i = 0; i < _secondShootPoint.Count; i++)
        {
            if (targets.Count == 0) continue;

            int targetIndex = Random.Range(0, targets.Count);
            endPosition = targets[targetIndex];

            // ✅ NonAlloc으로 GC 방지
            int count = Physics2D.OverlapCircleNonAlloc(endPosition, weaponStats.sizeOfArea, lightningHitBuffer);
            ApplyDamage(count);

            GameObject bolt = GameManager.instance.poolManager.GetMisc(lightningSynergy);
            if (bolt != null)
            {
                LightningBolt boltScript = bolt.GetComponent<LightningBolt>();
                if (boltScript != null)
                {
                    boltScript.SetDamage(damage);
                    boltScript.Activate(_secondShootPoint[i], endPosition, duration - synergyInterval);
                }
            }
        }
    }

    void ApplyDamage(int count)
    {
        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        for (int i = 0; i < count; i++)
        {
            Idamageable enemy = lightningHitBuffer[i].GetComponent<Idamageable>();
            if (enemy == null) continue;
            if (!lightningHitBuffer[i].gameObject.activeSelf) continue;

            PostMessage(damage, lightningHitBuffer[i].transform.position);

            enemy.TakeDamage(damage, knockback, knockbackSpeedFactor,
                Player.instance.transform.position, hitEffect);

            DamageTracker.instance.RecordDamage(weaponData.DisplayName, damage);

            GameObject spark = GameManager.instance.poolManager.GetMisc(sparkEffect);
            if (spark != null)
                spark.transform.position = lightningHitBuffer[i].transform.position;
        }
    }

    void FindLandingPositions()
    {
        if (targets == null)
            targets = new List<Vector2>();

        targets.Clear();

        Vector2 center = GameManager.instance.player.transform.position;

        // ✅ OverlapAreaNonAlloc으로 GC 방지
        int count = Physics2D.OverlapAreaNonAlloc(
            center - new Vector2(halfWidth * 0.8f, halfHeight * 0.8f),
            center + new Vector2(halfWidth * 0.8f, halfHeight * 0.8f),
            areaBuffer,
            enemy);

        if (count == 0)
        {
            isClean = true;
            return;
        }

        // ✅ 버퍼 재사용 (new List 제거)
        candidatesBuffer.Clear();
        for (int i = 0; i < count; i++)
        {
            if (areaBuffer[i].GetComponent<Idamageable>() != null)
                candidatesBuffer.Add(areaBuffer[i]);
        }

        if (candidatesBuffer.Count == 0)
        {
            isClean = true;
            return;
        }

        recurringPoolBuffer.Clear();
        recurringPoolBuffer.AddRange(candidatesBuffer);

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            Collider2D pick;

            if (candidatesBuffer.Count == 0)
            {
                pick = recurringPoolBuffer[Random.Range(0, recurringPoolBuffer.Count)];
            }
            else
            {
                int idx = Random.Range(0, candidatesBuffer.Count);
                pick = candidatesBuffer[idx];
                candidatesBuffer.RemoveAt(idx);
            }

            EnemyBase enemyBase = pick.GetComponent<EnemyBase>();
            Vector2 targetPoint = enemyBase != null
                ? enemyBase.GetRandomBodyPoint()
                : (Vector2)pick.transform.position;

            targets.Add(targetPoint);
        }
    }
}