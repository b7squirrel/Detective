using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] Image skillIconImage;  // 👈 아이콘 이미지 추가!
    [SerializeField] TMPro.TextMeshProUGUI skillName;
    [SerializeField] TMPro.TextMeshProUGUI skillNameShadow;
    [SerializeField] GameObject starPrefab;
    [SerializeField] GameObject[] stars;
    [SerializeField] Transform starContainer;
    [SerializeField] GameObject badge;
    [SerializeField] Image swipeMask;
    
    Animator Anim;
    Image badgeImage;
    Material badgeMat;
    Coroutine hitCo;
    [SerializeField] Material white;

    public void Init(SkillData skillData, int evoStage, float maxSliderValue)
    {
        // 아이콘 설정
        if (skillIconImage != null && skillData.skillIcon != null)
        {
            skillIconImage.sprite = skillData.skillIcon;
        }
        
        skillName.text = skillData.skillName;
        if (skillNameShadow != null) skillNameShadow.text = skillData.skillName;
        
        Debug.Log("Evo Stage = " + evoStage);
        SetNumStar(evoStage + 1);
        swipeMask.fillAmount = 1f;
        
        Anim = GetComponent<Animator>();
        badgeImage = badge.GetComponent<Image>();
        badgeMat = badgeImage.material;
    }

    public void SetSlider(float _coolTime)
    {
        swipeMask.fillAmount = 1 - _coolTime;
    }

    void SetNumStar(int numStars)
    {
        stars = null;
        if (stars == null)
        {
            stars = new GameObject[3];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }
        
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void BadgeUpAnim()
    {
        Anim.SetTrigger("Hit");
    }

    public void SetBadgeMatToWhite()
    {
        badgeImage.material = white;
    }

    public void SetBadgeMatToInit()
    {
        badgeImage.material = badgeMat;
    }

    public void PlayBadgeAnim(string _trigger)
    {
        Anim.SetTrigger(_trigger);
    }
}