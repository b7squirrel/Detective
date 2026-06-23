using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    // ✅ 수정: SerializeField 제거 — PlayerPrefs에서 로드
    private bool hapticsEnabled = true;

    const string KEY_HAPTICS = "HapticsState";

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
            return;
        }

        Load(); // ✅ 추가
    }

    void Start()
    {
        HapticController.Init();
    }

    // ───────────────────────────────────────────
    //  저장 / 불러오기
    // ───────────────────────────────────────────

    void Load()
    {
        hapticsEnabled = PlayerPrefs.GetInt(KEY_HAPTICS, 1) == 1;
    }

    public void SetState(bool state)
    {
        hapticsEnabled = state;
        PlayerPrefs.SetInt(KEY_HAPTICS, state ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetState() => hapticsEnabled;

    // ───────────────────────────────────────────
    //  기존 진동 메서드 유지
    // ───────────────────────────────────────────

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