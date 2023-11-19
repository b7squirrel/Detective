using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim;
    [SerializeField] SpriteRenderer body, head, chest, face, hands;
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] Transform spriteGroup;
    [SerializeField] Animator costume;
    [SerializeField] Transform headGroup; // 머리와 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
    [SerializeField] Transform chestGroup; // 가슴과 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다

    bool _facingRight = true;
    int essentialIndex;
    public bool FacingRight
    {
        get => _facingRight;
        set{
            if(_facingRight == value) return;
            _facingRight = value;
            FlipSpriteGroup();
        }
    }

    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }
    void Init(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    public void SetEquipmentSprites(WeaponData wd)
    {
        Init(wd.Animators.InGamePlayerAnim);
        head.sprite = wd.DefaultHead;
        chest.sprite = wd.DefaultChest;
        face.sprite = wd.DefaultFace;
        hands.sprite = wd.DefaultHand;
    }
    // GameManager의 Starting Data Container에서 weapon data, item data를 불러오니까 매개변수가 필요없다. 
    public void SetPlayerEquipmentSprites()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        List<Item> iDatas = GameManager.instance.startingDataContainer.GetItemDatas();

        Init(wd.Animators.InGamePlayerAnim);
        anim.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;

        for (int i = 0; i < 4; i++)
        {
            if (iDatas[i] == null)
            {
                sr[i + 1].gameObject.SetActive(false);
                continue;
            }

            sr[i + 1].sprite = iDatas[i].charImage;
        }
    }
    void FlipSpriteGroup()
    {
        transform.eulerAngles += new Vector3(0, 180f, 0);
    }
    public void SetAnimState(float speed)
    {
        anim.SetFloat("Speed", speed);
    }
    public void ParentWeaponObjectTo(int _index, Transform _weaponObject)
    {
        if (_index == 0 || _index == 2) // 머리 혹은 얼굴 부위이면
        {
            _weaponObject.SetParent(headGroup);
        }
        if (_index == 1) // 가슴 부위이면
        {
            _weaponObject.SetParent(chestGroup);
        }

        // 해당 부위의 스프라이트는 비활성화 시켜서 겹치지 않게 한다
        sr[_index + 1].gameObject.SetActive(false);
    }
}
