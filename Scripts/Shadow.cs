using UnityEngine;

public class Shadow : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitShadow(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    public void FLip(bool flip)
    {
        sr.flipX = flip;
    }
}
