using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.LightningBolt;

public class LightningWeapon : WeaponBase
{
    [SerializeField] GameObject lightning;
    [SerializeField] GameObject lightningSynergy;
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
        base.Attack();

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
            GameObject bolt = GameManager.instance.poolManager.GetMisc(lightning);
            LightningBoltScript boltScript = bolt.GetComponent<LightningBoltScript>();
            boltScript.StartObject.transform.parent = ShootPoint;
            boltScript.StartObject.transform.position = ShootPoint.position;
            boltScript.EndObject.transform.position = endPosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);

            SoundManager.instance.Play(strike);
            StartCoroutine(DisableBolt(boltScript));
        }

        List<Vector2> secondShootPoint = new List<Vector2>();
        secondShootPoint.AddRange(targets);
        targets.Clear();

        // 시너지 무기
        if(isSynergyWeaponActivated == false)
            return;

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

        FindLandingPositions(); // target 리스트 모으기

        for (int i = 0; i < _secondShootPoint.Count; i++)
        {
            int targetIndex = Random.Range(0, targets.Count);
            Debug.Log("target count = " + targets.Count + " targetIndex = " + targetIndex);
            if (targets.Count == 0) continue;
            endPosition = targets[targetIndex];
            GameObject bolt = GameManager.instance.poolManager.GetMisc(lightningSynergy);
            LightningBoltScript boltScript = bolt.GetComponent<LightningBoltScript>();
            boltScript.StartObject.transform.position = _secondShootPoint[i];
            boltScript.EndObject.transform.position = endPosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(endPosition, weaponStats.sizeOfArea);
            ApplyDamage(colliders);

            SoundManager.instance.Play(strike);
            StartCoroutine(DisableBolt(boltScript));
        }
    }

    // 볼트가 pool에서 나오면 pool에 들어가기 전 위치로 번개를 발사한다. 1프레임 정도 동안.
    IEnumerator DisableBolt(LightningBoltScript boltScript)
    {
        yield return new WaitForSeconds(duration);
        boltScript.StartObject.transform.position = Vector2.zero;
        boltScript.EndObject.transform.position = Vector2.zero;
        boltScript.gameObject.SetActive(false);
    }

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

        // 중복을 피하지만 화면에 적의 갯수가 부족하면 중복 허용
        List<Vector2> recurringPool = new List<Vector2>();
        recurringPool.AddRange(candidates);
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            if (candidates.Count == 0)
            {
                int recurringPoolIndex = Random.Range(0, recurringPool.Count);
                Vector2 recurringPick = recurringPool[recurringPoolIndex];
                targets.Add(recurringPick);
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

    void SplashAttack()
    {

    }
}
