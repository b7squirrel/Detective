using UnityEngine;

/// <summary>
/// 상자를 열었을 때, 여러개의 드롭이 생기게 하는 클래스
/// 보석, 동전 등
/// </summary>
public class MultiPropCreator : MonoBehaviour
{
    [SerializeField] GameObject propPrefab;
    [SerializeField] int numberOfDrops;

    void OnEnable()
    {
        for (int i = 0; i < numberOfDrops; i++)
        {
            GameObject drop = GameManager.instance.poolManager.GetMisc(propPrefab);
            drop.transform.position = transform.position;
        }
    }
}
