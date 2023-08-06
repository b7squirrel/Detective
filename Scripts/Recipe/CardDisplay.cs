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

        Debug.Log("등급 = " + grade.text + "이름 = " + Name.text);
    }
}
