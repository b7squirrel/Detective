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
/// Ŭ��, Ÿ��Ʋ, ũ������ �����ϴ� Ŭ���� ����Ʈ�� ���� SO
/// </summary>
[CreateAssetMenu(menuName = "AudioCredit", fileName = "AudioCreditSo")]
public class AudioCreditData : ScriptableObject
{
    [field: SerializeField] public AudioCredit[] AudioCredits;

}