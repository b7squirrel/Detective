using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI skillName;
    [SerializeField] GameObject starPrefab;
    [SerializeField] GameObject[] stars;
    [SerializeField] Transform starContainer;
    [SerializeField] Animator badgeAnim;
    Slider skillSlider;

    public void Init(string _skillName, int _evoStage, float _maxSliderValue)
    {
        skillName.text = _skillName;
        Debug.Log("Evo Stage = " + _evoStage);
        SetNumStar(_evoStage);
        skillSlider = GetComponent<Slider>();

        skillSlider.maxValue = 1;
        skillSlider.value = 0;
    }
    
    public void SetSlider(float _coolTime)
    {
        skillSlider.value = _coolTime;
    }
    void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 5개 만들어서 비활성화
            stars = new GameObject[3];
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
    public void BadgeUpAnim()
    {
        badgeAnim.SetTrigger("Up");
    }
}