using UnityEngine;
using TMPro;

/// <summary>
/// 확률 팝업 안에서 상품 하나를 표시하는 섹션.
/// 제목(상품명) + 필요시 서브라벨("일반"/"보장") + Row 컨테이너로 구성됩니다.
/// </summary>
public class ProbabilitySectionUI : MonoBehaviour
{
    [Header("제목")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("일반 확률")]
    [SerializeField] private GameObject normalSubLabel;
    [SerializeField] private Transform normalRowContainer;

    [Header("보장 확률")]
    [SerializeField] private GameObject guaranteeSubLabel;
    [SerializeField] private Transform guaranteeRowContainer;

    public Transform NormalRowContainer => normalRowContainer;
    public Transform GuaranteeRowContainer => guaranteeRowContainer;

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void SetHasGuarantee(bool hasGuarantee)
    {
        if (guaranteeSubLabel != null)
            guaranteeSubLabel.SetActive(hasGuarantee);
    }

    public void SetShowNormalSubLabel(bool show)
    {
        if (normalSubLabel != null)
            normalSubLabel.SetActive(show);
    }
}