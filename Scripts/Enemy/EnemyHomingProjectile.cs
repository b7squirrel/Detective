using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHomingProjectile : MonoBehaviour
{
    public float speed = 8f;                     // 이동 속도
    public float rotationSpeed = 200f;           // 회전 속도 (도/초)
    public float homingDelay = 1f;               // 호밍 시작 지연 시간
    float lifeTime = 0f;                         // 미사일 생존 시간
    [SerializeField] SlimeAttackType slimeAttackType;
    [SerializeField] AudioClip initSound; // 투사체 생성 사운드


    private Rigidbody2D rb;
    private Transform target;
    private float spawnTime;                     // 생성된 시간
    private bool isHomingActive = false;         // 호밍 활성화 여부
    int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int damage, float lifeTime, float homingDelay)
    {
        // 플레이어 타겟 설정
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.transform;
        }

        // 데미지 설정
        this.damage = damage;

        // 생성 시간 기록
        spawnTime = Time.time;

        // 랜덤한 방향으로 초기 속도 설정
        float randomAngle = Random.Range(0f, 360f);
        Vector2 initialDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        rb.velocity = initialDir * speed;

        // 호밍 딜레이 설정
        this.homingDelay = homingDelay;

        // 사운드 재생
        if(initSound != null) SoundManager.instance.Play(initSound);

        // 일정 시간 후 미사일 제거
        this.lifeTime = lifeTime;
        Invoke(nameof(Deactivate), this.lifeTime);
    }

    // 초기 방향을 지정할 수 있는 오버로드
    public void Initialize(int damage, float lifeTime, Vector2 initialDirection)
    {
        // 플레이어 타겟 설정
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.transform;
        }

        // 데미지 설정
        this.damage = damage;

        // 생성 시간 기록
        spawnTime = Time.time;

        // 지정된 방향으로 초기 속도 설정
        rb.velocity = initialDirection.normalized * speed;

        // 일정 시간 후 미사일 제거
        this.lifeTime = lifeTime;
        Invoke(nameof(Deactivate), this.lifeTime);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // 호밍 활성화 시간 체크
        if (!isHomingActive && Time.time - spawnTime >= homingDelay)
        {
            isHomingActive = true;
            Debug.Log("호밍 활성화!");
        }

        // 호밍이 활성화된 경우에만 추적
        if (isHomingActive)
        {
            // 현재 방향
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            float angle = Vector2.SignedAngle(rb.velocity, direction);

            // 회전량 제한
            float maxRotate = rotationSpeed * Time.fixedDeltaTime;
            angle = Mathf.Clamp(angle, -maxRotate, maxRotate);

            // 회전 적용
            Vector2 newDir = Quaternion.Euler(0, 0, angle) * rb.velocity.normalized;
            rb.velocity = newDir * speed;
        }
        // 호밍이 비활성화된 경우 초기 방향으로 계속 이동
        else
        {
            // 속도 크기 유지 (속도가 줄어들지 않도록)
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
                // 데미지 주기. slime attack type이 실수로 선택되지 않았다면 슬라임으로 설정
                if (slimeAttackType == SlimeAttackType.None) slimeAttackType = SlimeAttackType.Slime;
                character.TakeDamage(damage, EnemyType.Melee, slimeAttackType);
            }
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        // 디버그용 - 호밍 활성화 상태를 색상으로 표시
        if (isHomingActive)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}