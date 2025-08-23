using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 주어진 시간동안 날아가는 적 투사체
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeToLive = 3f;
    float hitRange;
    int damage;
    Vector3 dir;
    bool initDone;
    public UnityEvent onDoneEvent; // 목표물에 도달하면 실행할 이벤트들

    [Header("Feedback")]
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] GameObject dropPrefab; // 프로젝타일이 사라지고 떨어트릴 오브젝트

    /// <summary>
    /// 투사체를 초기화합니다. _dir이 Vector3.zero면 플레이어를 향해 자동 조준
    /// </summary>
    public virtual void Init(int _damage, Vector3 _dir)
    {
        if (_dir == Vector3.zero)
        {
            dir = (GameManager.instance.player.transform.position - transform.position).normalized;
        }
        else
        {
            dir = _dir;
        }

        damage = _damage;
        initDone = true;

        hitRange = GetComponentInChildren<SpriteRenderer>().transform.localScale.x;

    }
    void Update()
    {
        if (initDone == false) return;
        if (Time.timeScale == 0)
            return;
        ApplyMovement();
        AttackCoolTimer();
        CastDamage();
    }
    void AttackCoolTimer()
    {
        timeToLive -= Time.deltaTime;
        if (timeToLive < 0f)
        {
            DieProjectile(); // 프로젝타일의 역할이 끝남. 그냥 사라지도록
        }
    }
    void ApplyMovement()
    {
        transform.position += speed * Time.deltaTime * dir;
    }
    void CastDamage()
    {
        if (Time.frameCount % 2 != 0) // 홀수 프레임 스킵
            return;

        float sqrDist = ((Vector2)GameManager.instance.player.transform.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDist < hitRange * hitRange)
        {
            GameManager.instance.character.TakeDamage(damage, EnemyType.Projectile);
            OnProjectileDone(); // 프로젝타일의 역할이 끝남. 유니티 이벤트 실행
        }
    }

    void DieProjectile()
    {
        timeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);

        initDone = false;

        gameObject.SetActive(false);
    }

    void OnProjectileDone()
    {
        onDoneEvent?.Invoke();
        DieProjectile(); // 공통적으로 들어가야 하니 함수를 따로 빼지 않고 여기에 넣어 버리자
    }

    // 유니티 이벤트에서 실행할 함수
    public void Event_GenerateHitEffect()
    {
        if (hitEffectPrefab == null) return;
        GameObject hitEffect = GameManager.instance.poolManager.GetMisc(hitEffectPrefab);

        if (hitEffect != null)
        {
            hitEffect.transform.position = transform.position;
        }
    }
    public void Event_GenerateDrop()
    {
        if (dropPrefab == null) return;
        GameObject dropObject = GameManager.instance.poolManager.GetMisc(dropPrefab);

        if (dropObject != null)
        {
            dropObject.transform.position = transform.position;
        }
    }
}