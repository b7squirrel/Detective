using UnityEngine;

public class PrefsManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] ButtonToggle buttonToggle;
    SoundManager soundManager;
    MusicManager musicManager;
    bool soundState;

    // ������ ������ �� ���� mute�� ä�� ������ ��, �����͸� Ȯ���ؼ� mute���� ���� ����
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
        // ����, ����, �ɼ� ���� ��ư �̹��� �¾�
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