using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class EggButton : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] AudioClip pickEggSound;
    [SerializeField] AudioClip breakEggSound;
    [SerializeField] AudioClip eggClickSound;

    [Header("White Flash")]
    [SerializeField] Material whiteMat;
    Material initMat;
    Image image;

    bool isClicked; // �ʹ� �������� Ŭ���Ǵ� ���� ���� ����

    Animator anim;

    bool pickedEgg;

    void OnEnable()
    {
        pickedEgg = false;
        if (image == null) image = GetComponent<Image>();
        if (initMat == null) initMat = image.material;
        image.material = initMat; // whiteMat�� ����� ���·� �������� �ʱ� ����
    }
    public void PlayEggClickSound()
    {
        if (pickedEgg)
        {
            SoundManager.instance.Play(breakEggSound);
        }
        else
        {
            pickedEgg = true;
            SoundManager.instance.Play(pickEggSound);
        }
    }
    public void EggClicledFeedback()
    {
        if (isClicked) return;
        if (anim == null) anim = GetComponentInParent<Animator>();
        anim.SetTrigger("Clicked");

        StartCoroutine(WhiteFlashCo());
    }

    IEnumerator WhiteFlashCo()
    {
        isClicked = true;
        image.material = whiteMat;
        yield return new WaitForSecondsRealtime(.05f);
        isClicked = false;
        image.material = initMat;
    }

    public void ChangeEggImage()
    {

    }
    public void GetEggStats()
    {

    }
}
