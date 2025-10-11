using UnityEngine;

public class PlaneProjectile : ProjectileBase
{
    Vector3 target;
    [SerializeField] LayerMask targetLayer; // 조준해서 던지는 것은 enemy지만 터지면 enemy, prop 둘 다 공격
    [SerializeField] float speed = 10f; // 미사일의 이동 속도
    [SerializeField] float rotateSpeed = 600f; // 회전 속도
    Vector3 offsetDirection; // 초기 옵셋 방향
    float sizeOfArea = 1f; // 타격 범위

    TrailRenderer trailRenderer;
    int frameCount = 0;

    // 다시 활성화 될 때 트레일이 이상하게 시작되는 문제를 해결하기 위해서
    private void OnDisable()
    {
        if(trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.Clear();
    }
    protected override void Update()
    {
        if (Time.timeScale == 0f) return;
        AttackCoolTimer();
        ApplyMovement();

        frameCount++;
        if(frameCount > 30) frameCount = 0;

        if(frameCount % 8 == 0)
        {
            CastDamage();
        }
    }
    protected override void ApplyMovement()
    {
        // // 타겟을 향한 현재 방향을 계산합니다.
        // Vector3 directionToTarget = (target - transform.position).normalized;

        // // 현재 방향에서 타겟을 향한 방향으로 회전시킵니다.
        // float rotateAmount = Vector3.Cross(transform.up, directionToTarget).z;
        // transform.Rotate(0, 0, rotateAmount * rotateSpeed * Time.deltaTime);

        // // 미사일을 전진시킵니다.
        // transform.position += transform.up * speed * Time.deltaTime;

        // // 목표물에 도착하면 데미지 띄우고 비활성화
        // if (Vector3.Distance(transform.position, target) < 1f)
        // {
        //     CastDamage();
        //     DieProjectile();
        // }
        // 타겟을 향한 현재 방향을 계산합니다.
        Vector3 directionToTarget = (target - transform.position).normalized;

        // 현재 방향 벡터 (transform.up)
        Vector3 currentDirection = transform.up;

        // 두 벡터 사이의 각도를 계산
        float angle = Vector3.SignedAngle(currentDirection, directionToTarget, Vector3.forward);

        // 회전량을 제한하여 부드러운 회전
        float maxRotationThisFrame = rotateSpeed * Time.deltaTime;
        float rotationAmount = Mathf.Clamp(angle, -maxRotationThisFrame, maxRotationThisFrame);

        // 회전 적용
        transform.Rotate(0, 0, rotationAmount);

        // 미사일을 전진시킵니다.
        transform.position += transform.up * speed * Time.deltaTime;

        // 목표물에 도착하면 데미지 띄우고 비활성화
        if (Vector3.Distance(transform.position, target) < 1f)
        {
            CastDamage();
            DieProjectile();
        }
    }
    public void Init(Vector3 _taget, int damage)
    {
        // 타겟과의 초기 각도 오프셋을 설정합니다.
        target = _taget;

        float randomAngle = UnityEngine.Random.Range(-70f, 70f);
        offsetDirection = Quaternion.Euler(0, 0, randomAngle) * (target - transform.position).normalized;
        //offsetDirection = (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, offsetDirection);
        transform.localScale = .5f * Vector3.one;
        // Debug.LogError($"비행기의 부모 = {transform.parent.name}");

        Damage = damage;
    }

    protected override void AttackCoolTimer()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
        {
            DieProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CastDamage();
    }

    protected override void CastDamage()
    {
        if (Time.frameCount % 10 != 0) return; // 10프레임 간격으로 데미지를 입도록

        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, sizeOfArea, targetLayer);
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enmey = hit[i].GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                if (enmey.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enmey.transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage,
                                                             KnockBackChance,
                                                             KnockBackSpeedFactor,
                                                             transform.position,
                                                             hitEffect);
                hitDetected = true;
            }
        }
    }
}