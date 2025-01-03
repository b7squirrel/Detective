using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
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

    public void Init(string _skillName, int _evoStage, float _maxSliderValue)
    {
        skillName.text = _skillName;
        if (skillNameShadow != null) skillNameShadow.text = _skillName;
        Debug.Log("Evo Stage = " + _evoStage);
        SetNumStar(_evoStage + 1);
        swipeMask.fillAmount = 1f;

        Anim = GetComponent<Animator>();
        badgeImage = badge.GetComponent<Image>();
        badgeMat = badgeImage.material;
    }

    public void SetSlider(float _coolTime)
    {
        //skillSlider.value = _coolTime;
        swipeMask.fillAmount = 1 - _coolTime;
    }
    void SetNumStar(int numStars)
    {
        stars = null;
        if (stars == null)
        {
            // 3개 만들어서 비활성화
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
        //Anim.SetTrigger("Up");
        Anim.SetTrigger("Hit");
        //if (hitCo != null) StopCoroutine(HitCo());
        //hitCo = StartCoroutine(HitCo());
    }
    // animation event
    public void SetBadgeMatToWhite()
    {
        badgeImage.material = white;
    }
    public void SetBadgeMatToInit()
    {
        badgeImage.material = badgeMat;
    }
    IEnumerator HitCo()
    {
        Debug.Log("Hit Co");
        badgeImage.material = white;
        badge.transform.localScale = Vector3.one * 1.4f;
        yield return new WaitForSeconds(.3f);
        badgeImage.material = badgeMat;
        badge.transform.localScale = Vector3.one;
    }
    public void PlayBadgeAnim(string _trigger)
    {
        Anim.SetTrigger(_trigger);
    }
}