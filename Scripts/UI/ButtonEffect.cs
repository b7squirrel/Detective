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
    [SerializeField] GameObject buttonEffect; // 버튼이 눌러지면 이펙트 발생
    [SerializeField] bool shouldBeLocked;
    [SerializeField] bool ignoreButtonEffectAnim;

    // ⭐ 추가: 외부에서 사운드를 직접 제어할 때 자동 재생을 스킵하기 위한 플래그
    // StageViewerController처럼 onClick 순서 문제로 직접 PlayButtonSound()를 호출하는 경우 사용
    [HideInInspector] public bool ignoreSoundOnce = false;

    public bool ShoutldBeInitialSound { get; set; } = true;

    Button myButton;
    Animator bottonAnim;

    void Awake()
    {
        myButton = GetComponentInChildren<Button>();
        bottonAnim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(LockButton);
            myButton.onClick.AddListener(PlayAnimation);
            myButton.onClick.AddListener(PlayButtonSound);
        }

        if (buttonEffect != null) buttonEffect.SetActive(false);
    }

    public void LockButton()
    {
        if (shouldBeLocked == false) { return; }
        GetComponent<Button>().interactable = false;
    }

    public void PlayAnimation()
    {
        if (ignoreButtonEffectAnim) return;
        if (bottonAnim == null) return;
        bottonAnim.SetTrigger("Pressed");
        ButtonParticleEffect();
    }

    public void ButtonParticleEffect()
    {
        // 눌렀을 때 이펙트
        if (buttonEffect == null) return;
        buttonEffect.SetActive(true);
        buttonEffect.GetComponent<Animator>().SetTrigger("On");
    }

    public void PlayButtonSound()
    {
        // ⭐ 외부에서 이미 직접 호출했다면 자동 재생 스킵 (한 번만 스킵)
        if (ignoreSoundOnce)
        {
            ignoreSoundOnce = false;
            return;
        }

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