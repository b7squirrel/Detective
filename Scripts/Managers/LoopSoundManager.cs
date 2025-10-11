using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 루프되는 사운드는 중첩되지 않도록 하나만 재생
/// 화면 상에서 해당 오브젝트가 모두 사라지면 루프 사운드 정지
/// </summary>
public class LoopSoundManager : MonoBehaviour
{
    Dictionary<AudioClip, int> audioNums = new Dictionary<AudioClip, int>();

    public void RegisterAudio(AudioClip audio)
    {
        // 딕셔너리에서 오디오 이름으로 된 string항목이 있는지 검색
        if (audioNums.ContainsKey(audio))
        {
            // 항목이 있으면 값에 1을 더함
            audioNums[audio]++;
        }
        else
        {
            // 없으면 매개변수로 넘겨받은 audioName으로 딕셔너리에 추가. 값은 1
            audioNums.Add(audio, 1);
        }

        if (audioNums[audio] == 1)
        {
            SoundManager.instance.PlayLoop(audio);
        }
    }

    public void UnregisterAudio(AudioClip audio)
    {
        // 딕셔너리에서 오디오 이름으로 된 string 항목이 있는지 검색
        if (audioNums.ContainsKey(audio))
        {
            // 항목이 있으면 값에서 1을 빼줌
            audioNums[audio]--;

            // 선택사항: 0 이하가 되면 딕셔너리에서 제거 (메모리 정리)
            if (audioNums[audio] <= 0)
            {
                audioNums.Remove(audio);
                SoundManager.instance.StopLoop(audio);
            }
        }
        else
        {
            // 없으면 오디오가 Loop Sound Manager에 등록되어 있지 않음 이라고 로그를 띄움
            Debug.LogWarning($"오디오가 Loop Sound Manager에 등록되어 있지 않음: {audio.name}");
        }
    }

    // 유틸리티 메서드: 특정 오디오의 개수 확인
    public int GetAudioCount(AudioClip audio)
    {
        return audioNums.ContainsKey(audio) ? audioNums[audio] : 0;
    }
}
