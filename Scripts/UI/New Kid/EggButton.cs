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
    [SerializeField] TMPro.TextMeshProUGUI probText;
    float currentProbability = 0f;


    [Header("���� ���� Ȯ��")]
    [SerializeField] float desiredFontSizeFactor;
    float initFontSize;
    bool isInit;
    float rateToGetRare;

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
        // Slider �� ������Ʈ
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
