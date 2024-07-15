using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager instance;
    [SerializeField] int MaxGemNumbers;
    //[SerializeField] Sprite[] gemSprites; // b, g, p, big b, big g, big p
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
    //public GemProperties GetGemProperties(int _exp)
    //{
    //    GemExp gemExp = new GemExp();
    //    int gemIndex = gemExp.GetGemIndex(_exp);
    //    float size = gemIndex < 3 ? 1f : 2f;

    //    // 스프라이트, 사이즈, 경험치를 담는 클래스 반환
    //    return new GemProperties(gemSprites[gemIndex], size, _exp);
    //}


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
