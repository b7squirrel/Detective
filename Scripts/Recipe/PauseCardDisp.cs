using UnityEngine;
using UnityEngine.UI;

public class PauseCardDisp : MonoBehaviour
{
    [SerializeField] Transform starContainer;
    [SerializeField] Transform cardBaseContainer;
    [SerializeField] GameObject starPrefab;
    [SerializeField] GameObject leadTag;
    [SerializeField] GameObject synergyGroup;
    [SerializeField] GameObject[] synergytags;
    [SerializeField] GameObject synergyDoneGroup;
    [SerializeField] GameObject newWeaponText;
    [SerializeField] GameObject newItemText;
    [SerializeField] RectTransform synergyOutPoint;
    [SerializeField] RectTransform synergyInPoint;
    SimpleUILineConnector lineConnector;

    GameObject[] stars;
    public string Name { get; private set; }

    [Header("Synergy Icon")]
    [SerializeField] Image synergyIcon;

    public void InitWeaponCardDisplay(WeaponData _wd)
    {
        SetNumStar(_wd.stats.currentLevel, true);
        newWeaponText.SetActive(_wd.stats.currentLevel == 0);
        newItemText.SetActive(false);

        for (int i = 0; i < StaticValues.MaxGrade; i++)
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);

        cardBaseContainer.GetChild(_wd.grade).gameObject.SetActive(true);
        Name = _wd.Name;
        synergyGroup.SetActive(false);

        synergyIcon.gameObject.SetActive(true);
        synergyIcon.sprite = _wd.SynergyItem.charImage;
    }

    public void EnableLeadTag(bool enableLeadTag) => leadTag.SetActive(enableLeadTag);

    void SetNumStar(int numStars, bool _isWeapon)
    {
        int maxStarNum = _isWeapon ? 5 : 3;

        if (stars == null)
        {
            stars = new GameObject[maxStarNum];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }
        for (int i = 0; i < stars.Length; i++)
            stars[i].SetActive(false);

        for (int i = 0; i < numStars; i++)
            stars[i].SetActive(true);
    }

    public void UpdatePauseCardLevel(int _level, bool _isWeapon, bool _isSynergy)
    {
        if (_isSynergy)
        {
            SetNumStar(0, _isWeapon);
            synergyGroup.SetActive(true);
            GetComponent<RectTransform>().localScale = 1.2f * Vector2.one;
            return;
        }
        SetNumStar(_level, _isWeapon);
        newWeaponText.SetActive(false);
        newItemText.SetActive(false);
    }

    public void InitItemCardDisplay(Item _item)
    {
        SetNumStar(_item.stats.currentLevel, false);
        newWeaponText.SetActive(false);
        newItemText.SetActive(_item.stats.currentLevel == 0);

        for (int i = 0; i < StaticValues.MaxGrade; i++)
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);

        cardBaseContainer.GetChild(0).gameObject.SetActive(true);
        Name = _item.Name;
        synergyGroup.SetActive(false);

        synergyIcon.gameObject.SetActive(false);
        foreach (var item in synergytags)
            item.SetActive(false);
    }

    public RectTransform GetSynergyInPoint() => synergyInPoint;
    public RectTransform GetSynergyOutPoint() => synergyOutPoint;
}
