using UnityEngine;

public class UpTab : MonoBehaviour
{
    Animator anim;
    [SerializeField] string tabType;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public string GetTabType()
    {
        return tabType;
    }

    public void SetTabActive(bool _isActive)
    {
        if(_isActive)
        {
            anim.SetTrigger("On");
        }
        else
        {
            anim.SetTrigger("Off");
        }
    }
}