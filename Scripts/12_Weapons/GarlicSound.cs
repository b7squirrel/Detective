using UnityEngine;

public class GarlicSound : MonoBehaviour
{
    [SerializeField] AudioClip[] noteSounds;
    int currentIndex;
    bool isPlaying = false;

    public void PlayNoteSound(int index)
    {
        SoundManager.instance.Play(noteSounds[index]);
    }
    public void PlayTest()
    {
        int index = UnityEngine.Random.Range(0, noteSounds.Length);
        SoundManager.instance.Stop(noteSounds[currentIndex]);
        SoundManager.instance.Play(noteSounds[index]);
    }
    public void PlayLoopSound()
    {
        if(isPlaying) return;
        int index = UnityEngine.Random.Range(0, noteSounds.Length);
        SoundManager.instance.Stop(noteSounds[currentIndex]);
        SoundManager.instance.PlayLoop(noteSounds[index]);
        currentIndex = index;
        isPlaying = true;
    }
}
