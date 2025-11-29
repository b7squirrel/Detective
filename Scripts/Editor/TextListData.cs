using UnityEngine;
using TMPro;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TextListData", menuName = "Tools/Text List Data")]
public class TextListData : ScriptableObject
{
    public List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();
}
