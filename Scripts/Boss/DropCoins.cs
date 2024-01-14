using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCoins : MonoBehaviour
{
    [SerializeField] int bulletsAmount;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float dropForce = 10f; 
    float startAngle = 0f, endAngle = 360f;
    Vector2 bulletMoveDirection;
    Vector3 initialPosition;

    public void Init(int numberOfCoins, Vector2 pos)
    {
        bulletsAmount = numberOfCoins;
        initialPosition = pos;
        StartCoroutine(FireCo());
    }
    void Fire()
    {
        float angleStep = (endAngle - startAngle) / bulletsAmount;
        float angle = startAngle;

        for (int i = 0; i < bulletsAmount; i++)
        {
            float bulDirX = initialPosition.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float bulDirY = initialPosition.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
            Vector2 bulDir = (bulMoveVector - initialPosition).normalized;

            //GameObject bullet = Instantiate(bulletPrefab, initialPosition, Quaternion.identity);
            GameObject bullet = GameManager.instance.poolManager.GetMisc(bulletPrefab);
            //bullet.GetComponent<ShadowHeight>().Initialize(bulDir * dropForce, 50f);

            angle += angleStep;
        }
    }
    IEnumerator FireCo()
    {
        yield return new WaitForSeconds(.2f);
        Fire();
    }
}
