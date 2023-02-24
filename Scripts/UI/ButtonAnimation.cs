using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    Animator anim;
    [SerializeField] MainMenu mainMenu;
    [SerializeField] AudioClip pressed;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void PlayButtonAnim(bool isPaused)
    {
        if (isPaused == false)
        {
            anim.SetTrigger("Pressed");
            SoundManager.instance.Play(pressed);
        }
    }
}
