using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    Vector2 pastPosition, currentPosition;

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    public void Init(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    public void Flip(bool flip)
    {
        sr.flipX = flip;
    }
}
