using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 실제 버튼의 행동과는 상관 없는 기능들. 실제 행동들은 모두 유니티 이벤트로 등록
/// </summary>
public class ButtonEffect : MonoBehaviour
{
    [SerializeField] AudioClip buttonSound;
    [SerializeField] GameObject buttonEffect;
    [SerializeField] bool shouldBeLocked;
    [SerializeField] bool ignoreButtonEffectAnim;
    Button myButton;
    void Start()
    {
        if (myButton == null)
            myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(LockButton);
            myButton.onClick.AddListener(PlayAnimation);
            myButton.onClick.AddListener(PlayButtonSound);
        }
    }
    public void LockButton()
    {
        if(shouldBeLocked == false) { return; }
        GetComponent<Button>().interactable = false;
    }
    public void PlayAnimation()
    {
        if (ignoreButtonEffectAnim) return;
        if (myButton.GetComponent<Animator>() == null) return;
        myButton.GetComponent<Animator>().SetTrigger("Pressed");
    }

    public void ButtonParticleEffect()
    {
        // 눌렀을 때 이펙트
    }
    public void PlayButtonSound()
    {
        SoundManager.instance.Play(buttonSound);
    }
}