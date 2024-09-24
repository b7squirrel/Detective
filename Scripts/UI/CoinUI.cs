using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public static CoinUI instance;
    Animator anim;

    void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }
    public void PopCoin()
    {
        anim.SetTrigger("Pop");
    }
}
