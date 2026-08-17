using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    // ✅ 수정: SerializeField 제거 — PlayerPrefs에서 로드
    private bool hapticsEnabled = false;

    // ⭐ 추가: 이 기기가 Lofelt의 advanced haptics(정밀 진동)를 지원하는지 캐싱
    private static bool isAdvancedHapticsSupported = false;

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
        // ⭐ 수정: Init() 반환값을 저장해서 이후 재생 시 분기에 사용
        isAdvancedHapticsSupported = HapticController.Init();
        Logger.Log($"[HapticManager] Advanced Haptics 지원 여부: {isAdvancedHapticsSupported}");
    }

    // ───────────────────────────────────────────
    //  저장 / 불러오기
    // ───────────────────────────────────────────

    void Load()
    {
        hapticsEnabled = PlayerPrefs.GetInt(KEY_HAPTICS, 0) == 1;
    }

    public void SetState(bool state)
    {
        hapticsEnabled = state;
        PlayerPrefs.SetInt(KEY_HAPTICS, state ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetState() => hapticsEnabled;

    // ───────────────────────────────────────────
    //  ⭐ 추가: 지원 기기 여부에 따라 프리셋 또는 기본 진동으로 분기
    // ───────────────────────────────────────────

    static void PlayPresetOrFallback(HapticPatterns.PresetType presetType)
    {
        if (Instance == null || !Instance.hapticsEnabled) return;

        if (isAdvancedHapticsSupported)
        {
            HapticPatterns.PlayPreset(presetType);
        }
        else
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }

    // ───────────────────────────────────────────
    //  기존 진동 메서드 유지 (내부만 PlayPresetOrFallback으로 교체)
    // ───────────────────────────────────────────

    public static void PlayDamage()
    {
        PlayPresetOrFallback(HapticPatterns.PresetType.LightImpact);
    }

    public static void PlayHeavyDamage()
    {
        PlayPresetOrFallback(HapticPatterns.PresetType.MediumImpact);
    }

    public static void PlayDeath()
    {
        PlayPresetOrFallback(HapticPatterns.PresetType.HeavyImpact);
    }
}