using UnityEngine;

public class TutorialPopupSound : MonoBehaviour
{
    [SerializeField] AudioClip openSound;  // 팝업 열릴 때
    [SerializeField] AudioClip closeSound; // 팝업 닫힐 때

    void OnEnable()
    {
        if (openSound != null && SoundManager.instance != null)
            SoundManager.instance.Play(openSound);
    }

    public void PlayCloseSound()
    {
        if (closeSound != null && SoundManager.instance != null)
            SoundManager.instance.Play(closeSound);
    }
}