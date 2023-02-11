using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField] List<GameObject> dropItemPrefab;
    [SerializeField][Range(0f, 1f)] float chance = 1f;

    bool isQuiting;

    void OnApplicationQuit()
    {
        isQuiting = true;
    }

    public void CheckDrop()
    {
        if (isQuiting) return;

        if (Random.value < chance)
        {
            GameObject toDrop = dropItemPrefab[Random.Range(0, dropItemPrefab.Count)];
            SpawnManager.instance.SpawnObject(transform.position, toDrop);
        }
    }
}
