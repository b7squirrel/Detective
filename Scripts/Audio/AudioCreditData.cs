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
/// 클립, 타이틀, 크레딧을 포함하는 클래스 리스트를 가진 SO
/// </summary>
[CreateAssetMenu(menuName = "AudioCredit", fileName = "AudioCreditSo")]
public class AudioCreditData : ScriptableObject
{
    [field: SerializeField] public AudioCredit[] AudioCredits;

}