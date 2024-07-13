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
        Vector2 randomVec = GetRandomPositionWithinRadius();
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(randomVec, verticalVel);
    }

    Vector2 GetRandomPositionWithinRadius()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, radius);
        float x = distance * Mathf.Cos(angle);
        float y = distance * Mathf.Sin(angle);

        return new Vector2(x, y);
    }
}