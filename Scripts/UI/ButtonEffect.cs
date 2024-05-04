using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��ư�� �ൿ���� ��� ���� ��ɵ�. ���� �ൿ���� ��� ����Ƽ �̺�Ʈ�� ���
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
        // ������ �� ����Ʈ
    }
    public void PlayButtonSound()
    {
        SoundManager.instance.Play(buttonSound);
    }
}