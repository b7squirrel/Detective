using System;
using UnityEngine;

[Serializable]
public class AudioCredit
{
    [field: SerializeField] public AudioClip Clip;
    [field: SerializeField] public string Title;
    [field: SerializeField] public string Credit;
}

/// <summary>
/// 클립, 타이틀, 크레딧을 포함하는 클래스 리스트를 가진 SO
/// </summary>
[CreateAssetMenu(menuName = "AudioCredit", fileName = "AudioCreditSo")]
public class AudioCreditData : ScriptableObject
{
    [field: SerializeField] public AudioCredit[] AudioCredits;

}