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
    bool needToOffset;
    WeaponData leadWeaponData; //리드 오리 데이터 저장해서 사용 

    CardSpriteAnim cardSpriteAnim;
    public void InitWeaponIcon(WeaponData wd)
    {
        // ⭐ 초기화 추가
        needToOffset = false;
        headMain.anchoredPosition = Vector2.zero;

        if (leadWeaponData == null) leadWeaponData = GameManager.instance.startingDataContainer.GetLeadWeaponData();

        bool isLead = false;
        if (wd.Name == leadWeaponData.Name)
        {
            InitWeaponCardDisplay(leadWeaponData, null);
            isLead = true;
        }
        else
        {
            InitWeaponCardDisplay(wd, null);
            isLead = false;
        }

        InitSpriteRow(); // card sprite row의 이미지 참조들이 남지 않게 초기화

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
    }
    void InitWeaponCardDisplay(WeaponData weaponData, CardData cardData)
    {
        needToOffset = false;

        // 캐릭터 이미지
        //charImage.sprite = weaponData.charImage;
        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;
        charFaceExpression.gameObject.SetActive(true);
        if (charFaceImage == null) charFaceImage = charFaceExpression.GetComponent<Image>();
        charFaceImage.sprite = weaponData.faceImage;

        // 데이터로 카드를 display할 때가 아닌 경우라면 여기까지만 진행
        if (cardData == null) return;
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

            // needToOffset이 true인 아이템이 있으면 offset 적용
            if (needToOffset && !this.needToOffset)
            {
                this.needToOffset = true;
                headMain.anchoredPosition += offset;
            }

            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow);
        }
    }
    #endregion

    void EmptyCardDisplay()
    {

        // 캐릭터 이미지
        charImage.rectTransform.localScale = .7f * Vector3.one;
        charImage.gameObject.SetActive(false);

        // 장비 이미지
        for (int i = 0; i < 4; i++)
        {
            if (equipmentImages[i] == null)
                continue;
            equipmentImages[i].gameObject.SetActive(false);
        }
    }
}
