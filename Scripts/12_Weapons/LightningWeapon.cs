using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningWeapon : WeaponBase
{
    [SerializeField] GameObject lightning;          // LightningBolt 프리팹 (PoolingKey 부착)
    [SerializeField] GameObject lightningSynergy;   // 시너지용 프리팹
    [SerializeField] float duration = 0.4f;

    List<Vector2> targets;
    [SerializeField] bool isClean;

    [Header("Synergy")]
    [SerializeField] float synergyInterval = 0.1f; // 번개 사이 시간차

    [Header("Sound")]
    [SerializeField] AudioClip shoot, strike;

    [Header("Effects")]
    [SerializeField] Transform strikeEffect;
    [SerializeField] GameObject sparkEffect; // 스파크 프리팹

    Vector2 startPosition, endPosition;

    // ─────────────────────────────────────────
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

            // 데미지 적용
            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);


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

        List<Vector2> secondShootPoint = new List<Vector2>(targets);

        // 시너지
        if (!isSynergyWeaponActivated) return;

        if (isClean)
        {
            isClean = false;
            return;
        }

        StartCoroutine(SecondaryAttack(secondShootPoint));
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

            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);

            GameObject bolt = GameManager.instance.poolManager.GetMisc(lightningSynergy);
            if (bolt != null)
            {
                LightningBolt boltScript = bolt.GetComponent<LightningBolt>();
                if (boltScript != null)
                {
                    boltScript.SetDamage(damage);
                    boltScript.Activate(_secondShootPoint[i], endPosition, duration);
                }
            }
        }
    }
    // ─────────────────────────────────────────
    void ApplyDamage(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Idamageable enemy = colliders[i].transform.GetComponent<Idamageable>();
            GameObject enemyObject = colliders[i].gameObject;

            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(damage, colliders[i].transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enemy.TakeDamage(damage,
                                 knockback,
                                 knockbackSpeedFactor,
                                 Player.instance.transform.position,
                                 hitEffect);

                DamageTracker.instance.RecordDamage(weaponData.DisplayName, damage);

                // ⚡ 스파크 이펙트 스폰
                GameObject spark = GameManager.instance.poolManager.GetMisc(sparkEffect);
                if (spark != null)
                {
                    spark.transform.position = colliders[i].transform.position;
                }
            }
        }
    }

    void FindLandingPositions()
    {
        if (targets == null)
            targets = new List<Vector2>();

        targets.Clear(); // ✅ 항상 초기화

        Vector2 center = GameManager.instance.player.transform.position;

        Collider2D[] enemies =
            Physics2D.OverlapAreaAll(
                center - new Vector2(halfWidth * 0.8f, halfHeight * 0.8f),
                center + new Vector2(halfWidth * 0.8f, halfHeight * 0.8f),
                enemy);

        if (enemies.Length == 0)
        {
            isClean = true;
            return;
        }

        // ✅ Vector2 대신 Collider2D로 저장
        List<Collider2D> candidates = new List<Collider2D>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].GetComponent<Idamageable>() == null) continue;
            candidates.Add(enemies[i]);
        }

        if (candidates.Count == 0)
        {
            isClean = true;
            return;
        }

        // ✅ recurringPool도 Collider2D로
        List<Collider2D> recurringPool = new List<Collider2D>(candidates);

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            Collider2D pick;

            if (candidates.Count == 0)
            {
                pick = recurringPool[Random.Range(0, recurringPool.Count)];
            }
            else
            {
                pick = candidates[Random.Range(0, candidates.Count)];
                candidates.Remove(pick);
            }

            // ✅ 타겟을 꺼낼 때마다 새로 랜덤 포인트 계산 → 분산 효과
            EnemyBase enemyBase = pick.GetComponent<EnemyBase>();
            Vector2 targetPoint = enemyBase != null
                ? enemyBase.GetRandomBodyPoint()
                : (Vector2)pick.transform.position;

            targets.Add(targetPoint);
        }
    }
}