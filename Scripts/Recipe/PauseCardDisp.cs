using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseCardDisp : MonoBehaviour
{
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] Image[] equipSR;
    Costume costume;
    [SerializeField] SetCostumeImage setCostumeImage;

    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected GameObject focusImage;
    [SerializeField] protected GameObject leaderText;
    [SerializeField] protected GameObject leaderIcon;

    GameObject[] stars;
    public string Name { get; private set; }
    int currentLevel;

    #region 오리
    public void InitLeadWeaponCardDisplay(WeaponData _wd)
    {
        // 별
        SetNumStar(currentLevel, true);

        // 이름
        Name = _wd.Name;

        // Focus Image 활성화
        // focusImage.gameObject.SetActive(true);
        // leaderText.gameObject.SetActive(true);
        leaderIcon.gameObject.SetActive(true);

        // 오리 base 이미지, 애니메이션
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = _wd.Animators.PauseCardAnim;

        // 장비 장착
        List<Item> items = new();
        items = FindObjectOfType<StartingDataContainer>().GetItemDatas();
        for (int i = 0; i < 4; i++)
        {
            if (items[i] != null)
            {
                equipSR[i].sprite = items[i].charImage;
                equipSR[i].gameObject.SetActive(true);
                continue;
            }
            equipSR[i].sprite = null;
            equipSR[i].gameObject.SetActive(false);

            if (_wd.costume != null)
            {
                costume = _wd.costume;
                setCostumeImage.SetCostumeData(costume);
            }
        }
    }
    public void InitWeaponCardDisplay(WeaponData _wd)
    {
        // 별
        SetNumStar(currentLevel, true);

        // 이름
        Name = _wd.Name;

        // 오리 base 이미지, 애니메이션
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = _wd.Animators.PauseCardAnim;

        // 기본 장비 장착
        if (_wd.DefaultHead != null)
        {
            equipSR[0].sprite = _wd.DefaultHead;
            equipSR[0].gameObject.SetActive(true);
        }
        if (_wd.DefaultChest != null)
        {
            equipSR[1].sprite = _wd.DefaultChest;
            equipSR[1].gameObject.SetActive(true);
        }
        if (_wd.DefaultFace != null)
        {
            equipSR[2].sprite = _wd.DefaultFace;
            equipSR[2].gameObject.SetActive(true);
        }
        if (_wd.DefaultHands != null)
        {
            equipSR[3].sprite = _wd.DefaultHands;
            equipSR[3].gameObject.SetActive(true);
        }

        if (_wd.costume != null)
        {
            costume = _wd.costume;
            setCostumeImage.SetCostumeData(costume);
            Debug.Log("costume name = " + costume.name);
        }
    }

    // 애니메이션 이벤트로 해당 프레임마다 스프라이트 교체
    public void SetEquipCardImage(int index, Sprite equipmentImage)
    {
        if (equipmentImage == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
            return;
        }
        equipmentImages[index].gameObject.SetActive(true);
        equipmentImages[index].sprite = equipmentImage;
    }

    #endregion
    #region 공통
    void SetNumStar(int numStars, bool _isWeapon)
    {
        int maxStarNum = _isWeapon ? 5 : 3;

        if (stars == null)
        {
            // 최대 합성 레벨만큼 만들어서 비활성화
            stars = new GameObject[maxStarNum];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // 일단 모든 별을 비활성화. 많은 별에서 적은 별로 업데이트 하면 많은 별로 남아있기 떄문
        for (int i = 0; i < maxStarNum; i++)
        {
            stars[i].SetActive(false);
        }

        // 등급만큼 별 활성화. 별 리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void EmptyCardDisplay()
    {
        // 별 비활성화
        DeactivateStars();

        // 오리 이미지 비활성화
        charImage.gameObject.SetActive(false);

        // 장비 이미지 비활성화
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i] == null)
                continue;
            equipmentAnimators[i].gameObject.SetActive(false);
        }
    }

    void DeactivateStars()
    {
        // 별 비활성화
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }

        focusImage.gameObject.SetActive(false);
        leaderText.gameObject.SetActive(false);
        leaderIcon.gameObject.SetActive(false);

    }
    public void UpdatePauseCardLevel(int _level, bool _isWeapon)
    {
        Debug.Log("Upgraded Level = " + _level);
        SetNumStar(_level, _isWeapon);
    }
    #endregion
    #region 아이템
    public void InitItemCardDisplay(Item _item)
    {
        // 별
        SetNumStar(currentLevel, false);

        // 이름
        Name = _item.Name;

        charImage.sprite = _item.charImage;
        charImage.SetNativeSize();
        charImage.rectTransform.localScale = 1.2f * Vector2.one;
    }
    
    #endregion
}