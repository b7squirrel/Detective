using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim; // 오리의 animator
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] SpriteRenderer face;
    [SerializeField] Transform spriteGroup;
    [SerializeField] Transform headGroup; // 머리와 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
    [SerializeField] Transform chestGroup; // 가슴과 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
    [SerializeField] Transform handsGroup; // 손과 함께 움직이는 중비들은 모두 여기에 페어런트 시킨다
    [SerializeField] GameObject[] testParts; // 테스트 파츠. 생성되면 바로 숨기도록
    StartingDataContainer startingDataContainer;

    Sprite sprite; // 개별 무기들의 sprite

    // 장비 스프라이트
    [SerializeField] SpriteRow[] equipSprites = new SpriteRow[4];
    bool initWeapon = false;

    public static int indexSortingOrder = 100; // 소팅오더를 정하기 위한 인덱스

    bool _facingRight = true;
    int essentialIndex;

    [Header("Invincible Effects")]
    Coroutine invincibleSpriteChange;

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
        
        for (int i = 0; i < testParts.Length; i++)
        {
            testParts[i].SetActive(false);
        }
    }
    void InitAnimController(RuntimeAnimatorController animCon)
    {
        if(anim == null) anim = GetComponent<Animator>();
        anim.runtimeAnimatorController = animCon;
    }
  
    void StoreSpriteRows(WeaponData _wd, bool _isInitialWeapon)
    {
        if (_isInitialWeapon) // 플레이어
        {
            if (startingDataContainer == null) startingDataContainer = GameManager.instance.startingDataContainer;
            List<Item> iData = startingDataContainer.GetItemDatas();

            for (int i = 0; i < 4; i++)
            {
                if (iData[i] != null &&
                    iData[i].spriteRow.sprites.Length > 0)
                {
                    sr[i + 1].gameObject.SetActive(true);
                    equipSprites[i] = iData[i].spriteRow;
                }
                else
                {
                    sr[i + 1].gameObject.SetActive(false);
                    equipSprites[i] = null;
                }
            }
        }
        else // 동료
        {
            for (int i = 0; i < 4; i++)
            {
                if(_wd.defaultItems[i] != null && _wd.defaultItems[i].spriteRow.sprites.Length> 0)
                {
                    sr[i + 1].gameObject.SetActive(true);
                    equipSprites[i] = _wd.defaultItems[i].spriteRow;
                    Debug.Log($"{_wd.Name}의 {i}스프라이트가 있습니다.");
                    Debug.Log($"{_wd.Name}의 {equipSprites[i].sprites[0].name}스프라이트가 있습니다.");
                }
                else
                {
                    sr[i + 1].gameObject.SetActive(false);
                    equipSprites[i] = null;
                    Debug.Log($"{_wd.Name}의 {i}스프라이트가 없습니다.");
                }
            }
        }
        face.sprite = _wd.faceImage;
        face.gameObject.SetActive(true);
    }

    // 따라다니는 아이들의 sprite는 모두 default로
    public void SetEquipmentSprites(WeaponData wd)
    {
        InitAnimController(wd.Animators.InGamePlayerAnim);
        // SetFollwersEquipSprites(wd);
        StoreSpriteRows(wd, false);
        initWeapon = false;

        // 1-head, 2-chest, 3-face, 4-hand (expression, 2chest, 1head, 3face, 4hand 순서로 배열하기)
        indexSortingOrder--;

        sr[4].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[3].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[1].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[2].sortingOrder = indexSortingOrder; // 얼굴 부위가 가슴보다 위에 오도록
        indexSortingOrder--;

        face.sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[0].sortingOrder = indexSortingOrder;
        indexSortingOrder--;
    }
    /// <summary>
    /// 리드 오리의 장비 초기화
    /// GameManager의 Starting Data Container에서 weapon data, item data를 불러오니까 매개변수가 필요없다. 
    /// </summary>
    public void SetPlayerEquipmentSprites()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        List<Item> iDatas = GameManager.instance.startingDataContainer.GetItemDatas();

        InitAnimController(wd.Animators.InGamePlayerAnim);
        anim.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;

        StoreSpriteRows(wd, true);
        initWeapon = true;

        sr[4].sortingOrder = indexSortingOrder; // 손에 든 무기가 가장 위에
        indexSortingOrder--;

        sr[3].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[1].sortingOrder = indexSortingOrder;
        indexSortingOrder--;

        sr[2].sortingOrder = indexSortingOrder; // 얼굴 부위가 가슴보다 위에 오도록
        indexSortingOrder--;

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
    
    public void SetEquippedItemSprite(int _index)
    {
        if (equipSprites == null) return;
        if(initWeapon == false) Debug.Log($"equipSprites 배열의 길이는 {equipSprites.Length}입니다.");

        for (int i = 0; i < 4; i++)
        {
            if (equipSprites[i] == null) continue;
            if (equipSprites[i].sprites.Length == 1)
            {
                sr[i + 1].sprite = equipSprites[i].sprites[0];
                if(initWeapon == false) Debug.Log($"{equipSprites[i].sprites[0].name}을 sr[{i + 1}]에 넘겨줍니다.");
            }
            else
            {
                Debug.Log($"i - {i}, index - {_index}");
                sr[i + 1].sprite = equipSprites[i].sprites[_index];
                if(initWeapon == false) Debug.Log($"{equipSprites[i].sprites[_index].name}을 sr[{i + 1}]에 넘겨줍니다.");
            }
        }
    }
    public void ParentWeaponObjectTo(int _index, Transform _weaponObject, bool _needParent, int _debugIndex)
    {
        if (_needParent == false) // 페어런트 시킬 필요가 없는 무기라면 아무것도 하지 않는다
        {
            // 해당 부위의 스프라이트는 비활성화 시켜서 겹치지 않게 한다
            if (_index < 4)
            {
                sr[_index + 1].gameObject.SetActive(false);
                Debug.Log($"sr[{_index}]를 비활성화 합니다.");
            }
            return;
        }

        SpriteRenderer _sr = null;
        if (_weaponObject.GetComponentInChildren<Weapon>() != null)
        {
            _sr = _weaponObject.GetComponentInChildren<Weapon>().GetWeaponSprite();
            
        }
        if (_index == 0) // 머리 부위이면
        {
            _weaponObject.SetParent(headGroup);
            
            if(_sr != null) { _sr.sortingOrder = sr[1].sortingOrder; }
        }
        if (_index == 1) // 가슴 부위이면
        {
            _weaponObject.SetParent(chestGroup);
            if (_sr != null) { _sr.sortingOrder = sr[2].sortingOrder; }
        }
        if (_index == 2) // 얼굴 부위이면
        {
            _weaponObject.SetParent(headGroup);
            if (_sr != null) { _sr.sortingOrder = sr[3].sortingOrder; }
        }
        if (_index == 3) // 손 부위이면
        {
            _weaponObject.SetParent(handsGroup);
            if (_sr != null) { _sr.sortingOrder = sr[4].sortingOrder; }
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
    #region Invincible
    public void SetSpritesInvincible(bool _isInvincible)
    {
        // 먼저 진행되고 있던 코루틴이 있다면 종료
        if (invincibleSpriteChange != null) StopCoroutine(invincibleSpriteChange);

        if (_isInvincible)
        {
            invincibleSpriteChange = StartCoroutine(InvincibleSpriteChangeCo());
        }
        else
        {
            SetSpriteColorToDefault();
        }
    }
    IEnumerator InvincibleSpriteChangeCo()
    {
        while(true)
        {
            Color randomColor = Colors.randomColors[Random.Range(0, Colors.randomColors.Length)];
            for (int i = 0; i < sr.Length; i++)
            {
                sr[i].color = randomColor;
            }
            if (face != null) face.color = randomColor;
            yield return null;
        }
    }
    void SetSpriteColorToDefault()
    {
        Color defaultColor = new Color(1, 1, 1, 1);
        for (int i = 0; i < sr.Length; i++)
        {
            sr[i].color = defaultColor;
        }
        if (face != null) face.color = defaultColor;
    }
    #endregion
}