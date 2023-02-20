using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    private void Awake()
    {
        instance= this;
    }
    public void SpawnObject(Vector3 worldPosition, GameObject toSpawn, bool isGem, int experience)
    {
        Transform pickUP = Instantiate(toSpawn).transform;

        if (pickUP.GetComponent<GemPickUpObject>() != null)
            pickUP.GetComponent<GemPickUpObject>().ExpAmount = experience;

        pickUP.position = worldPosition;
    }
}
