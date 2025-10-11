using UnityEngine;

public class EnemyBouncingLaserProjectile : MonoBehaviour
{
    Rigidbody2D rb;
    float speed;
    float lifeTime;
    Vector2 moveDirection;
    bool isInitialized;
    int damage;

    [SerializeField] LayerMask reflectLayerMask; // 반사될 수 있는 레이어 마스크

    [Header("사운드")]
    [SerializeField] AudioClip[] electricBallSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 투사체를 초기화하고 지정된 방향과 속도로 발사
    /// </summary>
    public void Initialize(Vector2 direction, float speed, int damage, float duration)
    {
        this.moveDirection = direction.normalized;
        this.speed = speed;
        this.lifeTime = duration;
        this.damage = damage;
        isInitialized = true;

        rb.velocity = moveDirection * speed;

        // 사운드
        RegisterSound();

        Invoke(nameof(Deactivate), lifeTime);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;
        rb.velocity = moveDirection * speed;
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
        // 지정된 레이어에 포함된 오브젝트에만 반사
            if ((reflectLayerMask.value & (1 << other.gameObject.layer)) != 0)
            {
                Vector2 contactPoint = other.ClosestPoint(transform.position);
                Vector2 normal = (transform.position - (Vector3)contactPoint).normalized;

                // 벽의 표면 방향으로 반사
                moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
                rb.velocity = moveDirection * speed;
            }
    }

    void RegisterSound()
    {
        // 전기 루프 사운드
        foreach (var item in electricBallSound)
        {
            GameManager.instance.loopSoundManager.RegisterAudio(item);
        }
    }
    void UnregisterSound()
    {
        // 전기 루프 사운드 정지
        foreach (var item in electricBallSound)
        {
            GameManager.instance.loopSoundManager.UnregisterAudio(item);
        }
    }

    void Deactivate()
    {
        isInitialized = false;

        UnregisterSound();
        
        gameObject.SetActive(false);
    }
}