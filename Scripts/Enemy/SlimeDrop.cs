using System.Collections;
using UnityEngine;

public class SlimeDrop : MonoBehaviour
{
    [SerializeField] float life;
    [SerializeField] GameObject mainBody; // 사라질 때 파괴할 오브젝트
    [SerializeField] SpriteRenderer srBody, srBodyCore;
    [SerializeField] float fadeDuration;
    float lifeCounter;
    SlimeDropManager slimeDropManager;
    Animator anim;
    #region OnTrigger
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
            StartCoroutine(DieCo());
        }
    }
    IEnumerator DieCo()
    {
        float elapsed = 0f;
        Color originalColorBody = srBody.color;
        Color originalColorBodyCore = srBodyCore.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            srBody.color = new Color(originalColorBody.r, originalColorBody.g, originalColorBody.b, alpha);
            srBodyCore.color = new Color(originalColorBodyCore.r, originalColorBodyCore.g, originalColorBodyCore.b, alpha);
            yield return null;
        }

        // 최종적으로 완전히 사라졌다면 비활성화하거나 제거할 수 있음
        srBody.color = new Color(originalColorBody.r, originalColorBody.g, originalColorBody.b, 0f);
            srBodyCore.color = new Color(originalColorBodyCore.r, originalColorBodyCore.g, originalColorBodyCore.b, 0f);
        Destroy(mainBody);
    }
    #endregion
}
