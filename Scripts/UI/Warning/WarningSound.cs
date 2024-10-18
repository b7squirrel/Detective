using UnityEngine;

public class WarningSound : MonoBehaviour
{
    [SerializeField] AudioClip startingSound;
    [SerializeField] AudioClip closingSound;
    [SerializeField] AudioClip idleSound;

    // �ִϸ��̼� �̺�Ʈ
    public void PlayStartingSound()
    {
        SoundManager.instance.Play(startingSound);
    }
    public void PlayClosingSound()
    {
        SoundManager.instance.Play(closingSound);
    }
    public void PlayIdleSound()
    {
        // ��ġ ��ȭ ���� ���, �ݵ�� ���
        SoundManager.instance.Play(idleSound);
    }
}
