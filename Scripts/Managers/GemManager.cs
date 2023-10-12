using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager instance;
    [SerializeField] int MaxGemNumbers;
    int gemNumbers, potentialExp;
    Character character;

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

    public void IncreaseGemCount() => gemNumbers++;
    public void DecreaseGemCount() => gemNumbers--;

    public bool HasPotentialExp() => potentialExp > 0;

    public void IncreasePotentialExp(int exp) => potentialExp += exp;

    public void ResetPotentialExp() => potentialExp = 0;
    public int GetPotentialExp() => potentialExp;
}
