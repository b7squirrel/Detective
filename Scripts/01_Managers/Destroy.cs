using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] float life;
    void Start()
    {
        Destroy(gameObject, life);
    }
}
