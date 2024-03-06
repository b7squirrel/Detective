using System.Collections;
using UnityEngine;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] GameObject kidImage;
    [SerializeField] GameObject closeButton;
    [SerializeField] PauseManager pauseManager;
    [SerializeField] GameObject oriName;
    [SerializeField] GameObject oriNameShadow;
    [SerializeField] GameObject newKidText;
    [SerializeField] GameObject blackBGPanel;
    RuntimeAnimatorController kidAnim;
    Coroutine Close;





    [SerializeField] SpriteRenderer[] EquipmentSprites;
    [SerializeField] GameObject rawImage;
    [SerializeField] Animator anim; // 오리(weapon container)의 animator

    void Init(WeaponData wd)
    {
        CloseNewKidImage();
        anim.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;
        for (int i = 0; i < EquipmentSprites.Length; i++)
        {
            EquipmentSprites[i].sprite = null;
        }
    }
    // 장비 sprite는 모두 default로
    public void SetEquipmentSprites(WeaponData wd)
    {
        Init(wd);

        if (wd.DefaultHead != null) EquipmentSprites[0].sprite = wd.DefaultHead;
        if (wd.DefaultChest != null) EquipmentSprites[1].sprite = wd.DefaultChest;
        if (wd.DefaultFace != null) EquipmentSprites[2].sprite = wd.DefaultFace;
        if (wd.DefaultHands != null) EquipmentSprites[3].sprite = wd.DefaultHands;
    }

    void OpenNewKidImage()
    {
        rawImage.SetActive(true);
    }
    void CloseNewKidImage()
    {
        rawImage.SetActive(false);

    }







    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
    }
    public void EggPanelUP(RuntimeAnimatorController anim, string name)
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        kidAnim = anim;
        newKidText.SetActive(true);
        oriName.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        oriName.SetActive(false);
        oriNameShadow.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        oriNameShadow.SetActive(false);

        blackBGPanel.SetActive(true);
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp(bool isActive)
    {
        OpenNewKidImage();
        //kidImage.SetActive(isActive);

        newKidText.SetActive(false);
        oriName.SetActive(true);
        oriNameShadow.SetActive(true);

        //if (isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = kidAnim;
        closeButton.SetActive(true);
    }
    public void EggAnimFinished()
    {
        KidImageUp(true);
        Close = StartCoroutine(CloseCo());
    }

    IEnumerator CloseCo()
    {
        yield return new WaitForSecondsRealtime(5f);
        CloseButtonPressed();
    }
    public void CloseButtonPressed()
    {
        EggImageUp(false);
        KidImageUp(false);
        pauseManager.UnPauseGame();
        closeButton.SetActive(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
        oriNameShadow.SetActive(false);

        CloseNewKidImage();

        // 돌아가고 있는 코루틴을 멈추지 않으면 
        // 버튼을 누르지 않고 자동으로 창이 종료되었을 때 코루틴이 실행되어 정지된 시간을 풀어버림
        StopCoroutine(Close); 
    }
}
