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
    [SerializeField] private float increaseProbability; // ��ư Ŭ���� ������
    [SerializeField] private float decreaseRate; // �ʴ� ���ҷ�
    [SerializeField] private float maxProbability; // �ִ� Ȯ��
    [SerializeField] Slider probSlider;
    float currentProbability = 0f;


    [Header("���� ���� Ȯ��")]
    [SerializeField] TMPro.TextMeshProUGUI textInteger;
    [SerializeField] TMPro.TextMeshProUGUI textDecimal;
    [SerializeField] float desiredFontSizeFactor;
    [SerializeField] float increment; // Ŭ���� ������ Ȯ�� ������
    float prevIntegerPart, prevDecimalPart;
    float integerPart, decimalPart;
    float initFontSize;
    bool isInit;
    float rateToGetRare;
    float previousRate, currentRate;

    bool isClicked; // �ʹ� �������� Ŭ���Ǵ� ���� ���� ����

    Animator anim;

    bool pickedEgg;

    void OnEnable()
    {
        pickedEgg = false;
        isClicked = false;

        if (image == null) image = GetComponent<Image>();
        if (initMat == null) initMat = image.material;
        image.material = initMat; // whiteMat�� ����� ���·� �������� �ʱ� ����

        if (isInit == false)
        {
            initFontSize = textInteger.fontSize;
            isInit = true;
        }
    }
    void OnButtonClick()
    {
        // Ȯ�� ����
        currentProbability = Mathf.Min(currentProbability + increaseProbability, maxProbability);
    }
    void ResetCurrentProbability()
    {
        currentProbability = 0f;
    }

    void Update()
    {
        // Ȯ���� 0���� ũ�� ������ ����
        if (currentProbability > 0)
        {
            currentProbability = Mathf.Max(0f, currentProbability - (decreaseRate * Time.unscaledDeltaTime));
            UpdateText();
            UpdateSlider();
        }
    }

    public void InitRate()
    {
        ResetCurrentProbability();
        rateToGetRare = 0f;
        integerPart = 0;
        decimalPart = 0;
        prevDecimalPart = 0;
        prevIntegerPart = 0;
        textInteger.text = integerPart.ToString();
        textDecimal.text = decimalPart.ToString().Substring(1) + "%";

        if (probSlider != null)
        {
            probSlider.minValue = 0f;
            probSlider.maxValue = maxProbability;
            probSlider.value = 0f;

            // ���� ����� �����Ǿ����� �α׷� Ȯ��
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
        if (textInteger.gameObject.activeSelf == false)
            return;
        StartCoroutine(UpTextCo(_integerUp));
    }
    IEnumerator UpTextCo(bool _integerUp)
    {
        if (_integerUp) textInteger.fontSize *= desiredFontSizeFactor;
        UpdateText();

        yield return new WaitForSecondsRealtime(.06f);

        textInteger.fontSize = initFontSize;
    }

    void UpdateText()
    {
        textInteger.text = Mathf.FloorToInt(currentProbability).ToString();
    }
    void  UpdateSlider()
    {
        // Slider �� ������Ʈ
        if (probSlider != null)
        {
            probSlider.value = Mathf.FloorToInt(currentProbability);
        }
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
