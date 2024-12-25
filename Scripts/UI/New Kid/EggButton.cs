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

    [Header("White Flash")]
    [SerializeField] Material whiteMat;
    Material initMat;
    Image image;
    [SerializeField] Image[] gradeTags;
    Coroutine popFeedbackCo;
    bool isPopFeedbackDone;

    [Header("Probability Settings")]
    [SerializeField] private float increaseProbability; // 버튼 클릭시 증가량
    [SerializeField] private float decreaseRate; // 초당 감소량
    [SerializeField] private float maxProbability; // 최대 확률
    float currentProbability = 0f;
    string pastGrade;
    int currentGradeIndex, pastGradeIndex;
    [SerializeField] RectTransform gradeRoll; // 클릭에 따라 grade가 점점 올라가도록
    [SerializeField] RectTransform gradePanel; // 등급이 올라갈 때 스케일을 잠깐 올리도록
    [SerializeField] TMPro.TextMeshProUGUI gradeTitle; // 등급이 올라갈 때 등급 텍스트도 변경
    [SerializeField] Animator gradePanelAnim;
    bool isGradeFixed; // 등급이 결정된 이후에는 다이얼이 더 이상 움직이지 않도록 하려고
    float fixedProbability; // 확정된 등급의 y높이를 프레임에 딱 맞추기 위해서

    [SerializeField] Image nameTag; // 이름표의 색깔을 등급과 맞추기 위해
    
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
        isGradeFixed = false;
        PlayGradePanelAnim("Init");

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
        currentGradeIndex = 0;
    }

    void Update()
    {
        // 확률이 0보다 크고 아직 등급이 결정되지 않았다면 서서히 감소
        if (currentProbability > 0 && isGradeFixed == false)
        {
            currentProbability = Mathf.Max(0f, currentProbability - (decreaseRate * Time.unscaledDeltaTime));
            UpdateGradeTitle();
        }
    }

    public void InitRate()
    {
        ResetCurrentProbability(); // 확률 초기화
        UpdateGradeTitle(); // 초기화된 확률에 대한 등급 패널 초기화
        InitGradeColors();
        popFeedbackCo = null;
        isPopFeedbackDone = true;
        rateToGetRare = 0f;

    }

    // 버튼이 눌러지면 이벤트로 실행
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
    // 버튼이 눌러지면 이벤트로 실행
    public void EggClickedFeedback()
    {
        if (isClicked) return;

        OnButtonClick();

        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        UpdateGradeTitle();
        UpdateRateForGameManager();

        StartCoroutine(EggWhiteFlashCo());
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
            gradeTags[i].color = MyGrade.GradeColors[i];
        }
    }

    void UpdateGradeTitle()
    {
        // Slider 값 업데이트
        // 등급 롤의 위치를 확률에 따라 업데이트
        // 등급 간 거리 = 128, 등급 총 거리 = 512
        // 백분위와의 비율 100:512 = 1 : x
        // 5.12를 곱해줘서 확률을 거리로 변환
        gradeRoll.anchoredPosition = new Vector2(gradeRoll.anchoredPosition.x, currentProbability * 5.12f);
        currentGradeIndex = pastGradeIndex;

        for (int i = 0; i < MyGrade.mGrades.Length; i++)
        {
            if (currentProbability >= i * 25f && currentProbability < (i + 1) * 25f)
            {
                currentGradeIndex = i;
                gradeTitle.text = MyGrade.mGrades[i];

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
            else
            {
                //for (int i = 0; i < upgradeSounds.Length; i++)
                //{
                //    Debug.Log("Down");
                //    SoundManager.instance.Play(downGradeSounds[i]);
                //    PlayGradePanelAnim("Downgrade");
                //}
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
        gradeTags[_gradeIndex].color = MyGrade.GradeColors[_gradeIndex];
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

        if (popFeedbackCo != null) StopCoroutine(popFeedbackCo);

        isGradeFixed = true;

        for (int i = 0; i < MyGrade.mGrades.Length; i++)
        {
            if (currentProbability >= i * 25f && currentProbability < (i + 1) * 25f)
            {
                currentProbability = i * 25f;
                fixedProbability = currentProbability;
                nameTag.color = MyGrade.GradeColors[i];
                break;
            }
        }
        gradeRoll.anchoredPosition = new Vector2(gradeRoll.anchoredPosition.x, currentProbability * 5.12f);
        
        Debug.Log($"currentProb = {currentProbability}, fixedProb = {fixedProbability}");
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

    public int GetWeaponGradeIndex()
    {
        return currentGradeIndex;
    }
}