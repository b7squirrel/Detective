using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim;
    [SerializeField] SpriteRenderer body, head, chest, face, hands;
    [SerializeField] Transform spriteGroup;
    [SerializeField] Animator costume;
    bool _facingRight = true;
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
    public void Init(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    public void SetEquipmentSprites(WeaponData wd)
    {
        anim.runtimeAnimatorController = wd.animatorController;
        head.sprite = wd.DefaultHead;
        chest.sprite = wd.DefaultChest;
        face.sprite = wd.DefaultFace;
        hands.sprite = wd.DefaultHand;
    }
    public void SetPlayerEquipmentSprites(WeaponData wd, List<Item> itemDatas)
    {
        anim.runtimeAnimatorController = wd.animatorController;
        if (itemDatas[0] == null)
        {
            head.gameObject.SetActive(false);
        }
        else
        {
            head.sprite = itemDatas[0].charImage;
        }

        if (itemDatas[1] == null)
        {
            chest.gameObject.SetActive(false);
        }
        else
        {
            chest.sprite = itemDatas[1].charImage;
        }

        if (itemDatas[2] == null)
        {
            face.gameObject.SetActive(false);
        }
        else
        {
            face.sprite = itemDatas[2].charImage;
        }

        // if(itemDatas[3] == null) 
        // {
        //     hands.gameObject.SetActive(false);
        // }
        // else
        // {
        //     hands.sprite = itemDatas[3].charImage;
        // }
    }
    void FlipSpriteGroup()
    {
        transform.eulerAngles += new Vector3(0, 180f, 0);
    }
    public void SetAnimState(float speed)
    {
        anim.SetFloat("Speed", speed);
    }
}
