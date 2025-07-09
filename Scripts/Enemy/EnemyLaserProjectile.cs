using UnityEngine;

// 적의 레이저 프로젝타일 클래스
public class EnemyLaserProjectile : MonoBehaviour
{
    [SerializeField] LineRenderer laserLine;
    [SerializeField] Transform hitEffect;
    [SerializeField] float maxDistance = 50f;
    [SerializeField] float laserWidth = 1f; // 레이저 두께

    private float damage;
    private LayerMask destructables;
    private LayerMask walls;
    private float duration;
    private int frameCount = 5; // 데미지 처리 간격

    void Start()
    {
        // LineRenderer 설정
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
            if (laserLine == null)
            {
                laserLine = gameObject.AddComponent<LineRenderer>();
            }
        }

        // LineRenderer 기본 설정
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        // laserLine.color = Color.red;
        laserLine.startWidth = laserWidth;  // 시작점 두께
        laserLine.endWidth = laserWidth;    // 끝점 두께
        laserLine.positionCount = 2;
        laserLine.sortingLayerName = "Effect"; // Sorting Layer를 Effect로 설정
        laserLine.sortingOrder = 10;

        // 히트 이펙트 설정
        if (hitEffect == null)
        {
            GameObject hitEffectObj = new GameObject("HitEffect");
            hitEffect = hitEffectObj.transform;
            hitEffect.parent = transform;

            // 간단한 히트 이펙트 (원형)
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            circle.transform.parent = hitEffect;
            circle.transform.localPosition = Vector3.zero;
            circle.transform.localScale = Vector3.one * 0.5f;
            circle.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(circle.GetComponent<Collider>());
        }
    }

    void OnEnable()
    {
        if (laserLine != null)
        {
            laserLine.enabled = true;
        }
    }

    void OnDisable()
    {
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(false);
        }
    }

    public void Initialize(float _damage, LayerMask _destructables, LayerMask _walls, float _duration, float _laserWidth = 1f)
    {
        damage = _damage;
        destructables = _destructables;
        walls = _walls;
        duration = _duration;
        laserWidth = _laserWidth;

        // LineRenderer 두께 업데이트
        if (laserLine != null)
        {
            laserLine.startWidth = laserWidth;
            laserLine.endWidth = laserWidth;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        CastLaser();
    }

    void CastLaser()
    {
        Vector2 startPos = transform.position;
        Vector2 direction = transform.right; // 로컬 X축 방향
        Vector2 endPos = startPos + direction * maxDistance;

        // 모든 레이어를 검사하여 가장 가까운 충돌점 찾기
        LayerMask allLayers = destructables | walls;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxDistance, allLayers);

        Vector2 laserEndPoint = hit.collider != null ? hit.point : endPos;

        // 레이저 라인 그리기
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, laserEndPoint);

        // 히트 이펙트 위치 설정
        if (hitEffect != null)
        {
            hitEffect.position = laserEndPoint;
            hitEffect.gameObject.SetActive(hit.collider != null);
        }

        // 데미지 처리 (일정 간격으로) - 두꺼운 레이저를 위해 여러 레이캐스트 사용
        if (Time.frameCount % frameCount == 0)
        {
            // 레이저 중심선과 양쪽 가장자리에서 레이캐스트 실행
            Vector2 perpendicular = new Vector2(-direction.y, direction.x); // 수직 벡터
            float halfWidth = laserWidth / 2f;

            // 중심, 위쪽, 아래쪽에서 레이캐스트
            Vector2[] rayPositions = {
                startPos, // 중심
                startPos + perpendicular * halfWidth, // 위쪽
                startPos - perpendicular * halfWidth, // 아래쪽
                startPos + perpendicular * halfWidth * 0.5f, // 중간 위
                startPos - perpendicular * halfWidth * 0.5f  // 중간 아래
            };

            foreach (Vector2 rayPos in rayPositions)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayPos, direction, maxDistance, destructables);
                foreach (var hitTarget in hits)
                {
                    DealDamage(hitTarget);
                }
            }
        }
    }

    void DealDamage(RaycastHit2D hitTarget)
    {
        // if (hitTarget.collider == null) return;

        // // IDamageable 인터페이스 확인
        // var damageable = hitTarget.transform.GetComponent<IDamageable>();
        // if (damageable == null) return;

        // // 화면 밖에 있으면 데미지 처리하지 않음
        // var spriteRenderer = hitTarget.transform.GetComponentInChildren<SpriteRenderer>();
        // if (spriteRenderer != null && !spriteRenderer.isVisible)
        // {
        //     return;
        // }

        // // 데미지 처리
        // damageable.TakeDamage(damage, 0, 0, hitTarget.point, null);

        // Debug.Log($"레이저가 {hitTarget.transform.name}에게 {damage} 데미지를 입혔습니다!");
    }
}