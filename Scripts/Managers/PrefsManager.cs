using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] ButtonToggle buttonToggle;
    SoundManager soundManager;
    MusicManager musicManager;
    bool soundState;

    // 게임을 시작할 떄 사운드 mute한 채로 시작한 후, 데이터를 확인해서 mute할지 말지 결정
    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();
        Load();
        soundManager.Init();
        InitSoundState(soundState);
    }
    public void Save()
    {
        PlayerPrefs.SetInt("SoundState", soundState ? 1 : 0);
        Debug.Log("Sound State Saved = " + soundState);
    }
    public void Load()
    {
        if (!PlayerPrefs.HasKey("SoundState"))
        {
            soundState = true;
            Save();
        }
        else
        {
            soundState = PlayerPrefs.GetInt("SoundState") != 0;
        }
        Debug.Log("Sound State Loaded = " + soundState);
    }
    void InitSoundState(bool _soundState)
    {
        // 사운드, 음악, 옵션 사운드 버튼 이미지 셋업
        soundManager.SetState(_soundState);
        musicManager.SetState(_soundState);

        buttonToggle.SetImage(_soundState);
    }
    public void SetSouindState()
    {
        soundState = !soundState;
        Debug.Log("sound State in SetSound State Method" + soundState);
        InitSoundState(soundState);
        Save();
    }
}