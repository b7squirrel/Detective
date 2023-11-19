using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviour, IBouncable
{
    public static Player instance;
    public Vector2 InputVec { get; private set; }
    Vector2 pastInputVec;
    Rigidbody2D rb;
    [SerializeField] SpriteRenderer[] sr;
    [SerializeField] Transform spriteGroup;
    Animator anim;
    Character character;

    WeaponContainerAnim weaponContainerAnim;
    public WeaponData wd;
    public List<Item> iDatas;
    [SerializeField] Transform faceGroup;

    [field: SerializeField]
    public float FacingDir { get; private set; } = 1f;
    public bool IsPauseing { get; set; }

    [Header("Joystic")]
    public FloatingJoystick joy;

    bool isBouncing;
    Vector2 bouncingForce;
    Coroutine bouncingCoroutine;

    public bool IsStill { get; set; } // 메뉴, 이벤트 등 플레이어가 움직이면 안되는 상황

    void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        character = GetComponent<Character>();
    }

    void LateUpdate()
    {
        if (IsStill)
        {
            InputVec = Vector2.zero;
            return;
        }

        Flip();
    }
    void FixedUpdate()
    {
        if (GameManager.instance.IsPlayerDead)
        {
            InputVec = Vector2.zero;
            return;
        }
        ApplyMovement();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        InputVec = context.ReadValue<Vector2>();
    }

    void ApplyMovement()
    {
        if (isBouncing)
        {
            rb.velocity = bouncingForce;
            return;
        }
        if (bouncingCoroutine != null)
            StopCoroutine(bouncingCoroutine);

#if UNITY_EDITOR

#elif UNITY_ANDROID
        InputVec = new Vector2(joy.Horizontal, joy.Vertical).normalized;
#endif

        Vector2 nextVec = InputVec * character.MoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
    }
    void Flip()
    {
        if (IsPauseing) return;

        if (weaponContainerAnim == null) weaponContainerAnim = GetComponentInChildren<WeaponContainerAnim>();

        if (InputVec.x != 0 && InputVec.x * FacingDir < 0f)
        {
            FacingDir *= -1f;
        }

        if(FacingDir < 0) 
        {
            weaponContainerAnim.FacingRight = false;
        }
        else
        {
            weaponContainerAnim.FacingRight = true;
        }
    }

    public void GetBounced(float bouncingForce, Vector2 direction, float bouncingTime)
    {
        this.bouncingForce = bouncingForce * direction;
        bouncingCoroutine = StartCoroutine(GetBouncedCo(bouncingTime));
    }
    IEnumerator GetBouncedCo(float bouncingTime)
    {
        isBouncing = true;
        yield return new WaitForSeconds(bouncingTime);
        isBouncing = false;
    }

    public bool IsPlayerMoving()
    {
        if (InputVec == Vector2.zero)
            return false;
        return true;
    }

    #region OnDead Event
    public void Die()
    {
        //anim.SetTrigger("Dead");
        rb.mass = 10000f;
    }
    #endregion
}
