using System.Collections;
using System.Collections.Generic;
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

    public void PopCoinIcon()
    {
        anim.SetTrigger("Pop");
    }
}
