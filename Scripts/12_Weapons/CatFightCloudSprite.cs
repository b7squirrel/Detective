using UnityEngine;
using System.Collections;

public class CatFightCloudSprite : MonoBehaviour
{
    Animator anim;
    float animationLength;
    float randomPositionRadius; // ✅ SerializeField 제거, 동적으로 받음
    bool isActive = false;
    Coroutine smokeCo;

    void Awake()
    {
        anim = GetComponent<Animator>();
        
        if (anim == null)
        {
            Debug.LogError("CatFightCloudSprite: Animator component not found!");
        }
    }

    // ✅ radius 파라미터 추가
    public void StartSmoke(float delay, float radius)
    {
        randomPositionRadius = radius; // SizeOfArea 저장
        
        if (smokeCo != null)
            StopCoroutine(smokeCo);
            
        smokeCo = StartCoroutine(SmokeCo(delay));
    }

    IEnumerator SmokeCo(float delay)
    {
        // 랜덤 offset 대기
        yield return new WaitForSeconds(delay);
        
        isActive = true;
        
        // 애니메이션 길이 감지 (한 번만)
        yield return null; // 1프레임 대기 (Animator 준비)
        GetAnimationLength();
        
        // 첫 번째 랜덤 위치로 이동
        MoveToRandomPosition();
        
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
            
            // 새 랜덤 위치로 이동
            if (isActive) // 아직 활성화 상태면
            {
                MoveToRandomPosition();
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
            Debug.Log($"CatFightCloudSprite: Animation length detected = {animationLength}s");
        }
        else
        {
            // fallback
            animationLength = 14f / 60f;
            Debug.LogWarning("CatFightCloudSprite: Could not detect animation length, using default");
        }
    }

    void MoveToRandomPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * randomPositionRadius;
        transform.localPosition = randomOffset;
    }

    // 부모(CatFightCloud)가 호출
    public void StopSmoke()
    {
        isActive = false;
        
        if (smokeCo != null)
        {
            StopCoroutine(smokeCo);
            smokeCo = null;
        }
    }

    void OnDisable()
    {
        StopSmoke();
    }
}