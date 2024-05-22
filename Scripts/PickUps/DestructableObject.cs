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

        DropItem();
        GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
        if (effect != null)
        {
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector2.one * 1.3f;
        }

        if (currentHp <= 0)
        {
            DestroyObject();
        }
        if(anim != null)
        {
            anim.SetTrigger("Hit");
        }
    }

    void DropItem()
    {
        GetComponent<DropOnDestroy>().CheckDrop();
    }

    void DestroyObject()
    {
        GameObject dieEffect = GameManager.instance.poolManager.GetMisc(destructableDieEffect);
        dieEffect.transform.position = transform.position;
        GetComponent<DropOnDestroy>().DropMultipleObjects();
        gameObject.SetActive(false);
    }
    void DestroyObjectWithoutDrop()
    {
        gameObject.SetActive(false);
    }

    bool IsOutOfRange()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();

        return new Equation().IsOutOfRange(transform.position, spawnConst);
    }
}
