using System.Collections;
using TMPro;
using UnityEngine;

public class IsButtonClicked : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI isClickedText;

    void Start()
    {
        isClickedText.text = "Not Clicked";       
    }

    public void OnButtonClicked()
    {
        StartCoroutine(ButtonClickedCo());
    }
    IEnumerator ButtonClickedCo()
    {
        isClickedText.text = "Clicked!";
        yield return new WaitForSeconds(10f);
        isClickedText.text = "Not Clicked";
    }
}
