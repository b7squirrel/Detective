using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마스터 볼륨을 UI 슬라이더로 제어하고 PlayerPrefs에 저장/불러오기 하는 매니저.
/// AudioListener.volume은 static이라 씬에 있는 모든 소리에 곱연산으로 적용됨.
/// </summary>
public class VolumeManager : MonoBehaviour
{
    public static VolumeManager instance;

    [Header("UI 연결")]
    [SerializeField] Slider volumeSlider;

    [Header("저장 키")]
    [SerializeField] string prefsKey = "MasterVolume";

    [Header("기본값")]
    [SerializeField] float defaultVolume = 1f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
#if DEV_VER
        // DEV / TEST 빌드 : 슬라이더로 마스터 볼륨 조절 + PlayerPrefs 저장
        float savedVolume = PlayerPrefs.GetFloat(prefsKey, defaultVolume);
        ApplyVolume(savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.gameObject.SetActive(true);
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        else
        {
            Logger.LogWarning("VolumeManager: volumeSlider가 연결되지 않았습니다.");
        }
#else
        // REAL 빌드 : 슬라이더 무시, 볼륨 항상 1 고정. on/off는 MusicManager/SoundManager의 SetState로만 제어.
        ApplyVolume(1f);
        if (volumeSlider != null)
        {
            volumeSlider.gameObject.SetActive(false);
        }
#endif
    }

    void OnSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(prefsKey, value);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
        if (instance == this)
        {
            instance = null;
        }
    }
}