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

    [Header("Sound")]
    [SerializeField] AudioClip shoot, strike;

    [Header("Effects")]
    [SerializeField] Transform strikeEffect;

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

        for (int i = 0; i < targets.Count; i++)
        {
            endPosition = targets[i];

            // 데미지 적용
            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);
            SoundManager.instance.Play(strike);

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
        targets.Clear();

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
        yield return null;

        FindLandingPositions();

        for (int i = 0; i < _secondShootPoint.Count; i++)
        {
            if (targets.Count == 0) continue;

            int targetIndex = Random.Range(0, targets.Count);
            endPosition = targets[targetIndex];

            // 데미지 적용
            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);
            SoundManager.instance.PlaySoundWith(strike, 1f, true, 0);

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
            }
        }
    }

    void FindLandingPositions()
    {
        if (targets == null)
            targets = new List<Vector2>();

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

        List<Vector2> candidates = new List<Vector2>();

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyBase enemyBase = enemies[i].GetComponent<EnemyBase>();
            Vector2 targetPoint = enemyBase != null
                ? enemyBase.GetRandomBodyPoint()
                : (Vector2)enemies[i].transform.position;

            candidates.Add(targetPoint);
        }

        List<Vector2> recurringPool = new List<Vector2>(candidates);

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            if (candidates.Count == 0)
            {
                int recurringPoolIndex = Random.Range(0, recurringPool.Count);
                targets.Add(recurringPool[recurringPoolIndex]);
            }
            else
            {
                int index = Random.Range(0, candidates.Count);
                Vector2 pick = candidates[index];
                targets.Add(pick);
                candidates.Remove(pick);
            }
        }
    }
}