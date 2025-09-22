using UnityEngine;
using UnityEngine.UI;

public class SliderMaxChecker : MonoBehaviour
{
    Slider slider;
    [SerializeField] Animator oriAnim; // 100%가 되었을 때 오리의 애니메이션을 idle로 변경

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError("Slider 컴포넌트가 없습니다!");
            return;
        }

        // 슬라이더 값 변경 시 이벤트 연결
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        // 슬라이더가 100%일 때만 출력
        if (Mathf.Approximately(value, slider.maxValue))
        {
            oriAnim.SetTrigger("Idle");
        }
    }
}
