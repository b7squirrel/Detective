using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
    public void ActivatePanel()
    {
        gameObject.SetActive(true);
    }
    public void DeactivatePanel()
    {
        gameObject.SetActive(false);
    }
}
