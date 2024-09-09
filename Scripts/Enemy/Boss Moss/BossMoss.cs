using System.Collections;
using UnityEngine;

public class BossMoss : BossBase
{
    #region Variables
    [Header("Boss Moth")]
    [SerializeField] GameObject powderPrefabs;
    [SerializeField] int projectileNums;
    [SerializeField] int powderDamage;
    #endregion
    #region Shoot
    public override void ShootMultiProjectiles()
    {
        StartCoroutine(ShootCo());
    }
    IEnumerator ShootCo()
    {
        int shootCounter = projectileNums;
        while (shootCounter > 0)
        {
            GameObject powder = GameManager.instance.poolManager.GetMisc(powderPrefabs);
            powder.transform.position = ShootPoint.position;
            powder.GetComponent<EnemyProjectile>().Init(powderDamage, GetProjectileDir());
            shootCounter--;
            yield return new WaitForSeconds(.1f);
        }
    }
    Vector2 GetProjectileDir()
    {
        // shooter�� player�� �ٶ󺸴� ���� ���� ���
        Vector2 directionToPlayer =
                 (GameManager.instance.player.transform.position - transform.position).normalized;

        // ������ ������ ����ϱ� ���� +15���� -15�� ���̿��� ������ ���� ����
        float randomAngle = Random.Range(-30f, 30f);

        // directionToPlayer ���͸� �������� randomAngle��ŭ ȸ���ϴ� Quaternion ����
        Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

        // shooter�� player�� �ٶ󺸴� ���⿡ randomRotation ����
        Vector2 randomDirection = randomRotation * directionToPlayer;

        return randomDirection;
    }
    #endregion
}
