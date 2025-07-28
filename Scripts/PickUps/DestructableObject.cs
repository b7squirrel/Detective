using UnityEngine;

/// <summary>
/// 일정 회수만큼 공격을 받으면 파괴된다.
/// 파괴 전까지 맞을 때마다 특정 아이템을 드롭
/// 파괴 되어도 특정 아이템 드롭
/// </summary>
public class DestructableObject : MonoBehaviour, Idamageable
{
    [SerializeField] int hp;
    [SerializeField] GameObject hitEffect;
    [SerializeField] GameObject destructableDieEffect;
    int currentHp;
    Animator anim;

    WallManager wallManager;

    void OnEnable()
    {
        currentHp = hp;
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (IsOutOfRange())
        {
            DestroyObjectWithoutDrop();
        }
    }

    public void TakeDamage(int damage, float knockBackChance, float knockBackSpeedFactor, Vector2 target, GameObject hitEffect)
    {
        // knockBackChance값을 받아오지만 쓰지는 않는다
        currentHp--;

        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        if (effect != null)
        {
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector2.one * 1.3f;
        }

        if (currentHp <= 0)
        {
            DestroyObject();
            return;
        }
        if(anim != null)
        {
            anim.SetTrigger("Hit");
        }
        DropItem();
    }

    void DropItem()
    {
        DropOnDestroy dropOnDestroy = GetComponent<DropOnDestroy>();
        
        GetComponent<DropOnDestroy>().CheckDrop();
    }

    void DestroyObject()
    {
        GameObject dieEffect = GameManager.instance.poolManager.GetMisc(destructableDieEffect);
        if (dieEffect == null) return; // 갯수 제한에 걸려서 더 이상 풀에서 꺼낼 수 없으면 이펙트 표시 안함
        dieEffect.transform.position = transform.position;
        //GetComponent<DropOnDestroy>().DropMultipleObjects();
        DropItem();
        gameObject.SetActive(false);
    }
    void DestroyObjectWithoutDrop()
    {
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DestroyObject();
        }       
    }
    bool IsOutOfRange()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();

        return new Equation().IsOutOfRange(transform.position, spawnConst);
    }
}
