using UnityEngine;

public class InvincibleCounterUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshPro number;
    public void SetCountNumber(int num)
    {
        number.text = num.ToString();
    }
}
