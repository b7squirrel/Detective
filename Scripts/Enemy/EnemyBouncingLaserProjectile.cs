using UnityEngine;

public class EnemyBouncingLaserProjectile : MonoBehaviour
{
    Rigidbody2D rb;
    float speed;
    float lifeTime;
    Vector2 moveDirection;
    bool isInitialized;

    [SerializeField] LayerMask reflectLayerMask; // 반사될 수 있는 레이어 마스크

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 투사체를 초기화하고 지정된 방향과 속도로 발사
    /// </summary>
    /// <param name="direction">정규화된 방향</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="duration">활성화 시간</param>
    public void Initialize(Vector2 direction, float speed, int damage, float duration)
    {
        this.moveDirection = direction.normalized;
        this.speed = speed;
        this.lifeTime = duration;
        isInitialized = true;

        rb.velocity = moveDirection * speed;

        Invoke(nameof(Deactivate), lifeTime);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;
        rb.velocity = moveDirection * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
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

    void Deactivate()
    {
        isInitialized = false;
        gameObject.SetActive(false);
    }
}