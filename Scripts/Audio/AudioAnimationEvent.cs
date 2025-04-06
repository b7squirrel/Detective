using System.Collections.Generic;
using UnityEngine;

// 애니메이터가 붙어 있는 오브젝트에 붙여서 사용
public class AudioAnimationEvent : MonoBehaviour
{
    [SerializeField] List<AudioClip> audioClips;

    public void PlayAudioClip(int index)
    {
        SoundManager.instance.Play(audioClips[index]);
    }
    public void PlayAudioClipLoop(int index)
    {
        SoundManager.instance.PlayLoop(audioClips[index]);
    }
    public void StopAudioClipLoop(int index)
    {
        SoundManager.instance.StopLoop(audioClips[index]);
    }
}
