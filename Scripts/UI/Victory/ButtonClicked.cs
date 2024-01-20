using UnityEngine;

public class ButtonClicked : MonoBehaviour
{
    public void SetButtonClicked()
    {
        GetComponent<Animator>().SetTrigger("Clicked");
    }
}