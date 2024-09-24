using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��ư�� �ൿ���� ��� ���� ��ɵ�. ���� �ൿ���� ��� ����Ƽ �̺�Ʈ�� ���
/// ������ �����ϴٵ��� �� ���� shouldBeInitialSound�� false�� ���� �� ��° ���� ���
/// </summary>
public class ButtonEffect : MonoBehaviour
{
    [SerializeField] AudioClip buttonSound;
    [SerializeField] AudioClip buttonSoundAlt;
    [SerializeField] GameObject buttonEffect; // ��ư�� �������� ����Ʈ �߻�
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
        ButtonParticleEffect();
    }

    public void ButtonParticleEffect()
    {
        // ������ �� ����Ʈ
        if(buttonEffect == null) return;
        buttonEffect.GetComponent<Animator>().SetTrigger("On");
    }
    public void PlayButtonSound()
    {
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