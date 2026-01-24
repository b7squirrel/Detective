using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWeapon : WeaponBase
{
    [SerializeField] GameObject planePrefab;
    [SerializeField] GameObject debugDot;
    [SerializeField] Transform target;  // 타겟의 위치
    [SerializeField] float speed = 10f; // 미사일의 이동 속도
    [SerializeField] float rotateSpeed = 200f; // 회전 속도

    [SerializeField] float radius = 10f;
    [SerializeField] float targetAngle;
    [SerializeField] float targetSpeed;

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }
    IEnumerator AttackCo()
    {
        int n = weaponStats.numberOfAttacks;
        while(n > 0)
        {
            yield return null;
            GenProjectile();
            n--;
        }
    }

    void GenProjectile()
    {
        GameObject plane = GameManager.instance.poolManager.GetMisc(planePrefab);
        plane.transform.position = transform.position;
        //Vector3 target = transform.position - new Vector3(10f, 0, 0);
        //Instantiate(debugDot, GetTargetPos(), Quaternion.identity);

        // 가장 가까운 적을 향해 발사
        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(1);
        if (closestEnemyPosition == null) return;
        if (closestEnemyPosition[0] == Vector2.zero)
        {
            return;
        }

        PlaneProjectile planeProj = plane.GetComponent<PlaneProjectile>();
        planeProj.Init(closestEnemyPosition[0], damage);

        // ✨ 투사체에 무기 이름 전달
        planeProj.WeaponName = weaponData.DisplayName;
    }

    Vector2 GetTargetPos()
    {
        // 각도를 시간에 따라 업데이트합니다.
        targetAngle += targetSpeed * Time.deltaTime;

        // 각도에 따른 x, y 좌표를 계산합니다.
        float x = transform.position.x + Mathf.Cos(targetAngle) * radius;
        float y = transform.position.y + Mathf.Sin(targetAngle) * radius;

        // 점의 위치를 업데이트합니다.
        return new Vector2(x, y);
    }
}