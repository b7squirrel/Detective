
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    Character character;
    GemManager gemManager;

    private void Awake()
    {
        instance = this;
        character = Player.instance.GetComponent<Character>();
        gemManager = GameManager.instance.GemManager;
    }
    public void SpawnObject(Vector3 worldPosition, GameObject toSpawn, bool isGem, int experience)
    {
        Transform pickup = null;
        if (isGem)
        {
            if (gemManager.IsMaxGemNumber()) // 보석이 일정량 이상 늘어나면
            {
                gemManager.IncreasePotentialExp(experience); // 임시로 경험치를 저장해 둠
                return;
            }
            pickup = GameManager.instance.poolManager.GetGem(toSpawn, experience).transform;
            gemManager.IncreaseGemCount(); // 보석 수 증가
        }
        else
        {
            GameObject o = GameManager.instance.poolManager.GetMisc(toSpawn);
            if (o == null) return;
            pickup = o.transform;
        }

        if (pickup.GetComponent<GemPickUpObject>() != null)
        {
            pickup.GetComponent<GemPickUpObject>().ExpAmount = experience;
        }

        pickup.position = worldPosition;
    }
}
