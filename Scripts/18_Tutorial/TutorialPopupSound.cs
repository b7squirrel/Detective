using UnityEngine;

public class TutorialPopupSound : MonoBehaviour
{
    [SerializeField] AudioClip openSound;  // 팝업 열릴 때
    [SerializeField] AudioClip closeSound; // 팝업 닫힐 때

    void OnEnable()
    {
        if (openSound != null)
            SoundManager.instance.Play(openSound);
    }

    // 닫기 버튼 OnClick()에 연결
    public void PlayCloseSound()
    {
        if (closeSound != null)
            SoundManager.instance.Play(closeSound);
    }
}