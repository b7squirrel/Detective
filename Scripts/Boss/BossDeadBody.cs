using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeadBody : MonoBehaviour
{
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;
    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    //animation events
    public void PlayCrownDropSFX()
    {
        SoundManager.instance.Play(crownDropSFX);
    }
    public void PlayerSquelchSFX()
    {
        SoundManager.instance.Play(squelchSFX);
        SoundManager.instance.Play(squeackSFX);
    }
}
