using System.Collections;
using UnityEngine;

// 레이저 동작을 담당하는 클래스
public class LaserBehavior : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float rotationSpeed;
    private float lifetime;
    private Vector3 direction;
    private Rigidbody rb;
    private bool isRotating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 중력 비활성화
        rb.useGravity = false;
    }

    public void Initialize(Vector3 target, float laserSpeed, float rotSpeed, float life)
    {
        targetPosition = target;
        speed = laserSpeed;
        rotationSpeed = rotSpeed;
        lifetime = life;

        // 타겟 방향 계산
        direction = (targetPosition - transform.position).normalized;

        // 타겟 방향으로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 레이저 발사
        rb.velocity = direction * speed;

        // 일정 시간 후 60도 회전 시작
        StartCoroutine(StartRotationAfterDelay(0.5f));

        // 생존 시간 후 파괴
        Destroy(gameObject, lifetime);
    }

    IEnumerator StartRotationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isRotating = true;
    }

    void Update()
    {
        if (isRotating)
        {
            // Y축 기준으로 회전
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌 시 데미지 처리 (필요에 따라 구현)
        if (other.CompareTag("Player"))
        {
            // 플레이어 데미지 처리 로직
            Debug.Log("플레이어가 레이저에 맞았습니다!");

            // 레이저 파괴
            Destroy(gameObject);
        }

        // 벽이나 다른 장애물과 충돌 시 파괴
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}