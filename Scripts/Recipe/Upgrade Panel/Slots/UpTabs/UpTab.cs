using UnityEngine;

public class UpTab : MonoBehaviour
{
    Animator anim;
    [SerializeField] string tabType;

    public string GetTabType()
    {
        return tabType;
    }

    public void SetTabActive(bool _isActive)
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        if (_isActive)
        {
            anim.SetTrigger("On");
        }
        else
        {
            anim.SetTrigger("Off");
        }
    }
}