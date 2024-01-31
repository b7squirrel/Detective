using System;
using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager instance;
    [SerializeField] int MaxGemNumbers;
    int gemNumbers, potentialExp;
    Character character;

    public event Action OnGemNumberChange; // Display Gem Number로 디버깅 하기 위한 액션

    [Header("Feedback")]
    [SerializeField] AudioClip gemPickup_A;

    void Awake()
    {
        instance = this;
        character = FindObjectOfType<Character>();
    }

    public void PutExpToPlayer(int exp)
    {
        PlayGemSound();
        character.level.AddExperience(exp);
    }

    public void PlayGemSound() => SoundManager.instance.Play(gemPickup_A);

    public bool IsMaxGemNumber() => gemNumbers >= MaxGemNumbers;

    public void IncreaseGemCount()
    {
        gemNumbers++;
        OnGemNumberChange?.Invoke();
    }
    public void DecreaseGemCount()
    {
        gemNumbers--;
        OnGemNumberChange?.Invoke();
    }

    public bool HasPotentialExp() => potentialExp > 0;

    public void IncreasePotentialExp(int exp) => potentialExp += exp;

    public void ResetPotentialExp() => potentialExp = 0;
    public int GetPotentialExp() => potentialExp;
    public int GetGemNumbers() => gemNumbers;
}
