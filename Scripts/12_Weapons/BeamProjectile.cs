using UnityEngine;

// 재활용해서 계속 사용하는 projectile의 경우 active여부를 weapon단계에서 관리하도록 한다
public class BeamProjectile : ProjectileBase
{
    [SerializeField] Transform endPoint;
    [SerializeField] LayerMask destructables;
    [SerializeField] LayerMask walls;
    [SerializeField] LayerMask screenEdges;
    [SerializeField] Transform hitWallEffect;
    Transform assignedMuzzlePoint;

    Transform startPoint;
    Animator anim;
    int frameCount = 7; // 몇 프레임 간격으로 데미지를 입힐지 정하는 변수
    bool isSynergyActivated;
    bool disableLaser;

    [SerializeField] LineRenderer laserLine;

    // ✅ LinecastNonAlloc용 static 버퍼
    static readonly RaycastHit2D[] linecastBuffer = new RaycastHit2D[20];

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱
    }

    void OnEnable()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (isSynergyActivated)
            anim.SetTrigger("Synergy");
    }

    private void OnDisable()
    {
        disableLaser = true;
        laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 0);
        laserLine.endColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 0);
        hitWallEffect.gameObject.SetActive(false);
    }

    protected override void DieProjectile()
    {
        gameObject.SetActive(false);
    }

    protected override void HitObject()
    {
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        CastRayToDestructables();
    }

    public void SetAnimToSynergy()
    {
        anim.SetTrigger("Synergy");
        frameCount = 4;
        isSynergyActivated = true;
    }

    void DrawLaser(Vector2 _endPos)
    {
        Vector2 startPos = assignedMuzzlePoint != null
            ? (Vector2)assignedMuzzlePoint.position
            : new Vector2(transform.position.x, transform.position.y + .5f);

        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, _endPos);
        hitWallEffect.position = _endPos;
        laserLine.widthMultiplier = Random.Range(.1f, .4f);
        hitWallEffect.gameObject.SetActive(true);
    }

    void CastRayToDestructables()
    {
        if (Time.timeScale == 0) return;

        Vector2 startPos = assignedMuzzlePoint != null
            ? (Vector2)assignedMuzzlePoint.position
            : new Vector2(transform.position.x, transform.position.y + .5f);

        LayerMask allLayers = destructables | walls | screenEdges;

        // ✅ LinecastNonAlloc으로 GC 방지
        int hitCount = Physics2D.LinecastNonAlloc(startPos, endPoint.position, linecastBuffer, allLayers);

        RaycastHit2D closestHit = new RaycastHit2D();
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            float distance = Vector2.Distance(startPos, linecastBuffer[i].point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHit = linecastBuffer[i];
            }
        }

        // 데미지 처리 (destructables만)
        if (Time.frameCount % frameCount == 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (((1 << linecastBuffer[i].collider.gameObject.layer) & destructables) != 0)
                    DealDamage(linecastBuffer[i]);
            }
        }

        // 레이저 그리기
        laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 1);
        laserLine.endColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 1);

        Vector2 laserEndPoint = closestHit.collider != null ? closestHit.point : (Vector2)endPoint.position;
        DrawLaser(laserEndPoint);
    }

    void DealDamage(RaycastHit2D _object)
    {
        if (_object == false) return;
        if (_object.transform.GetComponent<Idamageable>() == null) return;

        // 카메라 밖에 있으면 데미지 전달 하지 않음
        if (_object.transform.GetComponentInChildren<SpriteRenderer>().isVisible == false)
        {
            Logger.Log("out of the camera");
            return;
        }

        Transform enmey = _object.transform;
        PostMessage(Damage, enmey.position);

        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;
        if (hitEffect != null) hitEffect.transform.position = enmey.position;

        enmey.GetComponent<Idamageable>().TakeDamage(
            Damage, KnockBackChance, KnockBackSpeedFactor,
            _object.transform.position, hitEffect);

        if (!string.IsNullOrEmpty(WeaponName))
            DamageTracker.instance.RecordDamage(WeaponName, Damage);
    }

    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        assignedMuzzlePoint = muzzlePoint;
    }
}