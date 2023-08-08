using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSuccessUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI Name, Grade;
    [SerializeField] UnityEngine.UI.Image charImage;

    
    public void SetCard(Card card)
    {
        Name.text = card.GetCardName();
        Grade.text = card.GetCardGrade().ToString();
    }
}
