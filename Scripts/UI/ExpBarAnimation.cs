using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpBarAnimation : MonoBehaviour
{
    [SerializeField] Animator fillAnim; // 레벨업이 되는 순간 바 전체가 커졌다가 원래 크기로 돌아감
    [SerializeField] GameObject levelUpFill; // 업그레이드를 기다리는 동안 fill bar가 깜빡거림

    public void ExpBarEffect()
    {
        fillAnim.gameObject.SetActive(true);
        fillAnim.SetTrigger("Up");
        EnableExpFillBar();
    }
    void EnableExpFillBar()
    {
        levelUpFill.gameObject.SetActive(true);
    }
    public void DisableExpFillBar()
    {
        levelUpFill.gameObject.SetActive(false);
    }
}
