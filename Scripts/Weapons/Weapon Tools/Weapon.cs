using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform shootPoint;
    [field : SerializeField] public bool IsDirectional{get; private set;}
}
