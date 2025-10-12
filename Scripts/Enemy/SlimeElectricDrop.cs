using UnityEngine;

public class SlimeElectricDrop : MonoBehaviour
{
    [Header("Drop")]
    [SerializeField] float life;
    [SerializeField] GameObject mainBody; // 사라질 때 파괴할 오브젝트
    SlimeDropManager slimeDropManager;
    Animator anim;

    #region OnTrigger
    void OnTriggerEnter2D(Collider2D collision)
    {
        CachingReferences();

        if (collision.CompareTag("Player"))
            slimeDropManager?.EnterSlime();
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        CachingReferences();

        if (collision.CompareTag("Player"))
        {
            slimeDropManager?.ExitSlime();
        }
    }
    void CachingReferences()
    {
        if (slimeDropManager == null) slimeDropManager = FindObjectOfType<SlimeDropManager>();
        if (anim == null) anim = GetComponentInParent<Animator>();
    }
    #endregion

    #region 수명
    void Update()
    {
        Die();
    }
    void Die()
    {
        life -= Time.deltaTime;
        if (life <= 0)
        {
            Destroy(mainBody);
        }
    }
    #endregion
}
