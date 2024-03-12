using UnityEngine;

public class NewKidEggUi : MonoBehaviour
{
    [SerializeField] AudioClip newFriendTextSound;

    // animation event
    // Egg Panel Manager�� �ִϸ��̼��� �������� �˸��� �ڽ��� ��Ȱ��ȭ
    public void AnimFinished()
    {
        GameManager.instance.eggPanelManager.EggAnimFinished();
        GameManager.instance.eggPanelManager.EggImageUp(false);
    }

    // animation event
    // New Friend �ؽ�Ʈ�� �� �ϴ� �Ҹ�
    public void PlayNewFriendSound()
    {
        SoundManager.instance.Play(newFriendTextSound);
    }
}