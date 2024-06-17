using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeToLive = 3f;
    int damage;
    Vector3 dir;
    bool initDone;
    Character character;

    [Header("Feedback")]
    [SerializeField] GameObject hitEffectPrefab;

    public void Init(int _damage)
    {
        dir = (GameManager.instance.player.transform.position - transform.position).normalized;
        damage = _damage;
        initDone = true;
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
        //if (Time.frameCount % 2 != 0) // 2프레임에 한 번 충돌 체크
        //    return;

        float sqrDist = (GameManager.instance.player.transform.position - transform.position).sqrMagnitude;
        if( sqrDist < .5f )
        {
            if (character == null) character = GameManager.instance.character;
            character.TakeDamage(damage, EnemyType.Ranged);
            Debug.Log("projectile Damage = " + damage);
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