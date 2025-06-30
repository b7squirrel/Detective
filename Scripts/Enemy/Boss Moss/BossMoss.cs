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
        // shooter에서 player를 바라보는 방향 벡터 계산
        Vector2 directionToPlayer =
            (GameManager.instance.player.transform.position - transform.position).normalized;

        // 발사체 궤도를 랜덤하게 하기 위해 -30도에서 +30도 사이의 랜덤 각도 생성
        float randomAngle = Random.Range(-30f, 30f);

        // directionToPlayer 벡터를 Z축 기준으로 randomAngle만큼 회전하는 Quaternion 생성
        Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

        // shooter에서 player를 바라보는 방향에 randomRotation 적용
        Vector2 randomDirection = randomRotation * directionToPlayer;

        return randomDirection;
    }
    #endregion
}
