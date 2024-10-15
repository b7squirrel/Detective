using UnityEngine;

public class LoadingSwipeHint : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI hintText;
    HintsOnLoading hintsOnLoading;

    private void OnEnable()
    {
        if (hintsOnLoading == null) hintsOnLoading =FindObjectOfType<HintsOnLoading>();
        hintsOnLoading.Init();
        hintText.text = hintsOnLoading.GetHint();
    }
}
