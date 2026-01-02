using UnityEngine;

public class ButtonBadgeUI : MonoBehaviour
{
    [SerializeField] GameObject badge;

    public void ActivateBadge(bool _activate)
    {
        badge.SetActive(_activate);
    }
}
