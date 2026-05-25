using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHomingProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float rotationSpeed = 200f;
    public float homingDelay = 1f;
    float lifeTime = 0f;
    [SerializeField] SlimeAttackType slimeAttackType;
    [SerializeField] AudioClip initSound;

    private Rigidbody2D rb;
    private Transform target;
    private float spawnTime;
    private bool isHomingActive = false;
    int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int damage, float lifeTime, float homingDelay)
    {
        // ── 풀 재사용 시 이전 Invoke/상태 초기화 ──
        CancelInvoke();
        isHomingActive = false;

        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.transform;

        this.damage = damage;
        this.homingDelay = homingDelay;
        this.lifeTime = lifeTime;

        spawnTime = Time.time;

        float randomAngle = Random.Range(0f, 360f);
        Vector2 initialDir = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
        rb.velocity = initialDir * speed;

        if (initSound != null) SoundManager.instance.Play(initSound);

        Invoke(nameof(Deactivate), this.lifeTime);
    }

    // 초기 방향을 지정할 수 있는 오버로드
    public void Initialize(int damage, float lifeTime, Vector2 initialDirection)
    {
        // ── 풀 재사용 시 이전 Invoke/상태 초기화 ──
        CancelInvoke();
        isHomingActive = false;

        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.transform;

        this.damage = damage;
        this.lifeTime = lifeTime;

        spawnTime = Time.time;

        rb.velocity = initialDirection.normalized * speed;

        if (initSound != null) SoundManager.instance.Play(initSound);

        Invoke(nameof(Deactivate), this.lifeTime);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (!isHomingActive && Time.time - spawnTime >= homingDelay)
        {
            isHomingActive = true;
            Debug.Log("호밍 활성화!");
        }

        if (isHomingActive)
        {
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            float angle = Vector2.SignedAngle(rb.velocity, direction);

            float maxRotate = rotationSpeed * Time.fixedDeltaTime;
            angle = Mathf.Clamp(angle, -maxRotate, maxRotate);

            Vector2 newDir = Quaternion.Euler(0, 0, angle) * rb.velocity.normalized;
            rb.velocity = newDir * speed;
        }
        else
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
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

    void OnDrawGizmos()
    {
        Gizmos.color = isHomingActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}