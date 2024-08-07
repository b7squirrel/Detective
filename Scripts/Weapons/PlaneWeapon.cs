using System.Collections;
using UnityEngine;

public class PlaneWeapon : WeaponBase
{
    [SerializeField] GameObject planePrefab;
    [SerializeField] GameObject debugDot;
    [SerializeField] Transform target;  // Ÿ���� ��ġ
    [SerializeField] float speed = 10f; // �̻����� �̵� �ӵ�
    [SerializeField] float rotateSpeed = 200f; // ȸ�� �ӵ�

    [SerializeField] float radius = 10f;
    [SerializeField] float targetAngle;
    [SerializeField] float targetSpeed;

    protected override void Attack()
    {
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
        plane.GetComponent<PlaneProjectile>().Init(GetTargetPos());
    }

    Vector2 GetTargetPos()
    {
        // ������ �ð��� ���� ������Ʈ�մϴ�.
        targetAngle += targetSpeed * Time.deltaTime;

        // ������ ���� x, y ��ǥ�� ����մϴ�.
        float x = transform.position.x + Mathf.Cos(targetAngle) * radius;
        float y = transform.position.y + Mathf.Sin(targetAngle) * radius;

        // ���� ��ġ�� ������Ʈ�մϴ�.
        return new Vector2(x, y);
    }
}
