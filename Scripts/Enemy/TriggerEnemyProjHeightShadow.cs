using UnityEngine;

public class TriggerEnemyProjHeightShadow : MonoBehaviour
{
    public void Init()
    {
        ShadowHeightProjectile shadowHeightProj = GetComponent<ShadowHeightProjectile>();

        if (GameManager.instance == null || GameManager.instance.player == null)
        {
            Debug.LogError("GameManager 또는 Player가 없습니다!");
            return;
        }

        Logger.Log($"[Init] 진입 시 transform.position(자식)={transform.position}, transform.localPosition={transform.localPosition}, parent={transform.parent?.name}");

        // ⭐ 부모(루트, 적 위치로 설정된 오브젝트)에 아직 붙어있는 상태에서 리셋
        // → 부모 기준 0,0,0 = 정확히 적의 현재 위치로 스냅됨
        transform.localPosition = Vector3.zero;

        Transform originalParent = transform.parent;
        transform.SetParent(null);

        Vector2 targetPosition = GameManager.instance.player.transform.position;
        Vector2 startPosition = transform.position;

        Logger.Log($"[Init] startPosition={startPosition} / targetPosition={targetPosition}");

        float horizontalVelociy = GetHorizontalVelocity(startPosition, targetPosition);
        Vector2 direction = (targetPosition - startPosition).normalized;

        Logger.Log($"[Init] horizontalVelocity={horizontalVelociy} / direction={direction}");


        Vector2 groundVelocity;
        float verticalVel;

        groundVelocity = horizontalVelociy * direction;
        verticalVel = 50f;

        shadowHeightProj.Initialize(groundVelocity, verticalVel);

        Logger.Log($"[Init] Initialize 호출 후 transform.position={transform.position}");

        transform.SetParent(originalParent);

        Logger.Log($"[Init] 재부모화 후 transform.position={transform.position} / localPosition={transform.localPosition}");
    }

    public float GetHorizontalVelocity(Vector2 startPos, Vector2 targetPos)
    {
        // 거리의 제곱을 사용한 간단한 공식
        float distanceSquared = Vector2.SqrMagnitude(targetPos - startPos);
        float horizontalVelocity = (distanceSquared / 25f) + 10f;

        // 음수 방지 (거리가 음수일 경우)
        return Mathf.Max(0f, horizontalVelocity);
    }

    // public void InitProjectile(float horizontalVelocity, float verticalVelocity)
    // {
    //     ShadowHeightProjectile shadowHeightProj = GetComponent<ShadowHeightProjectile>();

    //     if (GameManager.instance == null || GameManager.instance.player == null)
    //     {
    //         Debug.LogError("GameManager 또는 Player가 없습니다!");
    //         return;
    //     }
    //     Transform originalParent = transform.parent;
    //     transform.SetParent(null);

    //     Vector2 targetPosition = GameManager.instance.player.transform.position;
    //     Vector2 startPosition = transform.position;
    //     Vector2 direction = (targetPosition - startPosition).normalized;

    //     Vector2 groundVelocity;
    //     float verticalVel;

    //     groundVelocity = horizontalVelocity * direction;
    //             verticalVel = verticalVelocity;

    //     shadowHeightProj.Initialize(groundVelocity, verticalVel);
    //     transform.SetParent(originalParent);
    // }

    #region 디버그 기즈모
    void OnDrawGizmos()
    {
        if (GameManager.instance == null || GameManager.instance.player == null) return;
        
        Vector2 startPos = transform.position;
        Vector2 targetPos = GameManager.instance.player.transform.position;
        
        // 시작점에서 목표점까지 선 그리기
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, targetPos);
        
        // 목표점 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        
        // 포물선 궤도 미리보기 (에디터에서만)
        if (Application.isPlaying) return;
    }
    #endregion
}