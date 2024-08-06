using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWeapon : WeaponBase
{
    [SerializeField] GameObject planePrefab;
    [SerializeField] GameObject debugDot;
    [SerializeField] Transform target;  // Ÿ���� ��ġ
    [SerializeField] float speed = 10f; // �̻����� �̵� �ӵ�
    [SerializeField] float rotateSpeed = 200f; // ȸ�� �ӵ�

    protected override void Attack()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            Debug.Log("Shhot");
            StartCoroutine(AttackCo());
        }
    }
    IEnumerator AttackCo()
    {
        int n = weaponStats.numberOfAttacks;
        while(n > 0)
        {
            GenProjectile();
            n--;
            yield return new WaitForSeconds(.2f);
        }
    }

    void GenProjectile()
    {
        GameObject plane = GameManager.instance.poolManager.GetMisc(planePrefab);
        plane.transform.position = transform.position;
        Vector3 target = transform.position - new Vector3(10f, 0, 0);
        Instantiate(debugDot, target, Quaternion.identity);
        plane.GetComponent<PlaneProjectile>().Init(target);
    }
}
