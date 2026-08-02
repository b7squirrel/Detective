using UnityEngine;
using DG.Tweening;
using TMPro;

public class EquipInfoPanel : MonoBehaviour
{
    // ★ 그림자 쌍이 있는 텍스트는 전부 ShadowedText로 교체
    [SerializeField] ShadowedText grade;
    [SerializeField] ShadowedText Name;
    [SerializeField] ShadowedText Level;
    [SerializeField] GameObject attributeATK;
    [SerializeField] GameObject attributeHP;
    [SerializeField] ShadowedText attributeATKText;
    [SerializeField] ShadowedText attributeHPText;
    [SerializeField] UnityEngine.UI.Image NameLabel;
    [SerializeField] UnityEngine.UI.Image GradeLabel;
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] Animator anim;
    [SerializeField] GameObject equipButton, unEquipButton;
    [SerializeField] GameObject[] itemCardBase;

    CardsDictionary cardsDictionary => CardsDictionary.Instance;
    public CardDisp cardDisp;

    float initLevelFontSize, initAttributeATKFontSize, initAttributeHPFontSize;
    Tween levelPopTween, atkPopTween, hpPopTween;
    Tween levelFlashTween, atkFlashTween, hpFlashTween;
    Color initLevelColor, initAttributeATKColor, initAttributeHPColor;

    // ★ 현재 표시 중인 데이터 저장
    private CardData currentCardData;
    private Item currentItemData;

    void Awake()
    {
        // 초기 폰트 사이즈 저장
        if (Level != null) initLevelFontSize = Level.fontSize;
        if (attributeATKText != null) initAttributeATKFontSize = attributeATKText.fontSize;
        if (attributeHPText != null) initAttributeHPFontSize = attributeHPText.fontSize;

        // ★ 원본 색상 저장 (플래시 후 복귀용)
        if (Level != null && Level.Main != null) initLevelColor = Level.Main.color;
        if (attributeATKText != null && attributeATKText.Main != null) initAttributeATKColor = attributeATKText.Main.color;
        if (attributeHPText != null && attributeHPText.Main != null) initAttributeHPColor = attributeHPText.Main.color;

        // ★ 언어 변경 이벤트 구독
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        // 파괴될 때 Tween 정리
        levelPopTween?.Kill();
        atkPopTween?.Kill();
        hpPopTween?.Kill();
        levelFlashTween?.Kill();
        atkFlashTween?.Kill();
        hpFlashTween?.Kill();

        // ★ 언어 변경 이벤트 구독 해제
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    // ★ 텍스트 업데이트 메서드
    void UpdateText()
    {
        if (currentCardData == null || currentItemData == null) return;

        // 등급
        grade.text = LocalizationManager.Game.gradeNames[currentCardData.Grade];

        // 아이템 이름
        Name.text = LocalizationManager.Item.GetItemDisplayName(currentItemData.Name);

        // 레벨
        Level.text = LocalizationManager.Game.level + " " +
                     currentCardData.Level.ToString() + " / " +
                     StaticValues.MaxLevel.ToString();
    }

    // 처음 패널이 활성화 되면 초기화
    public void SetPanel(CardData cardData, Item itemData, CardDisp _cardDisp, bool isEquipButton, bool isEssential)
    {
        Logger.LogWarning($"item data = {itemData.DisplayName}");

        // ★ 현재 데이터 저장
        currentCardData = cardData;
        currentItemData = itemData;

        this.cardDisp = _cardDisp;

        // ★ 다국어 적용 (ShadowedText.text 가 메인+그림자 동시 반영)
        grade.text = LocalizationManager.Game.gradeNames[cardData.Grade];
        Name.text = LocalizationManager.Item.GetItemDisplayName(itemData.Name);

        // 색상은 그림자와 별개이므로 기존처럼 Image 쪽만 처리 (변경 없음)
        NameLabel.color = MyGrade.GradeColors[cardData.Grade];
        GradeLabel.color = MyGrade.GradeColors[cardData.Grade];
        SetItemCardBase(cardData.Grade);

        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " +
                     cardData.Level.ToString() + " / " +
                     StaticValues.MaxLevel.ToString();


        attributeATK.SetActive(false);
        attributeHP.SetActive(false);
        if (cardData.Atk != 0)
        {
            attributeATK.SetActive(true);
            attributeATKText.text = "+ " + cardData.Atk.ToString();
        }
        if (cardData.Hp != 0)
        {
            attributeHP.SetActive(true);
            attributeHPText.text = "+ " + cardData.Hp.ToString();
        }

        WeaponItemData weaponItemData = cardsDictionary.GetWeaponItemData(cardData);
        itemImage.sprite = weaponItemData.itemData.charImage;
        anim.enabled = false;

        equipButton.SetActive(isEquipButton);

        if (isEssential)
        {
            unEquipButton.SetActive(false);
            return;
        }

        unEquipButton.SetActive(!isEquipButton);

        // 초기 폰트 사이즈 저장 (Awake에서 못 가져온 경우 대비)
        if (initLevelFontSize == 0) initLevelFontSize = Level.fontSize;
        if (initAttributeATKFontSize == 0 && attributeATKText != null) initAttributeATKFontSize = attributeATKText.fontSize;
        if (initAttributeHPFontSize == 0 && attributeHPText != null) initAttributeHPFontSize = attributeHPText.fontSize;

        // 초기 색상 저장 (Awake에서 못 가져온 경우 대비)
        if (initLevelColor.a == 0 && Level.Main != null) initLevelColor = Level.Main.color;
        if (initAttributeATKColor.a == 0 && attributeATKText.Main != null) initAttributeATKColor = attributeATKText.Main.color;
        if (initAttributeHPColor.a == 0 && attributeHPText.Main != null) initAttributeHPColor = attributeHPText.Main.color;

        // 기존 애니메이션 정리
        levelPopTween?.Kill();
        atkPopTween?.Kill();
        hpPopTween?.Kill();
        levelFlashTween?.Kill();
        atkFlashTween?.Kill();
        hpFlashTween?.Kill();

        // 폰트 사이즈 초기화 (메인+그림자 동시 반영)
        Level.fontSize = initLevelFontSize;
        if (attributeATK.activeSelf) attributeATKText.fontSize = initAttributeATKFontSize;
        if (attributeHP.activeSelf) attributeHPText.fontSize = initAttributeHPFontSize;

        // 색상도 원래 값으로 리셋 (직전 플래시가 끝나기 전에 다시 열렸을 경우 대비)
        if (Level.Main != null) Level.Main.color = initLevelColor;
        if (attributeATK.activeSelf && attributeATKText.Main != null) attributeATKText.Main.color = initAttributeATKColor;
        if (attributeHP.activeSelf && attributeHPText.Main != null) attributeHPText.Main.color = initAttributeHPColor;
    }

    // 레벨업을 하면 레벨과 ATK/HP를 업데이트 (둘 다 가진 장비도 대응)
    public void UpdatePanel(int _level, int _atk, int _hp)
    {
        if (currentCardData != null)
        {
            // 레벨 데이터 업데이트
            currentCardData.Level = _level;
        }

        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " +
                     _level.ToString() + " / " +
                     StaticValues.MaxLevel.ToString();

        Logger.Log("Ugraded");

        // 기존 애니메이션 중단
        levelPopTween?.Kill();
        atkPopTween?.Kill();
        hpPopTween?.Kill();
        levelFlashTween?.Kill();
        atkFlashTween?.Kill();
        hpFlashTween?.Kill();

        // 레벨 텍스트 갱신 + 애니메이션 (크기 팝 + 흰색 플래시)
        Level.fontSize = initLevelFontSize;
        levelPopTween = PopFontSizeTween(Level, initLevelFontSize);
        levelFlashTween = FlashWhiteTween(Level, initLevelColor);

        // ATK 텍스트가 활성화된 경우에만 갱신 + 애니메이션
        if (attributeATK.activeSelf)
        {
            attributeATKText.text = "+ " + _atk.ToString();
            attributeATKText.fontSize = initAttributeATKFontSize;
            atkPopTween = PopFontSizeTween(attributeATKText, initAttributeATKFontSize);
            atkFlashTween = FlashWhiteTween(attributeATKText, initAttributeATKColor);
        }

        // HP 텍스트가 활성화된 경우에만 갱신 + 애니메이션
        if (attributeHP.activeSelf)
        {
            attributeHPText.text = "+ " + _hp.ToString();
            attributeHPText.fontSize = initAttributeHPFontSize;
            hpPopTween = PopFontSizeTween(attributeHPText, initAttributeHPFontSize);
            hpFlashTween = FlashWhiteTween(attributeHPText, initAttributeHPColor);
        }
    }

    // ★ ShadowedText 기준으로 변경: fontSize setter를 통해 메인+그림자가 함께 애니메이션됨
    Tween PopFontSizeTween(ShadowedText text, float originalSize)
    {
        float targetSize = originalSize + 12f; // ★ 기존 7f -> 12f로 확대 (조금 더 크게 팝)
        return DOTween.To(() => text.fontSize, x => text.fontSize = x, targetSize, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                text.fontSize = originalSize;
            });
    }

    // ★ 순간적으로 흰색으로 번쩍였다가 원래 색으로 돌아오는 플래시 효과 (메인 텍스트만 대상, 그림자는 검정 유지)
    Tween FlashWhiteTween(ShadowedText text, Color originalColor)
    {
        if (text == null || text.Main == null) return null;

        text.Main.color = originalColor; // 시작 전 확실히 원본 색으로 리셋

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => text.Main.color, c => text.Main.color = c, Color.white, 0.08f));
        seq.Append(DOTween.To(() => text.Main.color, c => text.Main.color = c, originalColor, 0.18f));
        return seq;
    }

    void SetItemCardBase(int _index)
    {
        for (int i = 0; i < itemCardBase.Length; i++)
        {
            if (i == _index)
            {
                itemCardBase[i].SetActive(true);
                continue;
            }
            itemCardBase[i].SetActive(false);
        }
    }
}