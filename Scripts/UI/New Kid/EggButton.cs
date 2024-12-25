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
    [SerializeField] private float increaseProbability; // ��ư Ŭ���� ������
    [SerializeField] private float decreaseRate; // �ʴ� ���ҷ�
    [SerializeField] private float maxProbability; // �ִ� Ȯ��
    float currentProbability = 0f;
    string pastGrade;
    int currentGradeIndex, pastGradeIndex;
    [SerializeField] RectTransform gradeRoll; // Ŭ���� ���� grade�� ���� �ö󰡵���
    [SerializeField] RectTransform gradePanel; // ����� �ö� �� �������� ��� �ø�����
    [SerializeField] TMPro.TextMeshProUGUI gradeTitle; // ����� �ö� �� ��� �ؽ�Ʈ�� ����
    [SerializeField] Animator gradePanelAnim;
    bool isGradeFixed; // ����� ������ ���Ŀ��� ���̾��� �� �̻� �������� �ʵ��� �Ϸ���
    float fixedProbability; // Ȯ���� ����� y���̸� �����ӿ� �� ���߱� ���ؼ�

    [SerializeField] Image nameTag; // �̸�ǥ�� ������ ��ް� ���߱� ����
    
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
        isGradeFixed = false;
        PlayGradePanelAnim("Init");

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
        currentGradeIndex = 0;
    }

    void Update()
    {
        // Ȯ���� 0���� ũ�� ���� ����� �������� �ʾҴٸ� ������ ����
        if (currentProbability > 0 && isGradeFixed == false)
        {
            currentProbability = Mathf.Max(0f, currentProbability - (decreaseRate * Time.unscaledDeltaTime));
            UpdateGradeTitle();
        }
    }

    public void InitRate()
    {
        ResetCurrentProbability(); // Ȯ�� �ʱ�ȭ
        UpdateGradeTitle(); // �ʱ�ȭ�� Ȯ���� ���� ��� �г� �ʱ�ȭ
        InitGradeColors();
        popFeedbackCo = null;
        isPopFeedbackDone = true;
        rateToGetRare = 0f;

    }

    // ��ư�� �������� �̺�Ʈ�� ����
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
    // ��ư�� �������� �̺�Ʈ�� ����
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
        // Slider �� ������Ʈ
        // ��� ���� ��ġ�� Ȯ���� ���� ������Ʈ
        // ��� �� �Ÿ� = 128, ��� �� �Ÿ� = 512
        // ��������� ���� 100:512 = 1 : x
        // 5.12�� �����༭ Ȯ���� �Ÿ��� ��ȯ
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