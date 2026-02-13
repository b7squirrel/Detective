using UnityEngine;
using System.Collections;

public class CatClawSlash : MonoBehaviour
{
    Animator anim;
    float animationLength;
    float randomPositionRadius;
    bool isActive = false;
    Coroutine slashCo;

    void Awake()
    {
        anim = GetComponent<Animator>();
        
        if (anim == null)
        {
            Debug.LogError("CatClawSlash: Animator component not found!");
        }
    }

    // 부모(CatFightCloud)가 호출
    public void StartSlash(float delay, float radius)
    {
        randomPositionRadius = radius;
        
        if (slashCo != null)
            StopCoroutine(slashCo);
            
        slashCo = StartCoroutine(SlashCo(delay));
    }

    IEnumerator SlashCo(float delay)
    {
        // 랜덤 offset 대기
        yield return new WaitForSeconds(delay);
        
        isActive = true;
        
        // 애니메이션 길이 감지 (한 번만)
        yield return null; // 1프레임 대기 (Animator 준비)
        GetAnimationLength();
        
        // 첫 번째 랜덤 위치 + 회전
        MoveToRandomPositionAndRotation();
        
        // 애니메이션 재생 루프
        while (isActive)
        {
            // 애니메이션 트리거
            if (anim != null)
            {
                anim.SetTrigger("Play");
            }
            
            // 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(animationLength);
            
            // 새 랜덤 위치 + 회전
            if (isActive)
            {
                MoveToRandomPositionAndRotation();
            }
        }
    }

    void GetAnimationLength()
    {
        if (anim == null) return;
        
        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        
        if (clipInfo.Length > 0)
        {
            animationLength = clipInfo[0].clip.length;
        }
        else
        {
            // fallback
            animationLength = 0.5f;
            Debug.LogWarning("CatClawSlash: Could not detect animation length, using default");
        }
    }

    // ✅ 위치 + 회전 모두 랜덤
    void MoveToRandomPositionAndRotation()
    {
        // 랜덤 위치
        Vector2 randomOffset = Random.insideUnitCircle * randomPositionRadius;
        transform.localPosition = randomOffset;
        
        // ✅ 랜덤 회전 (0~360도)
        float randomRotation = Random.Range(0f, 360f);
        transform.localRotation = Quaternion.Euler(0, 0, randomRotation);
    }

    // 부모(CatFightCloud)가 호출
    public void StopSlash()
    {
        isActive = false;
        
        if (slashCo != null)
        {
            StopCoroutine(slashCo);
            slashCo = null;
        }
    }

    void OnDisable()
    {
        StopSlash();
    }
}