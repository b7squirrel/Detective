using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI versionText;
    [SerializeField] private string prefix = "v"; // 필요 없으면 빈 문자열로

    private void Start()
    {
        versionText.text = prefix + Application.version;
    }
}