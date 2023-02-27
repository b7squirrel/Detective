using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public Vector2 BouncingDir { get; set; }
    [SerializeField] float bouncingForce;
    [SerializeField] float bouncingTime;

    void OnEnable()
    {
        BouncingDir = transform.up;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Who is bouncing = " + other.gameObject.GetComponent<IBouncable>());
        if (other.gameObject.GetComponent<IBouncable>() != null)
        {
            other.gameObject.GetComponent<IBouncable>().GetBounced(bouncingForce, BouncingDir, bouncingTime);
        }
    }
}
