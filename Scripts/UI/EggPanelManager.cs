using System.Collections;
using UnityEngine;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] PauseManager pauseManager;
    [SerializeField] GameObject oriName;
    [SerializeField] GameObject yayText;
    [SerializeField] GameObject nameBar;
    [SerializeField] GameObject newKidText;
    [SerializeField] GameObject blackBGPanel;
    RuntimeAnimatorController kidAnim;
    Coroutine Close;

    [Header("임시 오브젝트 비활성화")]
    [SerializeField]
    GameObject[] testEquipmentImages;

    [SerializeField] GameObject newOriContainer;
    [SerializeField] SpriteRenderer[] EquipmentSprites;
    [SerializeField] GameObject rawImage;
    [SerializeField] Animator anim; // 오리(weapon container)의 animator
    [SerializeField] Animator eggPanelAnim;

    [Header("Sound")]
    [SerializeField] AudioClip oriSound;
    [SerializeField] AudioClip oriNameSound;
    [SerializeField] AudioClip cheerGroup;
    [SerializeField] AudioClip jumpUp;
    [SerializeField] AudioClip breakingEgg;
    [SerializeField] AudioClip nameHightlight;

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
        newOriContainer.SetActive(true);
        rawImage.SetActive(true);
        anim.SetTrigger("Victory");
    }
    void CloseNewKidImage()
    {
        newOriContainer.SetActive(false);
        rawImage.SetActive(false);
    }

    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();

        EggImageUp(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
        nameBar.SetActive(false);
        yayText.SetActive(false);
        CloseNewKidImage();

        for (int i = 0; i < testEquipmentImages.Length; i++)
        {
            testEquipmentImages[i].SetActive(false);
        }
    }
    public void EggPanelUP(RuntimeAnimatorController anim, string name)
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        kidAnim = anim;
        newKidText.SetActive(true);
        oriName.GetComponent<TMPro.TextMeshProUGUI>().text = name + "!";

        blackBGPanel.SetActive(true);
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp()
    {
        OpenNewKidImage();
        //kidImage.SetActive(isActive);

        newKidText.SetActive(false);
        oriName.SetActive(true);
        nameBar.SetActive(true);
        yayText.SetActive(true);
        //oriNameShadow.SetActive(true);

        //if (isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = kidAnim;
        //closeButton.SetActive(true);
        eggPanelAnim.SetTrigger("KidUp");
        SoundManager.instance.Play(oriSound);
        SoundManager.instance.Play(cheerGroup);
    }
    public void EggAnimFinished()
    {
        KidImageUp();
        Close = StartCoroutine(CloseCo());
    }

    IEnumerator CloseCo()
    {
        yield return new WaitForSecondsRealtime(1.66f); // 이름 반짝 사운드 재생 지점
        //SoundManager.instance.Play(nameHightlight);
        SoundManager.instance.Play(oriNameSound);

        yield return new WaitForSecondsRealtime(.4f); //폴짝 뛰어서 게임 안으로 들어가는 지점
        SoundManager.instance.Play(jumpUp);

        yield return new WaitForSecondsRealtime(0.32f); // 애니메이션 종료
        CloseButtonPressed();
    }
    public void CloseButtonPressed()
    {
        pauseManager.UnPauseGame();
        EggImageUp(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
        nameBar.SetActive(false);
        yayText.SetActive(false);

        CloseNewKidImage();

        // 돌아가고 있는 코루틴을 멈추지 않으면 
        // 버튼을 누르지 않고 자동으로 창이 종료되었을 때 코루틴이 실행되어 정지된 시간을 풀어버림
        StopCoroutine(Close); 
    }
}
