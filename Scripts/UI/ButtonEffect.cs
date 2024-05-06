using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 실제 버튼의 행동과는 상관 없는 기능들. 실제 행동들은 모두 유니티 이벤트로 등록
/// 코인이 부족하다든지 할 때는 shouldBeInitialSound를 false로 만들어서 두 번째 사운드 재생
/// </summary>
public class ButtonEffect : MonoBehaviour
{
    [SerializeField] AudioClip buttonSound;
    [SerializeField] AudioClip buttonSoundAlt;
    [SerializeField] GameObject buttonEffect;
    [SerializeField] bool shouldBeLocked;
    [SerializeField] bool ignoreButtonEffectAnim;
    public bool ShoutldBeInitialSound { get; set; } = true;
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
        Debug.Log("Button Flag = " + ShoutldBeInitialSound);
        if (ShoutldBeInitialSound)
        {
            SoundManager.instance.Play(buttonSound);
        }
        else
        {
            SoundManager.instance.Play(buttonSoundAlt);

        }
    }
}