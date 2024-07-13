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