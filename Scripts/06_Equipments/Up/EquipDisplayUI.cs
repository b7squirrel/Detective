using UnityEngine;
using DG.Tweening;
using TMPro;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Transform cardBaseContainer; // 5레벨
    [SerializeField] Transform starContainer;
    [SerializeField] Transform ribbon;
    [SerializeField] GameObject halo;
    [SerializeField] GameObject titleRibbon;
    [SerializeField] GameObject titleRibbonShadow;
    [SerializeField] GameObject SkillDescriptionPanel;
    [SerializeField] protected TextMeshProUGUI Title;
    [SerializeField] protected TextMeshProUGUI Level;
    [SerializeField] protected TextMeshProUGUI LevelShadow;
    [SerializeField] protected TextMeshProUGUI SkillName;
    [SerializeField] protected TextMeshProUGUI SkillDescription;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;
    SetCardDataOnSlot setCardDataOnSlot; // 카드 데이터와 슬롯을 넘겨 받아서 슬롯에 카드를 표시

    [SerializeField] TextMeshProUGUI atk, hp;
    CardsDictionary cardsDictionary => CardsDictionary.Instance;
    [SerializeField] GameObject atkLabel, hpLabel;
    [SerializeField] GameObject charButton;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject charUpgradeButton; // 디스플레이되는 오리카드 업그레이드 버튼

    [SerializeField] CanvasGroup charWarningLackCanvasGroup;

    [SerializeField] RectTransform charImage; // Char Disp 하위의 Char Image Transform
    [SerializeField] GameObject whiteFlash;
    [SerializeField] RectTransform charDispRoot; // ⭐ 추가: 최고레벨 팝을 걸 Char Disp 루트
    Tween charPopTween;

    [Header("Debug")]
    [SerializeField] GameObject[] testParts;

    float initAtkFontSize, initHpFontSize;
    Tween atkPopTween, hpPopTween;
    Tween levelMaxPopTween;     // ⭐ 추가: 최고레벨 도달 시 레벨 텍스트 팝 연출
    Tween charDispMaxPopTween;  // ⭐ 추가: 최고레벨 도달 시 Char Disp 전체 팝 연출

    public void SetWeaponDisplay(CardData charCardData, OriAttribute currentAttr, string dispName)
    {
        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);
        transform.gameObject.SetActive(true);

        int intGrade = charCardData.Grade;
        int intEvoStage = charCardData.EvoStage;
        SetNumStar(intEvoStage + 1);

        SkillDescriptionPanel.SetActive(true);

        // 스킬 이름 및 설명 (CharTexts 사용)
        SkillName.text = LocalizationManager.Char.skillNames[charCardData.PassiveSkill - 1];
        SkillName.color = MyGrade.GradeColors[charCardData.Grade];
        SkillDescription.text = LocalizationManager.Char.skillDescriptions[charCardData.PassiveSkill - 1];

        for (int i = 0; i < StaticValues.MaxGrade; i++)
        {
            if (i == intGrade)
            {
                cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);
                ribbon.GetChild(intGrade).gameObject.SetActive(true);
                continue;
            }
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
            ribbon.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 이름 텍스트
        titleRibbon.SetActive(true);
        titleRibbonShadow.SetActive(true);
        Title.text = dispName;
        
        // 카드 레벨 텍스트 (GameTexts 사용)
        Level.text = LocalizationManager.Game.level + " " + charCardData.Level;
        LevelShadow.text = Level.text;

        // 디버그
        if (testParts != null)
        {
            for (int i = 0; i < testParts.Length; i++)
            {
                testParts[i].SetActive(false);
            }
        }

        // 기존 애니메이션 정리
        atkPopTween?.Kill();
        hpPopTween?.Kill();

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();

        // 초기 폰트 사이즈 저장 (한 번만)
        if (initAtkFontSize == 0) initAtkFontSize = atk.fontSize;
        if (initHpFontSize == 0) initHpFontSize = hp.fontSize;

        // 폰트 사이즈 초기화
        atk.fontSize = initAtkFontSize;
        hp.fontSize = initHpFontSize;

        charButton.SetActive(true);
        backButton.SetActive(true);
        charUpgradeButton.SetActive(true);
    }

    public void SetAtkHpStats(int _currentAtk, int _currnetHp)
    {
        atk.text = _currentAtk.ToString();
        hp.text = _currnetHp.ToString();

        // 기존 애니메이션 중단
        atkPopTween?.Kill();
        hpPopTween?.Kill();

        // 초기화 후 애니메이션
        atk.fontSize = initAtkFontSize;
        hp.fontSize = initHpFontSize;

        atkPopTween = PopFontSizeTween(atk, initAtkFontSize);
        hpPopTween = PopFontSizeTween(hp, initHpFontSize);
    }

    Tween PopFontSizeTween(TextMeshProUGUI text, float originalSize)
    {
        float targetSize = originalSize + 12f;
        return DOTween.To(() => text.fontSize, x => text.fontSize = x, targetSize, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                text.fontSize = originalSize;
            });
    }
    
    public void SetLevelUI(CardData cardOnDisplay)
    {
        // GameTexts 사용
        Level.text = LocalizationManager.Game.level + " " + cardOnDisplay.Level;
        LevelShadow.text = Level.text;
    }

    // ⭐ 추가: 최고 레벨 도달 시 레벨 텍스트 팝 연출
    public void PlayMaxLevelPop()
    {
        levelMaxPopTween?.Kill();
        Level.transform.localScale = Vector3.one;
        LevelShadow.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.Append(Level.transform.DOPunchScale(Vector3.one * 0.8f, 0.2f, 6, 1f));
        seq.Join(LevelShadow.transform.DOPunchScale(Vector3.one * 0.8f, 0.5f, 6, 1f));

        levelMaxPopTween = seq;
    }

    // ⭐ 추가: 최고 레벨 도달 시 Char Disp 전체 팝 연출
    // ⭐ 수정: Vector3.one을 기준으로 고정 (캐싱 대신)
    public void PlayCharDispMaxPop()
    {
        if (charDispRoot == null) return;

        charDispMaxPopTween?.Kill();
        charDispRoot.localScale = Vector3.one;

        charDispMaxPopTween = charDispRoot.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 1f);
    }

    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 5개 만들어서 비활성화
            stars = new GameObject[5];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // 등급만큼 별 활성화하고 별리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void OffDisplay()
    {
        // Base Container 비활성화
        cardBaseContainer.gameObject.SetActive(false);

        // 스킬 설명 패널 비활성화
        SkillDescriptionPanel.SetActive(false);

        // 뒤로 가기 버튼 비 활성화
        backButton.SetActive(false);

        // 별 비활성화
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }

        // Halo
        halo.SetActive(false);

        // 카드 레벨 텍스트
        titleRibbon.SetActive(false);
        titleRibbonShadow.SetActive(false);

        Level.text = "";
        LevelShadow.text = "";
        Title.text = "";

        atkLabel.SetActive(false);
        hpLabel.SetActive(false);

        charButton.SetActive(false);
        charUpgradeButton.SetActive(false);

        // Tween 정리
        atkPopTween?.Kill();
        hpPopTween?.Kill();

        charPopTween?.Kill();
        charImage.localScale = Vector3.one;

        // ⭐ 추가: 최고레벨 팝 연출 정리 (패널을 나갈 때 찌그러진 채로 남지 않도록)
        levelMaxPopTween?.Kill();
        Level.transform.localScale = Vector3.one;
        LevelShadow.transform.localScale = Vector3.one;

        charDispMaxPopTween?.Kill();
        if (charDispRoot != null)
            charDispRoot.localScale = Vector3.one; // ⭐ 수정: charDispOriginalScale → Vector3.one

        whiteFlash.SetActive(false);

        // 폰트 사이즈 초기화
        if (initAtkFontSize > 0) atk.fontSize = initAtkFontSize;
        if (initHpFontSize > 0) hp.fontSize = initHpFontSize;

        GetComponentInParent<EquipmentPanelManager>().TempKillAllTweens();
        charWarningLackCanvasGroup.gameObject.SetActive(false);
    }

    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        charUpgradeButton.SetActive(true);

        charWarningLackCanvasGroup.gameObject.SetActive(true);
    }

    public void PopCharImage()
    {
        charPopTween?.Kill();
        charImage.localScale = Vector3.one;
        charImage.localPosition = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.OnStart(() => whiteFlash.SetActive(true));  // 시작 시 활성화

        seq.Append(charImage.DOScale(new Vector3(1.1f, 0.75f, 1f), 0.07f).SetEase(Ease.InSine));
        seq.Append(charImage.DOScale(new Vector3(0.9f, 1.2f, 1f), 0.09f).SetEase(Ease.OutExpo));
        RectTransform charRect = charImage as RectTransform;
        seq.Join(charRect.DOAnchorPosY(30f, 0.09f).SetEase(Ease.OutExpo));
        seq.Append(charImage.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutExpo));
        seq.Join(charRect.DOAnchorPosY(0f, 0.1f).SetEase(Ease.OutExpo));

        seq.OnComplete(() => whiteFlash.SetActive(false)); // 끝날 때 비활성화

        charPopTween = seq;
    }
    public void UnEquipCharImage()
    {
        charPopTween?.Kill();
        charImage.localScale = Vector3.one;
        charImage.localPosition = Vector3.zero;

        RectTransform charRect = charImage as RectTransform;

        Sequence seq = DOTween.Sequence();
        // 세로로 줄고 가로로 늘어남 (스쿼시)
        seq.Append(charImage.DOScale(new Vector3(1.2f, 0.2f, 1f), 0.1f).SetEase(Ease.InSine));
        
        
        seq.Join(charRect.DOAnchorPosY(-30f, 0.09f).SetEase(Ease.OutExpo));

        // 원래대로 복귀
        seq.Append(charImage.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutExpo));

        seq.Join(charRect.DOAnchorPosY(0f, 0.1f).SetEase(Ease.OutExpo));

        charPopTween = seq;
    }
}