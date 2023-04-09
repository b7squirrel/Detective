using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TextMeshProUGUI levelText;
    [SerializeField] Sprite[] fillImage;

    public void UpdateExperienceSlider(int current, int target)
    {
        slider.maxValue = target;
        slider.value = current;
    }

    public void SetLevelText(int level)
    {
        levelText.text = "LEVEL  " + level.ToString();
    }

    void SetFillImage()
    {
        float proportion = slider.value / slider.maxValue;
        if (proportion > 1 / 3f) slider.image.sprite = fillImage[0];
        if (proportion > 2 / 3f) slider.image.sprite = fillImage[1];
        if (proportion > 3 / 3f) slider.image.sprite = fillImage[2];
    }
}
