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
        //DeleteSoundState(); // 개발 중 초기화 상태 확인용 (사용자 설정 삭제)
        Load();

        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();
        soundManager.Init();
        InitSoundState(soundState);
        InitMusicState(musicState);
        Save();

    }
    public void Save()
    {
        PlayerPrefs.SetInt("SoundState", soundState ? 1 : 0);
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

        if (!PlayerPrefs.HasKey("MusicState"))
        {
            musicState = true;
            Save();
        }
        else
        {
            soundState = PlayerPrefs.GetInt("MusicState") != 0;
        }
    }
    void InitSoundState(bool _soundState)
    {
        // 사운드 매니저, 음악 매니저, 옵션 토글 버튼에 상태 반영
        soundManager.SetState(_soundState);
        buttonToggleSound.SetImage(_soundState); // 버튼 이미지도 상태에 맞게 설정
    }
    void InitMusicState(bool _musicState)
    {
        musicManager.SetState(_musicState);
        buttonToggleMusic.SetImage(_musicState); // 버튼 이미지도 상태에 맞게 설정
    }

    // 버튼 이벤트
    public void SetSoundState() // Toggle 버튼을 눌렀을 때 호출됨
    {
        soundState = !soundState;
        InitSoundState(soundState);
        Save();
    }
    public void SetMusicState() // 음악 토글 버튼
    {
        musicState = !musicState;
        InitMusicState(musicState);
        Save();
    }

    void DeleteSoundState()
    {
        PlayerPrefs.DeleteKey("SoundState");
        Debug.Log("SoundState has been deleted from PlayerPrefs.");
    }
}