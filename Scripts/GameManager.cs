using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public Character character;
    public PoolManager poolManager;
    public EggPanelManager eggPanelManager;
    public BossWarningPanel bossWarningPanel;
    public PauseManager pauseManager;
    public FieldItemSpawner fieldItemSpawner;
    public PlayerRegion playerRegion;
    public FeedbackManager feedbackManager;
    public FieldItemEffect fieldItemEffect;
    public PopupManager popupManager;
    public ProgressionBar progressionBar;
    

    public GameObject joystick;

    public MusicCreditManager musicCreditManager;

    public bool IsPlayerDead { get; set; }
    public bool IsPlayerInvincible { get; set; }
    public bool IsPlayerItemInvincible { get; set; }
    public bool IsPaused { get; private set; }


    [SerializeField] Camera currentCamera;
    public Collider2D cameraBoundary;

    public RectTransform CoinUIPosition;
    public StartingDataContainer startingDataContainer { get; private set; }
    public PlayerDataManager stageManager { get; private set; }

    [Header("Resource Tracker")]
    public GemManager GemManager;
    public KillManager KillManager;

    [Header("Confirmation Button")]
    [SerializeField] GameObject confimationButton;
    
    [Header("BG")]
    public GameObject darkBG;
    public GameObject lightBG;

    [Header("레어 오리 확률")]
    float rateToGetRare;

    [Header("Loading Swipe")]
    [SerializeField] Animator loadingSwipeAnim;

    #region Unity CallBack Functions
    void Awake()
    {
        instance = this;
        currentCamera = Camera.main;
        startingDataContainer = FindObjectOfType<StartingDataContainer>();
        stageManager = FindObjectOfType<PlayerDataManager>();

        GemManager = GetComponent<GemManager>();
        KillManager = GetComponent<KillManager>();

        bossWarningPanel = GetComponent<BossWarningPanel>();
        pauseManager = GetComponent<PauseManager>();

        fieldItemSpawner = FindObjectOfType<FieldItemSpawner>();
        playerRegion = GetComponent<PlayerRegion>();

        musicCreditManager = GetComponent<MusicCreditManager>();

        feedbackManager = GetComponent<FeedbackManager>();

        popupManager = GetComponent<PopupManager>();

        progressionBar = FindObjectOfType<ProgressionBar>();

        confimationButton.SetActive(false);

        OpenLoadingSwipe();
    }
    #endregion

    void OpenLoadingSwipe()
    {
        if (loadingSwipeAnim.gameObject.activeSelf == false)
            loadingSwipeAnim.gameObject.SetActive(true);
        loadingSwipeAnim.SetTrigger("Open");
    }
    public void SetPlayerDead()
    {
        IsPlayerDead = true;
    }
    public void SetPauseState(bool state)
    {
        IsPaused = state;
    }
    public void DestroyStartingData()
    {
        startingDataContainer.DestroyStartingDataContainer();
    }

    public float GetRateToGetRare()
    {
        return rateToGetRare;
    }
    public void SetRateToGetRare(float _rate)
    {
        rateToGetRare = _rate;
    }

    #region Option Input
    public void OnQuitGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Application.Quit();
        }
    }
    //public void OnResetGame(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //    }
    //}
    #endregion

    #region Camera Shake
    public void ShakeCam(float _duration, float _magnitude)
    {
        StartCoroutine(ShakeCamCo(_duration, _magnitude));
    }
    IEnumerator ShakeCamCo(float duration, float magnitude)
    {
        Vector3 originalPos = currentCamera.transform.position;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            currentCamera.transform.position += new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentCamera.transform.position = originalPos;
    }
    #endregion

    #region 확인 버튼
    public void ActivateConfirmationButton(float _delayToActivate)
    {
        StartCoroutine(ActivateButton(_delayToActivate));
    }
    IEnumerator ActivateButton(float _delayToActivate)
    {
        // 일단은 0.8초 후에 나오도록 함.
        yield return new WaitForSecondsRealtime(_delayToActivate);
        confimationButton.SetActive(true);
    }
    #endregion
}
