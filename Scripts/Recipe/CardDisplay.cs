using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI grade, Name;
    public void SetCardDisplay(string cardGrade, string cardName)
    {
        grade.text = cardGrade;
        Name.text = cardName;
    }
}
