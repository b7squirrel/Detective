using UnityEngine;

public class PickupSpawner : SingletonBehaviour<PickupSpawner>
{
    Character character;
    GemManager gemManager;
    
    /// <summary>
    /// 월드에 픽업 가능한 오브젝트(보석, 아이템, 이펙트)를 스폰합니다
    /// </summary>
    public void SpawnPickup(Vector3 worldPosition, GameObject prefab, bool isGem, int experience)
    {
        if (prefab == null) return;
        
        if (character == null && Player.instance != null)
        {
            character = Player.instance.GetComponent<Character>();
        }
        
        if (gemManager == null && GameManager.instance != null)
        {
            gemManager = GameManager.instance.GemManager;
        }
        
        Transform pickup = null;
        
        if (isGem)
        {
            if (GameManager.instance?.poolManager == null) return;
            pickup = GameManager.instance.poolManager.GetGem(prefab).transform;
        }
        else
        {
            if (GameManager.instance?.poolManager == null) return;
            GameObject obj = GameManager.instance.poolManager.GetMisc(prefab);
            if (obj == null) return;
            pickup = obj.transform;
        }

        if (pickup.GetComponent<GemPickUpObject>() != null)
        {
            pickup.GetComponent<GemPickUpObject>().ExpAmount = experience;
        }

        pickup.position = worldPosition;
    }
}
