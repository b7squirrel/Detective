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
        //DeleteSoundState(); // 최초 게임 실행 상황 재현을 위해
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
        // 사운드, 음악, 옵션 사운드 버튼 이미지 셋업
        soundManager.SetState(_soundState);
        musicManager.SetState(_soundState);

        buttonToggle.SetImage(_soundState);
    }
    public void SetSouindState()
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