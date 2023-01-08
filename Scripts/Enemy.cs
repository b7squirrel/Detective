using UnityEngine;

public class Enemy : MonoBehaviour, Idamageable
{
    [SerializeField] float speed;
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField] int experienceReward = 400;
    [SerializeField] RuntimeAnimatorController[] animCon;

    [SerializeField] Rigidbody2D target;

    bool isLive;

    int damage;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        health = maxHealth;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;
        Vector2 dirVec = target.position - rb.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;
        sr.flipX = target.position.x < rb.position.x;
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[Random.Range(0, data.spriteType + 1)];
        speed = data.speed;
        maxHealth = data.health;
        health = maxHealth;
        damage = data.damage;   
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (GameManager.instance.player == null)
            return;
        if (collision.gameObject == target.gameObject)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (target.gameObject == null)
            return;

        target.gameObject.GetComponent<Character>().TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 1)
        {
            //target.GetComponent<Level>().AddExperience(experienceReward);
            GetComponent<DropOnDestroy>().CheckDrop();
            gameObject.SetActive(false);
        }
    }
}
