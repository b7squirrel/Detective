using UnityEngine;

/// <summary>
/// 튀어오르는 물체의 방향을 랜덤하게 정해주는 클래스
/// </summary>
public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;

    // 랜덤 좌표 관련 변수
    [SerializeField] float radius;
   
    void OnEnable()
    {
        Vector2 randomVec = new GeneralFuctions().GetRandomPosInCircle(radius);
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(randomVec, verticalVel);
    }
}