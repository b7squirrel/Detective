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
    private float currentProbability = 0f;

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
            Debug.Log("Current Probability = " + currentProbability);
            Debug.Log("Time.deltaTime = " + Time.deltaTime);
            UpdateText();
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
        Debug.Log("InitRate");
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
        //float offset = UnityEngine.Random.Range(-increment / 3f, increment / 3f);
        //increment += offset;
        //rateToGetRare += increment;
        //rateToGetRare = Mathf.Round(rateToGetRare * 100f) / 100f;
        //integerPart = Mathf.FloorToInt(rateToGetRare);

        //decimalPart = rateToGetRare - integerPart;
        //decimalPart = Mathf.Round(decimalPart * 100) / 100f;

        OnButtonClick();

        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        // ���� �κ��� ��ȭ�� ���� ���� �����Ͼ�
        //if (integerPart > prevIntegerPart) UpText(true);
        UpText(true);
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
        //textInteger.text = integerPart.ToString();
        //textDecimal.text = decimalPart.ToString().Substring(1) + "%";

        yield return new WaitForSecondsRealtime(.06f);

        textInteger.fontSize = initFontSize;
    }

    void UpdateText()
    {
        textInteger.text = Mathf.FloorToInt(currentProbability).ToString();
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
