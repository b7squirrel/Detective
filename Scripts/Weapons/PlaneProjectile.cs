using UnityEngine;

public class PlaneProjectile : ProjectileBase
{
    Vector3 target;
    [SerializeField] LayerMask targetLayer; // �����ؼ� ������ ���� enemy���� ������ enemy, prop �� �� ����
    [SerializeField] float speed = 10f; // �̻����� �̵� �ӵ�
    [SerializeField] float rotateSpeed = 600f; // ȸ�� �ӵ�
    Vector3 offsetDirection; // �ʱ� �ɼ� ����
    float sizeOfArea = 1f; // Ÿ�� ����

    TrailRenderer trailRenderer;
    int frameCount = 0;

    // �ٽ� Ȱ��ȭ �� �� Ʈ������ �̻��ϰ� ���۵Ǵ� ������ �ذ��ϱ� ���ؼ�
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
        // Ÿ���� ���� ���� ������ ����մϴ�.
        Vector3 directionToTarget = (target - transform.position).normalized;

        // ���� ���⿡�� Ÿ���� ���� �������� ȸ����ŵ�ϴ�.
        float rotateAmount = Vector3.Cross(transform.up, directionToTarget).z;
        transform.Rotate(0, 0, rotateAmount * rotateSpeed * Time.deltaTime);

        // �̻����� ������ŵ�ϴ�.
        transform.position += transform.up * speed * Time.deltaTime;

        // ��ǥ���� �����ϸ� ������ ���� ��Ȱ��ȭ
        if (Vector3.Distance(transform.position, target) < 1f)
        {
            CastDamage();
            DieProjectile();
        }
    }
    public void Init(Vector3 _taget)
    {
        // Ÿ�ٰ��� �ʱ� ���� �������� �����մϴ�.
        target = _taget;

        float randomAngle = UnityEngine.Random.Range(-70f, 70f);
        offsetDirection = Quaternion.Euler(0, 0, randomAngle) * (target - transform.position).normalized;
        //offsetDirection = (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, offsetDirection);
        transform.localScale = .5f * Vector3.one;
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
