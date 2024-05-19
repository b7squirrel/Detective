using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim; // 오리의 animator
    Costume costume;
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] SpriteRenderer face;
    [SerializeField] Transform spriteGroup;
    [SerializeField] Transform headGroup; // 머리와 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
    [SerializeField] Transform chestGroup; // 가슴과 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
    [SerializeField] Transform handsGroup; // 손과 함께 움직이는 중비들은 모두 여기에 페어런트 시킨다
    [SerializeField] SpriteRenderer costumeSR;

    Sprite sprite; // 개별 무기들의 sprite
    public static int indexSortingOrder = 100; // 소팅오더를 정하기 위한 인덱스

    bool _facingRight = true;
    int essentialIndex;
    public bool FacingRight
    {
        get => _facingRight;
        set
        {
            if (_facingRight == value) return;
            _facingRight = value;
            FlipSpriteGroup();
        }
    }

    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        Debug.Log("current = " + face.sortingOrder);
    }
    void Init(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    // 따라다니는 아이들의 sprite는 모두 default로
    public void SetEquipmentSprites(WeaponData wd)
    {
        Init(wd.Animators.InGamePlayerAnim);

        costumeSR.sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[1].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[2].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[3].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[4].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        face.sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[0].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        if (wd.DefaultHead != null)
        {
            sr[1].sprite = wd.DefaultHead;
            
        }
        if (wd.DefaultChest != null)
        {
            sr[2].sprite = wd.DefaultChest;
            
        }
        if (wd.DefaultFace != null)
        {
            sr[3].sprite = wd.DefaultFace;
            
        }
        if (wd.DefaultHands != null)
        {
            sr[4].sprite = wd.DefaultHands;
        }

        if (wd.costume != null)
        {
            costume = wd.costume;
            costumeSR.color = new Color(1, 1, 1, 1);
            Debug.Log("costume name = " + costume.name);
        }
        else
        {
            costumeSR.color = new Color(1, 1, 1, 0);
        }
    }
    /// <summary>
    /// 리드 오리의 장비 초기화
    /// GameManager의 Starting Data Container에서 weapon data, item data를 불러오니까 매개변수가 필요없다. 
    /// </summary>
    public void SetPlayerEquipmentSprites()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        List<Item> iDatas = GameManager.instance.startingDataContainer.GetItemDatas();

        Init(wd.Animators.InGamePlayerAnim);
        anim.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;

        if (wd.costume != null)
        {
            costume = wd.costume;
            costumeSR.color = new Color(1, 1, 1, 1);

            costumeSR.sortingOrder = indexSortingOrder;
            indexSortingOrder--;
        }
        else
        {
            costumeSR.color = new Color(1, 1, 1, 0);
        }

        for (int i = 0; i < 4; i++)
        {
            sr[i + 1].sortingOrder = indexSortingOrder;
            indexSortingOrder--;

            if (iDatas[i] == null)
            {
                sr[i + 1].gameObject.SetActive(false);
                continue;
            }

            sr[i + 1].sprite = iDatas[i].charImage;
        }

        Debug.Log("face sorting order = " + indexSortingOrder);
        face.sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[0].sortingOrder = indexSortingOrder;
        indexSortingOrder--;
    }
    void FlipSpriteGroup()
    {
        transform.eulerAngles += new Vector3(0, 180f, 0);
    }
    /// <summary>
    /// 속도 인자를 받아서 idle anim 혹은 run anim 적용. 코스튬이 있다면 코스튬에도 애님 적용
    /// </summary>
    public void SetAnimState(float speed)
    {
        anim.SetFloat("Speed", speed);
    }
    // 애니메이션 이벤트
    public void SetCostumeSprite(int _index)
    {
        if (costume == null) return;
        costumeSR.sprite = costume.sprites[_index];
    }
    public void ParentWeaponObjectTo(int _index, Transform _weaponObject, bool _needParent)
    {
        if (_needParent == false) // 페어런트 시킬 필요가 없는 무기라면 아무것도 하지 않는다
        {
            // 해당 부위의 스프라이트는 비활성화 시켜서 겹치지 않게 한다
            if(_index < 4)
            {
                sr[_index + 1].gameObject.SetActive(false);
            }
            return;
        }

        if (_index == 0) // 머리 부위이면
        {
            _weaponObject.SetParent(headGroup);
        }
        if (_index == 1) // 가슴 부위이면
        {
            _weaponObject.SetParent(chestGroup);
        }
        if (_index == 2) // 얼굴 부위이면
        {
            _weaponObject.SetParent(headGroup);
        }
        if (_index == 3) // 손 부위이면
        {
            _weaponObject.SetParent(handsGroup);
        }
        if (_index == 4) // 그냥 몸에 붙여서 움직이는 무기라면
        {
            // 겹치는 스프라이트가 없을테니 여기서 끝
            _weaponObject.SetParent(transform);
            return;
        }
        // 해당 부위의 스프라이트는 비활성화 시켜서 겹치지 않게 한다
        _weaponObject.position = sr[_index + 1].GetComponent<Transform>().position;
        sr[_index + 1].gameObject.SetActive(false);
    }
    // 첫 번째 무기는 starting data 혹은 weaponData에서 스프라이트를 받아오지만
    public void SetWeaponToolSpriteRenderer(SpriteRenderer _sp, Sprite _sprite)
    {
        sprite = _sprite;
        _sp.sprite = sprite;
    }
    // 두 번째 무기부터는 첫 번째 무기의 스프라이트를 받아와서 주입한다.
    public void SetExtraWeaponToolSpriteRenderer(SpriteRenderer _sp)
    {
        _sp.sprite = sprite;
    }
}
