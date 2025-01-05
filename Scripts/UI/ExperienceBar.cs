using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TextMeshProUGUI levelText;
    [SerializeField] Sprite fillImage;
    [SerializeField] Image sliderFillImage;
    [SerializeField] Animator fillSliderAnim;
    [SerializeField] Animator levelTextAnim;
    int nextExp; // 블링크 애님에서는 exp바가 100%가 되어야 하므로. 임시로 저장해 두고 애니메이션이 끝나면 적용

    public void UpdateExperienceSlider(int current, int target)
    {
        slider.maxValue = target;
        slider.value = current;
        nextExp = current;

        fillSliderAnim.SetTrigger("Add");
    }

    public void ExpBarBlink(int _expToLevelUp)
    {
        fillSliderAnim.SetTrigger("Blink");
        slider.value = _expToLevelUp;
    }
    public void ExpBarIdle()
    {
        fillSliderAnim.SetTrigger("Idle");
        levelTextAnim.SetTrigger("Up");
    }

    public void SetLevelText(int level)
    {
        levelText.text = level.ToString();
    }
}