using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [field: SerializeField]
    public bool IsAvailable { get; set; }

    private void OnEnable()
    {
        IsAvailable = true;
    }
}
