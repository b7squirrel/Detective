using UnityEngine;

/// <summary>
/// Ƣ������� ��ü�� ������ �����ϰ� �����ִ� Ŭ����
/// </summary>
public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;

    // ���� ��ǥ ���� ����
    [SerializeField] float radius;
   
    void OnEnable()
    {
        Vector2 randomVec = new GeneralFuctions().GetRandomPosInCircle(radius);
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(randomVec, verticalVel);
    }
}