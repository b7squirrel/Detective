using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EggPanelManager : MonoBehaviour
{
    string currentWeaponName;
    int currentGrade;

    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] PauseManager pauseManager;
    [SerializeField] GameObject oriNameGroup;
    [SerializeField] GameObject oriName;
    [SerializeField] GameObject yayText;
    [SerializeField] GameObject nameBar;
    [SerializeField] GameObject newKidText;
    [SerializeField] GameObject birdFlock;
    [SerializeField] GameObject flashEffect;
    [SerializeField] ParticleSystem twinkleStarsParticle;
    [SerializeField] GameObject blackBGPanel;
    [SerializeField] GameObject whiteBGPanel;
    EggButton eggButton; // Egg Button에 접근해서 레어오리 확률을 초기화 시키기 위해

    Coroutine Close;

    [Header("임시 오브젝트 비활성화")]
    [SerializeField]
    GameObject[] testEquipmentImages;

    [SerializeField] GameObject newOriContainer;
    [SerializeField] SpriteRenderer[] EquipmentSprites;
    [SerializeField] GameObject rawImage;
    [SerializeField] Animator anim; // 오리(weapon container)의 animator
    [SerializeField] Animator eggPanelAnim;
    EggButton eggbutton;

    [Header("Sound")]
    [SerializeField] AudioClip oriSound;
    [SerializeField] AudioClip oriNameSound;
    [SerializeField] AudioClip cheerGroup;
    [SerializeField] AudioClip jumpUp;
    [SerializeField] AudioClip breakingEgg;

    WeaponDataDictionary wdDictionary;

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
    void SetEquipmentSprites(WeaponData wd)
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
        flashEffect.SetActive(true);
        anim.SetTrigger("Victory");
    }
    void CloseNewKidImage()
    {
        newOriContainer.SetActive(false);
        rawImage.SetActive(false);
        flashEffect.SetActive(false);
    }

    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();

        EggImageUp(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        whiteBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriNameGroup.SetActive(false);
        oriName.SetActive(false);
        nameBar.SetActive(false);
        yayText.SetActive(false);
        birdFlock.SetActive(false);
        flashEffect.SetActive(false);
        twinkleStarsParticle.Stop();

        CloseNewKidImage();

        for (int i = 0; i < testEquipmentImages.Length; i++)
        {
            testEquipmentImages[i].SetActive(false);
        }
    }
    public void EggPanelUP()
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        newKidText.SetActive(true);

        blackBGPanel.SetActive(true);
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);

        if (eggButton == null) eggButton = eggImage.GetComponentInChildren<EggButton>();
        if (isActive) eggButton.InitRate();
    }
    void KidImageUp()
    {
        OpenNewKidImage();
        newKidText.SetActive(false);
        oriNameGroup.SetActive(true);
        oriName.SetActive(true);
        nameBar.SetActive(true);
        yayText.SetActive(true);

        eggPanelAnim.SetTrigger("KidUp");
        SoundManager.instance.Play(oriSound);
        SoundManager.instance.Play(cheerGroup);
        birdFlock.SetActive(true);
        twinkleStarsParticle.Play();
    }

    public void SetWeaponName(string _name)
    {
        currentWeaponName = _name;
    }
    public void SetWeaponGrade(int _grade)
    {
        currentGrade = _grade;
    }
    public void EggAnimFinished()
    {
        // 뽑은 오리의 등급을 넘겨 받음
        if(eggbutton == null) eggbutton = FindObjectOfType<EggButton>();
        currentGrade = eggbutton.GetWeaponGradeIndex();

        // 뽑은 오리의 이름과 등급이 일치하는 Upgrade Data(Acquire) 가져오기
        UpgradeData newWd = GetAcquireData(currentWeaponName, currentGrade);

        // 이름 반영
        oriName.GetComponent<TMPro.TextMeshProUGUI>().text = newWd.weaponData.DisplayName;

        // 장비 스프라이트 설정
        SetEquipmentSprites(newWd.weaponData);

        // 플레이어에 새로운 오리 추가하기
        GameManager.instance.character.GetComponent<Level>().GetWeapon(newWd);

        // 새로운 아이 패널 띄우기, 확정된 등급 패널 애님 플레이
        KidImageUp();
        eggbutton.PlayGradePanelFixedAnim();
        Close = StartCoroutine(CloseCo());
    }
    UpgradeData GetAcquireData(string _name, int _grade)
    {
        if (wdDictionary == null) wdDictionary = FindObjectOfType<WeaponDataDictionary>();
        UpgradeData acquireData = wdDictionary.GetAcquireDataFrom(currentWeaponName, currentGrade);
        return acquireData;
    }

    IEnumerator CloseCo()
    {
        //whiteBGPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(1.66f); // 이름 반짝 사운드 재생 지점


        SoundManager.instance.Play(oriNameSound);
        whiteBGPanel.SetActive(false);
        yield return new WaitForSecondsRealtime(.4f); //폴짝 뛰어서 게임 안으로 들어가는 지점
        SoundManager.instance.Play(jumpUp);

        yield return new WaitForSecondsRealtime(0.32f); // 애니메이션 종료
        blackBGPanel.SetActive(false);
        CloseButtonPressed();
    }
    void CloseButtonPressed()
    {
        pauseManager.UnPauseGame();
        EggImageUp(false);
        eggPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
        nameBar.SetActive(false);
        yayText.SetActive(false);
        birdFlock.SetActive(false);
        twinkleStarsParticle.Stop();
        CloseNewKidImage();

        GameManager.instance.popupManager.IsUIDone = true;
        // 돌아가고 있는 코루틴을 멈추지 않으면 
        // 버튼을 누르지 않고 자동으로 창이 종료되었을 때 코루틴이 실행되어 정지된 시간을 풀어버림
        StopCoroutine(Close);
    }
}
