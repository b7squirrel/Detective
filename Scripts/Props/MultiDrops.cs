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
        StartCoroutine(GenItems(false));
    }
    public void InitBossItems(int _numbersToDrop, GameObject _itemsToDrop)
    {
        itemToDrop = _itemsToDrop;
        numberOfItems = _numbersToDrop;
        StartCoroutine(GenItems(true));
    }
    IEnumerator GenItems(bool isBoss)
    {
        while (numberOfItems > 0)
        {
            SpawnManager.instance.SpawnObject(transform.position, itemToDrop, false, 0);
            numberOfItems--;
            if (numberOfItems <= 0)
            {
                if (isBoss)
                {
                    PullDroppedItems();
                    yield break;
                }
                Destroy(gameObject);
            }
            yield return null;
        }
    }
    IEnumerator PullDroppedItems()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.character.GetComponentInChildren<Magnetic>().MagneticField(30f);
        Destroy(gameObject);
    }
}