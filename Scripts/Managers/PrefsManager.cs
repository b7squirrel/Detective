using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] ButtonToggle buttonToggle;
    SoundManager soundManager;
    MusicManager musicManager;
    bool soundState;

    private void Start()
    {
        //DeleteSoundState(); // 개발 중 초기화 상태 확인용 (사용자 설정 삭제)
        Load();

        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();
        soundManager.Init();
        InitSoundState(soundState);
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
    }
    void InitSoundState(bool _soundState)
    {
        // 사운드 매니저, 음악 매니저, 옵션 토글 버튼에 상태 반영
        soundManager.SetState(_soundState);
        musicManager.SetState(_soundState);

        buttonToggle.SetImage(_soundState); // 버튼 이미지도 상태에 맞게 설정
    }
    public void SetSouindState() // Toggle 버튼을 눌렀을 때 호출됨
    {
        soundState = !soundState;
        InitSoundState(soundState);
        Save();
    }

    void DeleteSoundState()
    {
        PlayerPrefs.DeleteKey("SoundState");
        Debug.Log("SoundState has been deleted from PlayerPrefs.");
    }
}