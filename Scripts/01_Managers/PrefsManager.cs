using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] ButtonToggle buttonToggleSound;
    [SerializeField] ButtonToggle buttonToggleMusic;

    // ✅ 추가
    [Header("Haptic")]
    [SerializeField] ButtonToggle buttonToggleHaptic;

    SoundManager soundManager;
    MusicManager musicManager;

    bool soundState;
    bool musicState;
    bool hapticState; // ✅ 추가

    private void Start()
    {
        Load();
        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();
        soundManager.Init();
        InitSoundState(soundState);
        InitMusicState(musicState);
        InitHapticState(hapticState); // ✅ 추가
    }

    public void Save()
    {
        PlayerPrefs.SetInt("SoundState", soundState ? 1 : 0);
        PlayerPrefs.SetInt("MusicState", musicState ? 1 : 0);
        PlayerPrefs.SetInt("HapticsState", hapticState ? 1 : 0); // ✅ 추가
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("SoundState"))
            soundState = true;
        else
            soundState = PlayerPrefs.GetInt("SoundState") != 0;

        if (!PlayerPrefs.HasKey("MusicState"))
            musicState = true;
        else
            musicState = PlayerPrefs.GetInt("MusicState") != 0;

        // ✅ 추가 — 기존 패턴 동일하게 유지
        if (!PlayerPrefs.HasKey("HapticsState"))
            hapticState = true;
        else
            hapticState = PlayerPrefs.GetInt("HapticsState") != 0;
    }

    void InitSoundState(bool state)
    {
        soundManager.SetState(state);
        buttonToggleSound.SetImage(state);
    }

    void InitMusicState(bool state)
    {
        musicManager.SetState(state);
        buttonToggleMusic.SetImage(state);
    }

    // ✅ 추가
    void InitHapticState(bool state)
    {
        // HapticManager는 Essential씬에만 있으므로 null 체크 필수
        Debug.Log($"[PrefsManager] InitHapticState → {state}, HapticManager.Instance: {HapticManager.Instance}");
        HapticManager.Instance?.SetState(state);
        buttonToggleHaptic?.SetImage(state);
    }

    public void SetSoundState()
    {
        soundState = !soundState;
        InitSoundState(soundState);
        Save();
    }

    public void SetMusicState()
    {
        musicState = !musicState;
        InitMusicState(musicState);
        Save();
    }

    // ✅ 추가
    public void SetHapticState()
    {
        hapticState = !hapticState;
        Debug.Log($"[PrefsManager] SetHapticState called → {hapticState}");
        InitHapticState(hapticState);
        Save();
    }
}