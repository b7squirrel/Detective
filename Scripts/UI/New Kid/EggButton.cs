using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EggButton : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] AudioClip pickEggSound;
    [SerializeField] AudioClip breakEggSound;
    [SerializeField] AudioClip eggClickSound;
    [SerializeField] AudioClip[] upgradeSounds;
    [SerializeField] AudioClip[] downGradeSounds;
    [SerializeField] AudioClip gradeFixedSound;

    [Header("Hand")]
    [SerializeField] GameObject handSL;

    [Header("White Flash")]
    [SerializeField] Material whiteMat;
    Coroutine whiteFlashCo;
    Material initMat;
    Image image;
    [SerializeField] Image[] gradeTags; // Inspector에서 3개로 조정
    Coroutine popFeedbackCo;
    bool isPopFeedbackDone;

    [Header("Probability Settings")]
    [SerializeField] private float increaseProbability;
    [SerializeField] private float decreaseRate;
    [SerializeField] private float maxProbability;
    float currentProbability = 0f;
    string pastGrade;
    int currentGradeIndex, pastGradeIndex;
    [SerializeField] RectTransform gradeRoll;
    [SerializeField] RectTransform gradePanel;
    [SerializeField] TMPro.TextMeshProUGUI gradeTitle;
    [SerializeField] Animator gradePanelAnim;
    bool isGradeFixed;
    float fixedProbability;

    [SerializeField] Image nameTag;

    bool isMaxGradeReached; // 정예 도달 여부
    [SerializeField] AudioClip maxGradeSound; // 정예 도달 사운드

    [Header("Grade Colors")]
    [SerializeField]
    Color[] eggGradeColors = new Color[]
{
    new Color(0.91f, 0.52f, 0.29f), // 신병 - 주황
    new Color(0.91f, 0.29f, 0.50f), // 고참 - 핫핑크
    new Color(0.91f, 0.78f, 0.29f), // 정예 - 금색
};

    [Header("레어 오리 확률")]
    [SerializeField] float desiredFontSizeFactor;
    float initFontSize;
    bool isInit;
    float rateToGetRare;

    bool isClicked;
    Animator anim;
    bool pickedEgg;

    void OnEnable()
    {
        pickedEgg = false;
        isClicked = false;

        if (image == null) image = GetComponent<Image>();
        if (initMat == null) initMat = image.material;
        image.material = initMat;
        isGradeFixed = false;
        PlayGradePanelAnim("Init");

        if (isInit == false)
        {
            isInit = true;
        }
    }

    void OnButtonClick()
    {
        currentProbability = Mathf.Min(currentProbability + increaseProbability, maxProbability);
    }

    void ResetCurrentProbability()
    {
        currentProbability = 0f;
        currentGradeIndex = 0;
    }

    void Update()
    {
        if (currentProbability > 0 && isGradeFixed == false)
        {
            // 정예(최고 등급) 도달 시 감소하지 않음
            if (currentGradeIndex >= MyGrade.EggGrades.Length - 1)
            {
                if (!isMaxGradeReached)
                {
                    isMaxGradeReached = true;
                    SoundManager.instance.Play(maxGradeSound);
                }
                return;
            }

            currentProbability = Mathf.Max(0f, currentProbability - (decreaseRate * Time.unscaledDeltaTime));
            UpdateGradeTitle();
        }
    }

    public void InitRate()
    {
        ResetCurrentProbability();
        UpdateGradeTitle();
        InitGradeColors();
        popFeedbackCo = null;
        isPopFeedbackDone = true;
        rateToGetRare = 0f;
        isMaxGradeReached = false;
        if (handSL != null) handSL.SetActive(true);
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
        if (isClicked || isGradeFixed) return;

        OnButtonClick();

        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        UpdateGradeTitle();
        UpdateRateForGameManager();

        // 3단계(정예) 도달 상태에서 클릭 시 피드백 반복 재생
        if (currentGradeIndex == MyGrade.EggGrades.Length - 1)
        {
            for (int i = 0; i < upgradeSounds.Length; i++)
            {
                SoundManager.instance.Play(upgradeSounds[i]);
            }
            PlayGradePanelAnim("Upgrade");

            if (isPopFeedbackDone)
            {
                popFeedbackCo = StartCoroutine(PopFeedbackCo(currentGradeIndex));
            }
        }

        if (whiteFlashCo != null) StopCoroutine(whiteFlashCo);
        whiteFlashCo = StartCoroutine(EggWhiteFlashCo());
    }

    IEnumerator EggWhiteFlashCo()
    {
        isClicked = true;
        image.material = whiteMat;
        yield return new WaitForSecondsRealtime(.05f);
        isClicked = false;
        image.material = initMat;
    }

    void InitGradeColors()
    {
        for (int i = 0; i < gradeTags.Length; i++)
        {
            gradeTags[i].color = eggGradeColors[i];
        }
    }

    void UpdateGradeTitle()
    {
        // 3번째 등급(index 2) 도달 시 롤을 더 이상 올리지 않음
        float clampedProbability = Mathf.Min(currentProbability, 2 * 25f);
        gradeRoll.anchoredPosition = new Vector2(gradeRoll.anchoredPosition.x, clampedProbability * 5.12f);

        currentGradeIndex = pastGradeIndex;

        for (int i = 0; i < MyGrade.EggGrades.Length; i++)
        {
            if (currentProbability >= i * 25f && currentProbability < (i + 1) * 25f)
            {
                currentGradeIndex = i;
                gradeTitle.text = MyGrade.EggGrades[i];
                break;
            }
        }

        if (currentGradeIndex != pastGradeIndex)
        {
            if (currentGradeIndex > pastGradeIndex)
            {
                for (int i = 0; i < upgradeSounds.Length; i++)
                {
                    SoundManager.instance.Play(upgradeSounds[i]);
                    PlayGradePanelAnim("Upgrade");
                }
                if (isPopFeedbackDone)
                {
                    popFeedbackCo = StartCoroutine(PopFeedbackCo(currentGradeIndex));
                }
            }
        }

        pastGradeIndex = currentGradeIndex;
    }

    IEnumerator PopFeedbackCo(int _gradeIndex)
    {
        isPopFeedbackDone = false;
        gradeTags[_gradeIndex].color = new Color(1, 1, 1, 1);

        float defaultFontSize = gradeTitle.fontSize;
        gradeTitle.fontSize = defaultFontSize * 1.2f;

        yield return new WaitForSecondsRealtime(.04f);
        gradeTags[_gradeIndex].color = eggGradeColors[_gradeIndex];
        gradeTitle.fontSize = defaultFontSize;

        isPopFeedbackDone = true;
    }

    void PlayGradePanelAnim(string _triggerParameter)
    {
        gradePanelAnim.SetTrigger(_triggerParameter);
    }

    public void PlayGradePanelFixedAnim()
    {
        PlayGradePanelAnim("Fixed");

        if (whiteFlashCo != null)
        {
            StopCoroutine(whiteFlashCo);
            whiteFlashCo = null;
        }

        image.material = initMat;
        isGradeFixed = true;

        if (handSL != null) handSL.SetActive(false);

        for (int i = 0; i < MyGrade.EggGrades.Length; i++)
        {
            if (currentProbability >= i * 25f && currentProbability < (i + 1) * 25f)
            {
                currentProbability = i * 25f;
                fixedProbability = currentProbability;
                nameTag.color = eggGradeColors[i]; // ← 여기도
                break;
            }
        }
        gradeRoll.anchoredPosition = new Vector2(gradeRoll.anchoredPosition.x, currentProbability * 5.12f);
    }

    void UpdateRateForGameManager()
    {
        GameManager.instance.SetRateToGetRare(rateToGetRare);
    }

    public void ChangeEggImage() { }
    public void GetEggStats() { }

    public int GetWeaponGradeIndex()
    {
        return currentGradeIndex;
    }
}