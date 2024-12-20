using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EggButton : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] AudioClip pickEggSound;
    [SerializeField] AudioClip breakEggSound;
    [SerializeField] AudioClip eggClickSound;

    [Header("White Flash")]
    [SerializeField] Material whiteMat;
    Material initMat;
    Image image;

    [Header("Probability Settings")]
    [SerializeField] private float increaseProbability; // 버튼 클릭시 증가량
    [SerializeField] private float decreaseRate; // 초당 감소량
    [SerializeField] private float maxProbability; // 최대 확률
    [SerializeField] Slider probSlider;
    [SerializeField] TMPro.TextMeshProUGUI probText;
    float currentProbability = 0f;


    [Header("레어 오리 확률")]
    [SerializeField] float desiredFontSizeFactor;
    float initFontSize;
    bool isInit;
    float rateToGetRare;

    bool isClicked; // 너무 연속으로 클릭되는 것을 막기 위해

    Animator anim;

    bool pickedEgg;

    void OnEnable()
    {
        pickedEgg = false;
        isClicked = false;

        if (image == null) image = GetComponent<Image>();
        if (initMat == null) initMat = image.material;
        image.material = initMat; // whiteMat이 적용된 상태로 시작하지 않기 위해

        if (isInit == false)
        {
            isInit = true;
        }
    }
    void OnButtonClick()
    {
        // 확률 증가
        currentProbability = Mathf.Min(currentProbability + increaseProbability, maxProbability);
    }
    void ResetCurrentProbability()
    {
        currentProbability = 0f;
    }

    void Update()
    {
        // 확률이 0보다 크면 서서히 감소
        if (currentProbability > 0)
        {
            currentProbability = Mathf.Max(0f, currentProbability - (decreaseRate * Time.unscaledDeltaTime));
            UpdateSlider();
            UpdateProbText();
        }
    }

    public void InitRate()
    {
        ResetCurrentProbability();
        probSlider.value = 0f;
        initFontSize = probText.fontSize;

        rateToGetRare = 0f;

        if (probSlider != null)
        {
            probSlider.minValue = 0f;
            probSlider.maxValue = maxProbability;
            probSlider.value = 0f;

            // 값이 제대로 설정되었는지 로그로 확인
            Debug.Log($"Slider initialized - Min: {probSlider.minValue}, Max: {probSlider.maxValue}, Current: {probSlider.value}");
        }
    }
    public void PlayEggClickSound()
    {
        if (pickedEgg)
        {
            SoundManager.instance.Play(breakEggSound);
        }
        else
        {
            pickedEgg = true;
            SoundManager.instance.Play(pickEggSound);
        }
    }
    public void EggClickedFeedback()
    {
        if (isClicked) return;

        OnButtonClick();

        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        UpText(true);
        UpdateSlider();
        UpdateProbText();
        UpdateRateForGameManager();

        StartCoroutine(WhiteFlashCo());
    }

    IEnumerator WhiteFlashCo()
    {
        isClicked = true;
        image.material = whiteMat;
        yield return new WaitForSecondsRealtime(.05f);
        isClicked = false;
        image.material = initMat;
    }
    public void UpText(bool _integerUp)
    {
        StartCoroutine(UpTextCo(_integerUp));
    }
    IEnumerator UpTextCo(bool _integerUp)
    {
        if (_integerUp) probText.fontSize *= desiredFontSizeFactor;
        UpdateProbText();

        yield return new WaitForSecondsRealtime(.06f);

        probText.fontSize = initFontSize;
    }

    void  UpdateSlider()
    {
        // Slider 값 업데이트
        if (probSlider != null)
        {
            probSlider.value = Mathf.FloorToInt(currentProbability);
        }
    }
    void UpdateProbText()
    {
        probText.text = Mathf.FloorToInt(currentProbability).ToString() + "%";
    }

    void UpdateRateForGameManager()
    {
        GameManager.instance.SetRateToGetRare(rateToGetRare);
    }

    public void ChangeEggImage()
    {

    }
    public void GetEggStats()
    {

    }
}
