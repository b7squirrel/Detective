using UnityEngine;

public class CheckSortingOrder : MonoBehaviour
{
    SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        Debug.Log("sorting order = " + sr.sortingLayerName);

    }
}