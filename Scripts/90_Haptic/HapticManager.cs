using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }
    [SerializeField] private bool hapticsEnabled = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HapticController.Init(); // ← 초기화 추가
    }

    public static void PlayDamage()
    {
        if (Instance == null || !Instance.hapticsEnabled) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    public static void PlayHeavyDamage()
    {
        if (Instance == null || !Instance.hapticsEnabled) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
    }

    public static void PlayDeath()
    {
        if (Instance == null || !Instance.hapticsEnabled) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
    }
}