using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim;
    [SerializeField] SpriteRenderer body, head, chest, face, hands;
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] Transform spriteGroup;
    [SerializeField] Animator costume;
    Transform headGroup; // 머리와 함께 움직이는 장비들은 모두 여기에 페어런트 시킨다
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
    public void SetPlayerEquipmentSprites(WeaponData wd, List<Item> itemDatas)
    {
        Init(wd.Animators.InGamePlayerAnim);
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

        if (itemDatas[3] == null)
        {
            hands.gameObject.SetActive(false);
        }
        else
        {
            hands.sprite = itemDatas[3].charImage;
        }

        if (wd.hideEssentialEquipmentOnPlay)
        {

        }

        wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        anim.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;

        List<Item> iDatas = GameManager.instance.startingDataContainer.GetItemDatas();

        for (int i = 0; i < 4; i++)
        {
            if (iDatas[i] == null)
            {
                sr[i + 1].gameObject.SetActive(false);
                continue;
            }

            if (i < 3)
            {
                sr[i + 1].sprite = iDatas[i].charImage;
                sr[i + 1].GetComponent<Transform>().SetParent(headGroup);
                // sr[i + 1].GetComponent<Transform>().localPosition = Vector3.zero;
            }
        }

        // 오리의 Idle 모션에 맞춰야 한다면
        if (wd.needToSyncIdle)
        {
            syncIdleAnim.SetIdleSync(true);
        }
        // 겹치지 않도록 Essential Weapon을 숨겨야 한다면
        if (wd.hideEssentialEquipmentOnPlay)
        {
            sr[GameManager.instance.startingDataContainer.GetEssectialIndex() + 1].gameObject.SetActive(false); // 필수 장비를 비활성화
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
}
