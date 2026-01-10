using UnityEngine;

public class PopupUI : MonoBehaviour
{
    [SerializeField] GameObject popupBGType;
    void OnEnable()
    {
        if(popupBGType != null) popupBGType.SetActive(true);
    }
}
