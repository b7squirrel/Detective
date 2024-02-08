using System.Collections;
using UnityEngine;

public class MultiDrops : MonoBehaviour
{
    GameObject itemToDrop;
    int numberOfItems;

    public void Init(int _numbersToDrop, GameObject _itemsToDrop)
    {
        itemToDrop = _itemsToDrop;
        numberOfItems = _numbersToDrop;
        StartCoroutine(GenItems());
    }
    IEnumerator GenItems()
    {
        while (numberOfItems > 0)
        {
            SpawnManager.instance.SpawnObject(transform.position, itemToDrop, false, 0);
            numberOfItems--;
            if (numberOfItems <= 0)
            {
                Destroy(gameObject);
            }
            yield return null;
        }
    }
}