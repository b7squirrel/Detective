using System;
using System.Collections;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [field : SerializeField] public Vector2 BouncingDir { get; set; }
    [SerializeField] float bouncingForce;
    [SerializeField] float bouncingTime;
    [SerializeField] AudioClip bouncerSFX;
    [SerializeField] GameObject dieEffect;
    Animator anim;

    void OnEnable()
    {
        BouncingDir = transform.up;
        anim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<IBouncable>() != null)
        {
            other.gameObject.GetComponent<IBouncable>().GetBounced(bouncingForce, BouncingDir, bouncingTime);
            anim.SetTrigger("Push");
            SoundManager.instance.Play(bouncerSFX);
        }
    }
    public void DeactivateWall()
    {
        Instantiate(dieEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
