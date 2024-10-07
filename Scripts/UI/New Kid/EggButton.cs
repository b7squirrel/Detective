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

    [Header("레어 오리 확률")]
    [SerializeField] TMPro.TextMeshProUGUI textInteger;
    [SerializeField] TMPro.TextMeshProUGUI textDecimal;
    [SerializeField] float desiredFontSizeFactor;
    [SerializeField] float increment; // 클릭할 때마다 확률 증가분
    float prevIntegerPart, prevDecimalPart;
    float integerPart, decimalPart;
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
            initFontSize = textInteger.fontSize;
            isInit = true;
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
        float offset = UnityEngine.Random.Range(-increment / 3f, increment / 3f);
        increment += offset;
        rateToGetRare += increment;
        rateToGetRare = Mathf.Round(rateToGetRare * 100f) / 100f;
        integerPart = Mathf.FloorToInt(rateToGetRare);
        Debug.Log("Integer Part = " + integerPart);
        decimalPart = rateToGetRare - integerPart;
        decimalPart = Mathf.Round(decimalPart * 100) / 100f;

        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        UpText(rateToGetRare);

        UpdateRateForGameManager();

        StartCoroutine(WhiteFlashCo());
    }

    IEnumerator WhiteFlashCo()
    {
        isClicked = true;
        image.material = whiteMat;
        Debug.Log("Meterial = " + image.material.name);
        yield return new WaitForSecondsRealtime(.05f);
        isClicked = false;
        image.material = initMat;
    }
    public void UpText(float _rate)
    {
        if (textInteger.gameObject.activeSelf == false)
            return;
        StartCoroutine(UpTextCo(_rate));
    }
    IEnumerator UpTextCo(float _rate)
    {
        textInteger.fontSize *= desiredFontSizeFactor;
        textInteger.text = integerPart.ToString();
        textDecimal.text = decimalPart.ToString().Substring(1) + "%";

        yield return new WaitForSecondsRealtime(.06f);

        textInteger.fontSize = initFontSize;
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
