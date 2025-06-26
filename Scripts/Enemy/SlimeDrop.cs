using UnityEngine;

public class SlimeDrop : MonoBehaviour
{
    SlimeDropManager slimeDropManager;
    Animator anim;
    void OnTriggerEnter2D(Collider2D collision)
    {
        CachingReferences();
        if (anim.speed == 0) return;// 스톱워치로 멈춘 상태라면 

        if (collision.CompareTag("Player"))
            slimeDropManager.EnterSlime();
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        CachingReferences();
        if (anim.speed == 0) return;// 스톱워치로 멈춘 상태라면 

        if (collision.CompareTag("Player"))
            slimeDropManager.ExitSlime();
    }
    void CachingReferences()
    {
        if (slimeDropManager == null) slimeDropManager = FindObjectOfType<SlimeDropManager>();
        if (anim == null) anim = GetComponentInParent<Animator>();
    }
}
