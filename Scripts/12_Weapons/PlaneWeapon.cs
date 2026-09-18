using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWeapon : WeaponBase
{
    [SerializeField] GameObject planePrefab;
    [SerializeField] GameObject debugDot;
    [SerializeField] Transform target;
    [SerializeField] float speed = 10f;
    [SerializeField] float rotateSpeed = 200f;
    [SerializeField] float radius = 10f;
    [SerializeField] float targetAngle;
    [SerializeField] float targetSpeed;

    [Header("시너지 - 흡혈")]
    [SerializeField][Range(0f, 1f)] float lifeStealRatio = 0.1f;
    [SerializeField][Range(0f, 100f)] float lifeStealChance = 30f;

    // GenProjectile에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        int n = weaponStats.numberOfAttacks;
        while (n > 0)
        {
            yield return null;
            GenProjectile();
            n--;
        }
    }

    public void OnProjectileHit(int damage)
    {
        if (!isSynergyWeaponActivated) return;

        // 확률 체크
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > lifeStealChance) return;

        int healAmount = Mathf.RoundToInt(damage * lifeStealRatio);
        if (healAmount <= 0) return;
        Wielder.Heal(healAmount, false);
        Wielder.ShowHealEffect();
    }

    void GenProjectile()
    {
        EnemyFinder.instance.GetEnemies(1, enemyQueryBuffer);
        if (enemyQueryBuffer.Count == 0 || enemyQueryBuffer[0] == Vector2.zero)
            return;

        // ✅ 적 위치 자체가 이미 오염됐다면 이번 발사는 스킵
        Vector2 enemyPos = enemyQueryBuffer[0];
        if (float.IsNaN(enemyPos.x) || float.IsNaN(enemyPos.y)
            || float.IsInfinity(enemyPos.x) || float.IsInfinity(enemyPos.y))
        {
            Debug.LogWarning($"[PlaneWeapon] 비정상적인 적 좌표 감지, 발사 스킵: {enemyPos}");
            return;
        }

        GameObject plane = GameManager.instance.poolManager.GetMisc(planePrefab);
        plane.transform.position = transform.position;

        PlaneProjectile planeProj = plane.GetComponent<PlaneProjectile>();
        planeProj.Init(enemyQueryBuffer[0], damage, this);
        planeProj.WeaponName = weaponData.DisplayName;
    }

    Vector2 GetTargetPos()
    {
        targetAngle += targetSpeed * Time.deltaTime;

        float x = transform.position.x + Mathf.Cos(targetAngle) * radius;
        float y = transform.position.y + Mathf.Sin(targetAngle) * radius;

        return new Vector2(x, y);
    }
}