using UnityEngine;

public class PauseCardDisp : MonoBehaviour
{
    [SerializeField] Transform starContainer;

    [SerializeField] GameObject starPrefab;
    [SerializeField] TMPro.TextMeshProUGUI synergyText;
    [SerializeField] GameObject leadTag;

    GameObject[] stars;
    public string Name { get; private set; }

    #region 오리
    public void InitWeaponCardDisplay(WeaponData _wd)
    {
        // 별
        SetNumStar(_wd.stats.currentLevel, true);

        // 이름
        Name = _wd.Name;

        synergyText.gameObject.SetActive(false);
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
            Debug.Log($"card disp의 인덱스 {i} of {maxStarNum}");
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
        // // 별 비활성화
        // DeactivateStars();

        // // 오리 이미지 비활성화
        // charImage.gameObject.SetActive(false);
        // if(synergyText != null) synergyText.gameObject.SetActive(false);

        // // 장비 이미지 비활성화
        // for (int i = 0; i < 4; i++)
        // {
        //     if (equipmentAnimators[i] == null)
        //         continue;
        //     equipmentAnimators[i].gameObject.SetActive(false);
        // }
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
    }
    public void UpdatePauseCardLevel(int _level, bool _isWeapon, bool _isSynergy)
    {
        if(_isSynergy)
        {
            SetNumStar(0, _isWeapon);
            synergyText.gameObject.SetActive(true);
            return;
        }
        SetNumStar(_level, _isWeapon);
    }
    #endregion
    
    #region 아이템
    public void InitItemCardDisplay(Item _item)
    {
        // 별
        SetNumStar(_item.stats.currentLevel, false);

        // 이름
        Name = _item.Name;
    }
    
    #endregion
}