using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform shootPoint;
    public Transform effectPoint;
    [field : SerializeField] public bool IsDirectional{get; private set;}
}
