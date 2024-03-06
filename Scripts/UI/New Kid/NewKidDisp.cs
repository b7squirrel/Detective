using UnityEngine;

/// <summary>
/// weapon container anim ��ũ��Ʈ�� ���� ����
/// </summary>
public class NewKidDisp : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] EquipmentSprites;
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] GameObject rawImage;
    Animator anim; // ������ animator

    void Init(RuntimeAnimatorController animCon)
    {
        rawImage.SetActive(true);
        anim.runtimeAnimatorController = animCon;
    }
    // ��� sprite�� ��� default��
    public void SetEquipmentSprites(WeaponData wd)
    {
        Init(wd.Animators.InGamePlayerAnim);

        if (wd.DefaultHead != null) sr[1].sprite = wd.DefaultHead;
        if (wd.DefaultChest != null) sr[2].sprite = wd.DefaultChest;
        if (wd.DefaultFace != null) sr[3].sprite = wd.DefaultFace;
        if (wd.DefaultHands != null) sr[4].sprite = wd.DefaultHands;
    }
    public void CloseNewKidImage()
    {
        rawImage.SetActive(false);

    }
}