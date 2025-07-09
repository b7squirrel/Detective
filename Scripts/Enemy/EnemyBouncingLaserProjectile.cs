using System.Collections;
using UnityEngine;

public class EnemyBouncingLaserProjectile : MonoBehaviour
{
    [SerializeField] LineRenderer laserLine;
    [SerializeField] Transform hitEffect;
    [SerializeField] float laserLength = 3f;    // 레이저 길이
    [SerializeField] float laserWidth = 0.3f;   // 레이저 두께
    
    private Vector3 direction;
    private float speed;
    private float damage;
    private int maxBounces;
    private int currentBounces = 0;
    private float lifetime;
    private LayerMask playerLayer;
    private LayerMask wallLayer;
    private Rigidbody2D rb;
    
    // 레이저 끝점 추적용
    private Vector3 laserEndPoint;
    
    void Start()
    {
        // Rigidbody2D 설정
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        
        // Collider 설정 (트리거로)
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.2f;
        
        // LineRenderer 설정
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
            if (laserLine == null)
            {
                laserLine = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        SetupLineRenderer();
        
        // 히트 이펙트 설정
        SetupHitEffect();
    }
    
    void SetupLineRenderer()
    {
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.material.color = Color.cyan;  // 반사 레이저는 청록색
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.positionCount = 2;
        laserLine.sortingOrder = 10;
    }
    
    void SetupHitEffect()
    {
        if (hitEffect == null)
        {
            GameObject hitEffectObj = new GameObject("HitEffect");
            hitEffect = hitEffectObj.transform;
            hitEffect.parent = transform;
            
            // 간단한 히트 이펙트
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            circle.transform.parent = hitEffect;
            circle.transform.localPosition = Vector3.zero;
            circle.transform.localScale = Vector3.one * 0.3f;
            circle.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(circle.GetComponent<Collider>());
        }
        hitEffect.gameObject.SetActive(false);
    }
    
    public void Initialize(Vector3 _direction, float _speed, float _damage, int _maxBounces, float _lifetime, LayerMask _playerLayer, LayerMask _wallLayer)
    {
        direction = _direction.normalized;
        speed = _speed;
        damage = _damage;
        maxBounces = _maxBounces;
        lifetime = _lifetime;
        playerLayer = _playerLayer;
        wallLayer = _wallLayer;
        currentBounces = 0;
        
        // 초기 속도 설정
        rb.velocity = direction * speed;
        
        // 생존 시간 후 파괴
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (Time.timeScale == 0) return;
        
        UpdateLaserVisual();
    }
    
    void UpdateLaserVisual()
    {
        // 레이저 시작점 (현재 위치)
        Vector3 startPos = transform.position;
        
        // 레이저 끝점 (이동 방향으로 일정 거리)
        laserEndPoint = startPos + direction * laserLength;
        
        // 레이저 라인 그리기
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, laserEndPoint);
        
        // 히트 이펙트 위치 업데이트
        if (hitEffect != null)
        {
            hitEffect.position = laserEndPoint;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            DealDamageToPlayer(other);
            ReflectLaser(other);
        }
        // 벽과 충돌
        else if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            ReflectLaser(other);
        }
    }
    
    void DealDamageToPlayer(Collider2D player)
    {
        // var damageable = player.GetComponent<IDamageable>();
        // if (damageable != null)
        // {
        //     damageable.TakeDamage(damage, 0, 0, transform.position, null);
        //     Debug.Log($"반사 레이저가 플레이어에게 {damage} 데미지!");
            
        //     // 히트 이펙트 표시
        //     ShowHitEffect();
        // }
    }
    
    void ReflectLaser(Collider2D hitObject)
    {
        // 최대 반사 횟수 체크
        if (currentBounces >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }
        
        // 충돌 지점과 법선 계산
        Vector2 hitPoint = hitObject.ClosestPoint(transform.position);
        Vector2 hitNormal = (Vector2)(transform.position - hitObject.transform.position).normalized;
        
        // 벽의 경우 더 정확한 법선 계산
        if (((1 << hitObject.gameObject.layer) & wallLayer) != 0)
        {
            // 벽의 가장 가까운 점에서 법선 계산
            Collider2D wallCollider = hitObject;
            Vector2 closestPoint = wallCollider.ClosestPoint(transform.position);
            hitNormal = ((Vector2)transform.position - closestPoint).normalized;
        }
        
        // 반사 벡터 계산
        Vector2 reflectedDirection = Vector2.Reflect(direction, hitNormal);
        direction = reflectedDirection.normalized;
        
        // 새로운 속도 적용
        rb.velocity = direction * speed;
        
        // 반사 횟수 증가
        currentBounces++;
        
        // 반사될 때마다 색상 변경 (시각적 피드백)
        float colorIntensity = 1f - (currentBounces / (float)maxBounces * 0.5f);
        laserLine.material.color = new Color(0, colorIntensity, colorIntensity, 1f);
        
        // 히트 이펙트 표시
        ShowHitEffect();
        
        Debug.Log($"레이저 반사! ({currentBounces}/{maxBounces})");
    }
    
    void ShowHitEffect()
    {
        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(true);
            StartCoroutine(HideHitEffectAfterDelay(0.2f));
        }
    }
    
    IEnumerator HideHitEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(false);
        }
    }
}
