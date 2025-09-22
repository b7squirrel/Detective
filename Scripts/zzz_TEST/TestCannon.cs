using UnityEngine;

public class TestCannon : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject cannonPrefab;
    [SerializeField] float coolDown; // 쿨다운 타임
    [SerializeField] int damage;
    float lastFireTime; // 마지막 발사 시간

    [Header("Debug")]
    [SerializeField] float horizontalVelocity;
    [SerializeField] float verticalVelocity;
    // 쿨다운이 지나면 캐논 오브젝트를 생성해서 플레이어를 향해 발사
    void Update()
    {
        // 쿨다운이 지났는지 확인
        if (Time.time >= lastFireTime + coolDown)
        {
            FireCannon();
            lastFireTime = Time.time;
        }
    }

    void FireCannon()
    {
        if (player == null) player = FindObjectOfType<Player>().transform;
        GameObject cannonBall = Instantiate(cannonPrefab, transform.position, Quaternion.identity);
        cannonBall.GetComponentInChildren<IEnemyProjectile>().InitProjectileDamage(damage);
        Debug.LogError($"데미지 = {damage}");
    }
}
