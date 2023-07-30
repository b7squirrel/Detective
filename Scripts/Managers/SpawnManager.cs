using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    Character character;

    private void Awake()
    {
        instance = this;
        character = Player.instance.GetComponent<Character>();
    }
    public void SpawnObject(Vector3 worldPosition, GameObject toSpawn, bool isGem, int experience)
    {
        Transform pickup = null;

        if (isGem)
        {
            pickup = GameManager.instance.poolManager.GetGem(toSpawn, experience).transform;
        }
        else
        {
            pickup = GameManager.instance.poolManager.GetMisc(toSpawn).transform;

        }

        if (pickup.GetComponent<GemPickUpObject>() != null)
            pickup.GetComponent<GemPickUpObject>().ExpAmount = experience;

        pickup.position = worldPosition;
    }
}
