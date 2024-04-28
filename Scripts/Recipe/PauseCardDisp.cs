using UnityEngine;
using UnityEngine.UI;

public class PauseCardDisp : MonoBehaviour
{
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] Image[] equipSR;
    Costume costume;
    [SerializeField] Image costumeImage;

    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;

    public void InitWeaponCardDisplay(WeaponData _wd)
    {
        // ��
        SetNumStar(1);

        // ���� base �̹���, �ִϸ��̼�
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = _wd.Animators.CardImageAnim;

        // �⺻ ��� ����
        if (_wd.DefaultHead != null) equipSR[0].sprite = _wd.DefaultHead;
        if (_wd.DefaultChest != null) equipSR[1].sprite = _wd.DefaultChest;
        if (_wd.DefaultFace != null) equipSR[2].sprite = _wd.DefaultFace;
        if (_wd.DefaultHands != null) equipSR[3].sprite = _wd.DefaultHands;

        if (_wd.costume != null)
        {
            costume = _wd.costume;
            costumeImage.color = new Color(1, 1, 1, 1);
            Debug.Log("costume name = " + costume.name);
        }
        else
        {
            costumeImage.color = new Color(1, 1, 1, 0);
        }
    }

    public void SetRunTimeAnimController(int index, RuntimeAnimatorController animatorController)
    {
        equipmentAnimators[index].gameObject.SetActive(true);
        equipmentAnimators[index].runtimeAnimatorController = animatorController;
        if (animatorController == null)
        {
            equipmentAnimators[index].gameObject.SetActive(false);
        }
        charAnim.Rebind();
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i].gameObject.activeSelf)
            {
                equipmentAnimators[i].Rebind();
            }
        }
    }
    public void SetEquipCardImage(int index, Sprite equipmentImage)
    {
        if (equipmentImage == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
            return;
        }
        equipmentImages[index].gameObject.SetActive(true);
        equipmentImages[index].sprite = equipmentImage;
    }
    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // �ִ� �ռ� ������ŭ ���� ��Ȱ��ȭ
            stars = new GameObject[StaticValues.MaxEvoStage];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // �ϴ� ��� ���� ��Ȱ��ȭ. ���� ������ ���� ���� ������Ʈ �ϸ� ���� ���� �����ֱ� ����
        for (int i = 0; i < StaticValues.MaxEvoStage; i++)
        {
            stars[i].SetActive(false);
        }

        // ��޸�ŭ �� Ȱ��ȭ�ϰ� ������Ʈ�� �ֱ�
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void EmptyCardDisplay()
    {
        // �� ��Ȱ��ȭ
        DeactivateStars();

        // ĳ���� �̹���
        charImage.gameObject.SetActive(false);

        // ��� �̹���
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i] == null)
                continue;
            equipmentAnimators[i].gameObject.SetActive(false);
        }
    }

    void DeactivateStars()
    {
        // �� ��Ȱ��ȭ
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }
    }
}