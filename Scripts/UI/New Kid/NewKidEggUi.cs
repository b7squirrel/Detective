using UnityEngine;

public class NewKidEggUi : MonoBehaviour
{
    [SerializeField] AudioClip eggDanceSound;
    [SerializeField] AudioClip newFriendTextSound;
    [SerializeField] AudioClip breakingEggSound;

    // animation event
    // Egg Panel Manager�� �ִϸ��̼��� �������� �˸��� �ڽ��� ��Ȱ��ȭ
    public void AnimFinished()
    {
        GameManager.instance.eggPanelManager.EggAnimFinished();
        GameManager.instance.eggPanelManager.EggImageUp(false);
    }

    // animation event
    public void PlayEggDanceSound()
    {
        SoundManager.instance.Play(eggDanceSound);

    }
    // New Friend �ؽ�Ʈ�� �� �ϴ� �Ҹ�
    public void PlayNewFriendSound()
    {
        SoundManager.instance.Play(newFriendTextSound);
    }

    public void PlayBreakingEggSound()
    {
        SoundManager.instance.Play(breakingEggSound);

    }
}