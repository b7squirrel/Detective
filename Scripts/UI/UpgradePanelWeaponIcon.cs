using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] RectTransform chestGroup;
    [SerializeField] RectTransform faceGroup;
    [SerializeField] RectTransform handGroup;
    [SerializeField] CanvasGroup iconCanvasGroup;

    WeaponData leadWeaponData;
    CardSpriteAnim cardSpriteAnim;
    Coroutine revealRoutine;

    struct TransformSnapshot
    {
        public Vector2 anchoredPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }
    RectTransform[] resetTargets;
    TransformSnapshot[] defaultSnapshots;

    // 이번에 적용해야 할 head offset을 모아뒀다가 Rebind 이후에 한 번에 적용
    Vector2 pendingHeadOffset;

    void Awake()
    {
        resetTargets = new[] { headMain, chestGroup, faceGroup, handGroup };
        defaultSnapshots = new TransformSnapshot[resetTargets.Length];
        for (int i = 0; i < resetTargets.Length; i++)
        {
            if (resetTargets[i] == null) continue;
            defaultSnapshots[i] = new TransformSnapshot
            {
                anchoredPosition = resetTargets[i].anchoredPosition,
                localRotation = resetTargets[i].localRotation,
                localScale = resetTargets[i].localScale
            };
        }
    }

    void ResetGroupsToDefault()
    {
        for (int i = 0; i < resetTargets.Length; i++)
        {
            if (resetTargets[i] == null) continue;
            resetTargets[i].anchoredPosition = defaultSnapshots[i].anchoredPosition;
            resetTargets[i].localRotation = defaultSnapshots[i].localRotation;
            resetTargets[i].localScale = defaultSnapshots[i].localScale;
        }
    }

    public void InitWeaponIcon(WeaponData wd)
    {
        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
        if (revealRoutine != null) StopCoroutine(revealRoutine);

        ResetGroupsToDefault(); // 화면에 안 보이는 동안 우선 정상값으로 (최종 확정은 Rebind 이후)
        pendingHeadOffset = Vector2.zero;

        if (leadWeaponData == null) leadWeaponData = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        bool isLead = wd.Name == leadWeaponData.Name;
        WeaponData dataToShow = isLead ? leadWeaponData : wd;

        // ★ 동료라면 로비 스쿼드에서 실제 장착한 아이템을 이름으로 조회
        // (같은 종 중복 편성은 GetSquadWeaponNames()로 이미 차단되어 있어 이름 매칭으로 충분)
        List<Item> companionEquippedItems = null;
        if (!isLead)
        {
            var companions = GameManager.instance.startingDataContainer.GetCompanions();
            foreach (var companion in companions)
            {
                if (companion.weaponData != null && companion.weaponData.Name == wd.Name)
                {
                    companionEquippedItems = companion.equippedItems;
                    break;
                }
            }
        }

        SetWeaponVisualData(dataToShow);
        InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item;
            if (isLead)
                item = GameManager.instance.startingDataContainer.GetItemDatas()[i];
            else if (companionEquippedItems != null && i < companionEquippedItems.Count)
                item = companionEquippedItems[i];   // 실제 장착 아이템
            else
                item = wd.defaultItems[i];          // 폴백: 필드에서 얻은 일반 동료(알 등)

            if (item == null)
            {
                SetEquipCardDisplay(i, null);
                continue;
            }
            SetEquipCardDisplay(i, item.spriteRow);
            if (item.needToOffset) pendingHeadOffset += item.posHead;
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
        yield return null;
        charAnim.Rebind();
        charAnim.Update(0f);
        yield return null;

        // 권위 있는 최종 리셋: Animator 재평가가 모두 끝난 뒤 확정
        ResetGroupsToDefault();
        headMain.anchoredPosition += pendingHeadOffset;

        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 1f;
        revealRoutine = null;
    }

    #region Card Sprite Anim 참조
    void InitSpriteRow()
    {
        if (cardSpriteAnim == null) cardSpriteAnim = GetComponentInChildren<CardSpriteAnim>();
        cardSpriteAnim.Init(equipmentImages);
    }

    void SetEquipCardDisplay(int index, SpriteRow spriteRow)
    {
        if (spriteRow == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
        }
        else
        {
            equipmentImages[index].gameObject.SetActive(true);
            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow);
        }
    }
    #endregion

    public void HideInstant()
    {
        if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
    }
}