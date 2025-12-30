using System;
using UnityEngine;

[Serializable]
public class AudioCredit
{
    [SerializeField] public AudioClip Clip;
    [SerializeField] public StageMusicType MusicType;
    [SerializeField] public string Credit;
}

/// <summary>
/// 배경음악 및 사운드 효과의 크레딧 정보를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "AudioCredit", fileName = "AudioCreditSo")]
public class AudioCreditData : ScriptableObject
{
    [field: SerializeField] public AudioCredit[] AudioCredits;

}