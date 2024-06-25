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
/// Ŭ��, Ÿ��Ʋ, ũ������ �����ϴ� Ŭ���� ����Ʈ�� ���� SO
/// </summary>
[CreateAssetMenu(menuName = "AudioCredit", fileName = "AudioCreditSo")]
public class AudioCreditData : ScriptableObject
{
    [field: SerializeField] public AudioCredit[] AudioCredits;

}