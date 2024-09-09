using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeToLive = 3f;
    float hitRange;
    int damage;
    Vector3 dir;
    bool initDone;

    [Header("Feedback")]
    [SerializeField] GameObject hitEffectPrefab;

    /// <summary>
    /// _dir ���ڸ� Vector3.zero �� ������ ĳ���͸� ���� ���ư�.
    /// </summary>
    public virtual void Init(int _damage, Vector3 _dir)
    {
        if(_dir == Vector3.zero)
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
            DieProjectile();
        }
    }
    void ApplyMovement()
    {
        transform.position += speed * Time.deltaTime * dir;
    }
    void CastDamage()
    {
        if (Time.frameCount % 2 != 0) // 2�����ӿ� �� �� �浹 üũ
            return;

        float sqrDist = (GameManager.instance.player.transform.position - transform.position).sqrMagnitude;
        if (sqrDist < hitRange)
        {
            GameManager.instance.character.TakeDamage(damage, EnemyType.Projectile);
            DieProjectile();
        }
    }
    
    void DieProjectile()
    {
        GameObject hitEffect =  GameManager.instance.poolManager.GetMisc(hitEffectPrefab);
        if(hitEffect != null )
        {
            hitEffect.transform.position = transform.position;
        }

        timeToLive = 3f;
        transform.localScale = new Vector3(1, 1, 1);

        initDone = false;

        gameObject.SetActive(false);
    }
}