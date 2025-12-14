using UnityEngine;

public class PropPoint : MonoBehaviour
{
    void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
}
