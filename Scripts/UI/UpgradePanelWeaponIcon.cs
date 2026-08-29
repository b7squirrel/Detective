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

    // ★ 이번에 적용해야 할 offset을 "즉시 적용"하지 않고 보관해뒀다가 Rebind 이후에 적용
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

        // 화면에 안 보이는 동안이라도 일단 정상값으로 (완전한 보장은 아니지만 최악의 깜빡임 방지용)
        ResetGroupsToDefault();
        pendingHeadOffset = Vector2.zero;

        if (leadWeaponData == null) leadWeaponData = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        bool isLead = wd.Name == leadWeaponData.Name;
        WeaponData dataToShow = isLead ? leadWeaponData : wd;

        SetWeaponVisualData(dataToShow);
        InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item = isLead ? GameManager.instance.startingDataContainer.GetItemDatas()[i] : wd.defaultItems[i];
            if (item == null)
            {
                SetEquipCardDisplay(i, null);
                continue;
            }
            SetEquipCardDisplay(i, item.spriteRow);
            // ★ 위치는 여기서 바로 적용하지 않고 나중에 몰아서 적용
            if (item.needToOffset)
            {
                pendingHeadOffset += item.posHead;
            }
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

        // ★ 권위 있는 최종 리셋: Rebind/Animator 평가가 끝난 뒤, 여기서 한 번 더 확정
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