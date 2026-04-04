using UnityEngine;

public enum StageGroundType {GreenForest, OrangeDesert, GreyStone, BlueIce, GreyLava}
public class StageGroundManager : MonoBehaviour
{
    [Tooltip("GreenForest, OrangeDesert, GreyStone, BlueIce, GreyLava, ")]
    [SerializeField] GameObject[] groundPrefabs;

    private GameObject currentGround; // 현재 생성된 ground를 추적 (선택)

    public void InitGround(StageGroundType groundType)
    {
        int index = (int)groundType; // enum → int 변환 (Green=0, Grey=1, ...)

        if (index < 0 || index >= groundPrefabs.Length)
        {
            Logger.LogError($"[StageGroundManager] groundPrefabs에 index {index}가 없습니다.");
            return;
        }

        if (groundPrefabs[index] == null)
        {
            Logger.LogError($"[StageGroundManager] groundPrefabs[{index}]가 비어 있습니다.");
            return;
        }

        // 기존 ground가 있으면 제거
        if (currentGround != null)
            Destroy(currentGround);

        // 새 ground 생성 (이 오브젝트의 자식으로)
        currentGround = Instantiate(groundPrefabs[index], transform);
    }
}
