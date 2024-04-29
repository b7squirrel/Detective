using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseCardDisp : MonoBehaviour
{
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] Image[] equipSR;
    Costume costume;
    [SerializeField] Image costumeImage;

    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected GameObject focusImage;

    GameObject[] stars;

    public void InitLeadWeaponCardDisplay(WeaponData _wd)
    {
        // 별
        SetNumStar(1);

        // Focus Image 활성화
        focusImage.gameObject.SetActive(true);

        // 오리 base 이미지, 애니메이션
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = _wd.Animators.CardImageAnim;

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
                costumeImage.color = new Color(1, 1, 1, 1);
                Debug.Log("costume name = " + costume.name);
            }
            else
            {
                costumeImage.color = new Color(1, 1, 1, 0);
            }
        }
    }
    public void InitWeaponCardDisplay(WeaponData _wd)
    {
        // 별
        SetNumStar(1);

        // 오리 base 이미지, 애니메이션
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = _wd.Animators.CardImageAnim;

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
            costumeImage.color = new Color(1, 1, 1, 1);
            Debug.Log("costume name = " + costume.name);
        }
        else
        {
            costumeImage.color = new Color(1, 1, 1, 0);
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
    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 최대 합성 레벨만큼 만들어서 비활성화
            stars = new GameObject[StaticValues.MaxEvoStage];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // 일단 모든 별을 비활성화. 많은 별에서 적은 별로 업데이트 하면 많은 별로 남아있기 떄문
        for (int i = 0; i < StaticValues.MaxEvoStage; i++)
        {
            stars[i].SetActive(false);
        }

        // 등급만큼 별 활성화. 별 리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }
    // 애니메이션 이벤트
    public void SetCostumeSprite(int _index)
    {
        if (costume == null) return;
        costumeImage.sprite = costume.sprites[_index];
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
    }
}