using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��ư�� �ൿ���� ��� ���� ��ɵ�. ���� �ൿ���� ��� ����Ƽ �̺�Ʈ�� ���
/// </summary>
public class ButtonEffect : MonoBehaviour
{
    [SerializeField] AudioClip buttonSound;
    [SerializeField] GameObject buttonEffect;
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
        GetComponent<Button>().interactable = false;
    }
    public void PlayAnimation()
    {
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