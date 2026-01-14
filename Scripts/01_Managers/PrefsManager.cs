using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] ButtonToggle buttonToggleSound;
    [SerializeField] ButtonToggle buttonToggleMusic;
    
    SoundManager soundManager;
    MusicManager musicManager;
    bool soundState;
    bool musicState;
    
    private void Start()
    {
        Load();
        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();
        soundManager.Init();
        InitSoundState(soundState);
        InitMusicState(musicState);
    }
    
    public void Save()
    {
        PlayerPrefs.SetInt("SoundState", soundState ? 1 : 0);
        PlayerPrefs.SetInt("MusicState", musicState ? 1 : 0);  // ← 추가
        PlayerPrefs.Save();  // 즉시 디스크에 저장
    }
    
    public void Load()
    {
        if (!PlayerPrefs.HasKey("SoundState"))
        {
            soundState = true;
        }
        else
        {
            soundState = PlayerPrefs.GetInt("SoundState") != 0;
        }
        
        if (!PlayerPrefs.HasKey("MusicState"))
        {
            musicState = true;
        }
        else
        {
            musicState = PlayerPrefs.GetInt("MusicState") != 0;  // ← 수정 (soundState → musicState)
        }
    }
    
    void InitSoundState(bool _soundState)
    {
        soundManager.SetState(_soundState);
        buttonToggleSound.SetImage(_soundState);
    }
    
    void InitMusicState(bool _musicState)
    {
        musicManager.SetState(_musicState);
        buttonToggleMusic.SetImage(_musicState);
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
    
    void DeleteSoundState()
    {
        PlayerPrefs.DeleteKey("SoundState");
        PlayerPrefs.DeleteKey("MusicState");  // 이것도 추가하는 것이 좋습니다
        Debug.Log("Sound and Music states have been deleted from PlayerPrefs.");
    }
}