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
    int currentHp;
    Animator anim;

    void OnEnable()
    {
        currentHp = hp;
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    public void TakeDamage(int damage, float knockBackChance, Vector2 target, GameObject hitEffect)
    {
        // knockBackChance값을 받아오지만 쓰지는 않는다
        currentHp--;
        if (currentHp <= 0)
        {
            DestroyObject();
        }
        if(anim != null)
        {
            anim.SetTrigger("Hit");
            GameObject effect = GameManager.instance.poolManager.GetMisc(hitEffect);
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector2.one * 1.2f;
        }
        DropItem();
    }

    void DropItem()
    {
        GetComponent<DropOnDestroy>().CheckDrop();
    }

    void DestroyObject()
    {
        gameObject.SetActive(false);
        GetComponent<DropOnDestroy>().DropLastItem();
    }
}
