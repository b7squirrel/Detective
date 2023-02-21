using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.LightningBolt;

public class LightningWeapon : WeaponBase
{
    [SerializeField] GameObject lightning;
    [SerializeField] float duration = .4f;
    List<Vector2> targets; //번개를 내릴 지점들
    [SerializeField] bool isClean;

    [Header("Sound")]
    [SerializeField] AudioClip shoot, strike;

    [Header("Effects")]
    [SerializeField] Transform strikeEffect;

    Vector2 startPosition, endPosition;

    [SerializeField] GameObject testCircle;

    protected override void Attack()
    {
        FindLandingPositions();

        if (isClean)
        {
            isClean = false;
            return;
        }

        Transform effect = Instantiate(strikeEffect, ShootPoint.position, Quaternion.identity);
        effect.transform.SetParent(ShootPoint);


        for (int i = 0; i < targets.Count; i++)
        {
            endPosition = targets[i];
            GameObject bolt = Instantiate(lightning);
            LightningBoltScript boltScript = bolt.GetComponent<LightningBoltScript>();
            boltScript.StartObject.transform.parent = ShootPoint;
            boltScript.StartObject.transform.position = ShootPoint.position;
            boltScript.EndObject.transform.position = endPosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);

            SoundManager.instance.Play(strike);
        }
        targets.Clear();
    }

    void ApplyDamage(Collider2D[] colliders)
    {
        foreach (var item in colliders)
        {
            Idamageable enemy = item.transform.GetComponent<Idamageable>();
            GameObject enemyObject = item.gameObject;

            if (enemy != null && enemyObject.activeSelf)
            {
                PostMessage(weaponStats.damage, item.transform.position);
                enemy.TakeDamage(weaponStats.damage, Wielder.knockBackChance);
            }
        }
    }

    void FindLandingPositions()
    {
        if (targets == null)
            targets = new List<Vector2>();

        Vector2 center = GameManager.instance.player.transform.position;

        Collider2D[] enemies =
                Physics2D.OverlapAreaAll(center - new Vector2(halfWidth * .8f, halfHeight * .8f),
                                            center + new Vector2(halfWidth * .8f, halfHeight * .8f), enemy);

        if (enemies.Length == 0)
        {
            isClean = true;
            return;
        }

        List<Vector2> candidates = new List<Vector2>();

        for (int i = 0; i < enemies.Length; i++)
        {
            candidates.Add((Vector2)enemies[i].transform.position);
        }

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2 pick = candidates[index];
            targets.Add(pick);
        }
    }
}
