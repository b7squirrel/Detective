using UnityEngine;

/// <summary>
/// �������� ��ӵǴ� ���� ����, ���� ���� �÷���
/// </summary>
public class PropSound : MonoBehaviour
{
    [SerializeField] AudioClip initSound;
    [SerializeField] AudioClip landingSound;

    void OnEnable()
    {
        PlayInitSound();
    }

    public void PlayInitSound()
    {
        SoundManager.instance.PlaySoundWith(initSound, 1f, true, 0f);
    }
    public void PlayLandingSound() 
    {
        SoundManager.instance.PlaySoundWith(landingSound, 1f, true, 0f);
    }

}