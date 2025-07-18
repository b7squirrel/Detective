using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFireProjectile : MonoBehaviour
{
    public float speed = 8f;                     // 이동 속도
    public float lifeTime = 10f;                 // 미사일 생존 시간

    private Rigidbody2D rb;
    private Transform target;
    int damage;
    Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int damage, float speed, Vector2 dir)
    {
        // 플레이어 타겟 설정
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.transform;
        }

        // 데미지 설정
        this.damage = damage;
        this.direction = dir;

        // 랜덤한 방향으로 초기 속도 설정
        this.speed = speed;
        float randomAngle = Random.Range(0f, 360f);
        Vector2 initialDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        rb.velocity = initialDir * this.speed;

        // 일정 시간 후 미사일 제거
        Invoke(nameof(Deactivate), lifeTime);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Character character = other.GetComponent<Character>();

            if (character != null)
            {
                // 데미지 주기
                character.TakeDamage(damage, EnemyType.Melee);
            }
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
