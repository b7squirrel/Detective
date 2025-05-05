using UnityEngine;

/// <summary>
/// 오브젝트가 활성화 되는 순간 On Enable로 Shadow Height 실행
/// </summary>
public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;

    [SerializeField] float radius;
   
    void OnEnable()
    {
        Vector2 randomVec = new GeneralFuctions().GetRandomPosInCircle(radius);
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(randomVec, verticalVel);
    }
}