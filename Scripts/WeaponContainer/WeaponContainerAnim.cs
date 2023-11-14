using UnityEngine;

public class WeaponContainerAnim : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    Vector2 pastPosition, currentPosition;
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
        sr = GetComponent<SpriteRenderer>();
    }
    public void Init(RuntimeAnimatorController animCon)
    {
        anim.runtimeAnimatorController = animCon;
    }
    public void SetEquipmentSprites(RuntimeAnimatorController _anim, Sprite _head, Sprite _chest, Sprite _face, Sprite _hands)
    {
        anim.runtimeAnimatorController = _anim;
        head.sprite = _head;
        chest.sprite = _chest;
        face.sprite = _face;
        hands.sprite = _hands;
    }
    void FlipSpriteGroup()
    {
        transform.eulerAngles += new Vector3(0, 180f, 0);
    }
    public void Flip(bool flip)
    {
        // sr.flipX = flip;
        // Debug.Log("Flip = " + flip);
        // if(flip) 
        // {
        //     transform.eulerAngles = new Vector3(0, 180f, 0);
        // }
        // else
        // {
        //     spriteGroup.eulerAngles = new Vector3(0, 0, 0);
        // }
    }
}
