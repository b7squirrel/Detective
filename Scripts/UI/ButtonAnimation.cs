using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    Animator anim;
    [SerializeField] MainMenu mainMenu;
    [SerializeField] AudioClip pressed, released;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void PlayButtonAnim(bool isPaused)
    {
        if (isPaused)
        {
            anim.SetTrigger("Pressed");
            SoundManager.instance.Play(pressed);
        }
        else
        {
            anim.SetTrigger("Released");
            SoundManager.instance.Play(released);
        }
    }
}
