using UnityEngine;

public class BossStateDisp : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshPro stateText;

    public void SetStateText(string state)
    {
        stateText.text = state;
    }
}
