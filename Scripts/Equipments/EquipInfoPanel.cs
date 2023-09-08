using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI grade;
    [SerializeField] TMPro.TextMeshProUGUI Name;
    [SerializeField] UnityEngine.UI.Image itemImage;

    public void SetPanel(CardData cardData)
    {
        grade.text = cardData.Grade;
        Name.text = cardData.Name;
        
        CardsDictionary cardsDictionary = FindAnyObjectByType<CardsDictionary>();
    }
}
