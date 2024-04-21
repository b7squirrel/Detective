using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TextMeshProUGUI levelText;
    [SerializeField] Sprite fillImage;
    [SerializeField] Image sliderFillImage;
    [SerializeField] Animator fillSliderAnim;

    public void UpdateExperienceSlider(int current, int target)
    {
        slider.maxValue = target;
        slider.value = current;

        Debug.Log("Add");
        fillSliderAnim.SetTrigger("Add");
        // SetFillImage();
    }

    public void ExpBarBlink()
    {
        Debug.Log("Blink");
        fillSliderAnim.SetTrigger("Blink");
    }
    public void ExpBarIdle()
    {
        Debug.Log("Idle");
        fillSliderAnim.SetTrigger("Idle");
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
