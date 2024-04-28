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
        Debug.Log("Blink");
        fillSliderAnim.SetTrigger("Blink");
        slider.value = _expToLevelUp;
    }
    public void ExpBarIdle()
    {
        Debug.Log("Idle");
        fillSliderAnim.SetTrigger("Idle");
        levelTextAnim.SetTrigger("Up");
    }

    public void SetLevelText(int level)
    {
        levelText.text = level.ToString();
    }

    // void SetFillImage()
    // {
    //     float proportion = slider.value / slider.maxValue;
    //     if (proportion < 3 / 3f) sliderFillImage.sprite = fillImage[2];
    //     if (proportion < 2 / 3f) sliderFillImage.sprite = fillImage[1];
    //     if (proportion < 1 / 3f) sliderFillImage.sprite = fillImage[0];
    // }
}
