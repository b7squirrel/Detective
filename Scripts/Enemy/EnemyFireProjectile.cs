using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFireProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 10f;
    [SerializeField] SlimeAttackType slimeAttackType;
    [SerializeField] AudioClip initSound;

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
        // ── 풀 재사용 시 이전 Invoke가 남아있으면 조기 비활성화되는 버그 방지 ──
        CancelInvoke();

        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.transform;

        this.damage = damage;
        this.direction = dir;
        this.speed = speed;

        // 초기 속도는 랜덤 방향 (FixedUpdate에서 direction으로 즉시 덮어씌워짐)
        float randomAngle = Random.Range(0f, 360f);
        Vector2 initialDir = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
        rb.velocity = initialDir * this.speed;

        if (initSound != null) SoundManager.instance.Play(initSound);

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
                if (slimeAttackType == SlimeAttackType.None) slimeAttackType = SlimeAttackType.Slime;
                character.TakeDamage(damage, EnemyType.Melee, slimeAttackType);
            }

            // 플레이어에 맞으면 즉시 회수
            Deactivate();
        }
    }

    void Deactivate()
    {
        CancelInvoke(); // 중복 Deactivate 방지
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}