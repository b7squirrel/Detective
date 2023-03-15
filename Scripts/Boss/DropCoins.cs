using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCoins : MonoBehaviour
{
    [SerializeField] int bulletsAmount;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float dropForce; 
    float startAngle = 0f, endAngle = 360f;
    Vector2 bulletMoveDirection;

    public void Init()
    {
        StartCoroutine(FireCo());
    }
    void Fire()
    {
        float angleStep = (endAngle - startAngle) / bulletsAmount;
        float angle = startAngle;

        for (int i = 0; i < bulletsAmount; i++)
        {
            float bulDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float bulDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
            Vector2 bulDir = (bulMoveVector - transform.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<ShadowHeight>().Initialize(bulDir * dropForce, 50f);

            angle += angleStep;
        }
    }
    IEnumerator FireCo()
    {
        yield return new WaitForSeconds(.2f);
        Fire();
    }
}
