using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelWeaponIcon : MonoBehaviour
{
    [SerializeField] protected Image charImage;
    [SerializeField] protected Image charFaceImage;
    [SerializeField] protected GameObject charFaceExpression;
    [SerializeField] Animator charAnim;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] RectTransform headMain;
    [SerializeField] CanvasGroup iconCanvasGroup;
    bool needToOffset;
    WeaponData leadWeaponData;

    CardSpriteAnim cardSpriteAnim;
    Coroutine revealRoutine;

    public void InitWeaponIcon(WeaponData wd)
    {
        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;

        // 이전 카드 세팅 도중이었다면 중복 실행 방지
        if (revealRoutine != null) StopCoroutine(revealRoutine);

        needToOffset = false;
        headMain.anchoredPosition = Vector2.zero;

        if (leadWeaponData == null) leadWeaponData = GameManager.instance.startingDataContainer.GetLeadWeaponData();

        bool isLead = wd.Name == leadWeaponData.Name;
        WeaponData dataToShow = isLead ? leadWeaponData : wd;

        // ⭐ 컨트롤러 교체까지만 즉시 진행 (Rebind는 아직 안 함)
        SetWeaponVisualData(dataToShow);

        InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item = isLead ? GameManager.instance.startingDataContainer.GetItemDatas()[i] : wd.defaultItems[i];

            if (item == null)
            {
                SetEquipCardDisplay(i, null, false, Vector2.zero);
                continue;
            }

            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;
            SetEquipCardDisplay(i, equipmentSpriteRow, item.needToOffset, offset);
        }

        revealRoutine = StartCoroutine(RebindThenReveal());
    }

    void SetWeaponVisualData(WeaponData weaponData)
    {
        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;

        charFaceExpression.gameObject.SetActive(true);
        if (charFaceImage == null) charFaceImage = charFaceExpression.GetComponent<Image>();
        charFaceImage.sprite = weaponData.faceImage;
    }

    IEnumerator RebindThenReveal()
    {
        yield return null; // ⭐ 카드(부모) 활성화 직후 Animator가 완전히 준비될 시간을 줌

        charAnim.Rebind();
        charAnim.Update(0f);

        yield return null; // ⭐ Rebind 결과가 실제로 트랜스폼에 반영되고 안정될 시간

        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 1f;
        revealRoutine = null;
    }

    #region Card Sprite Anim 참조
    void InitSpriteRow()
    {
        if (cardSpriteAnim == null) cardSpriteAnim = GetComponentInChildren<CardSpriteAnim>();
        cardSpriteAnim.Init(equipmentImages);
    }
    void SetEquipCardDisplay(int index, SpriteRow spriteRow, bool needToOffset, Vector2 offset)
    {
        if (spriteRow == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
        }
        else
        {
            equipmentImages[index].gameObject.SetActive(true);

            if (needToOffset && !this.needToOffset)
            {
                this.needToOffset = true;
                headMain.anchoredPosition += offset;
            }

            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow);
        }
    }
    #endregion

    public void HideInstant()
    {
        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
    }
}