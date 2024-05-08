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
    int nextExp; // ��ũ �ִԿ����� exp�ٰ� 100%�� �Ǿ�� �ϹǷ�. �ӽ÷� ������ �ΰ� �ִϸ��̼��� ������ ����

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
