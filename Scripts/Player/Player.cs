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
    Animator[] anim = new Animator[5];
    Character character;

    [field: SerializeField]
    public float FacingDir { get; private set; } = 1f;

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
        anim[0] = GetComponent<Animator>();
        for(int i = 1; i < anim.Length; i++)
        {
            anim[i] = sr[i].GetComponent<Animator>();
        }
        character = GetComponent<Character>();
    }
    void Start()
    {
        StartingDataContainer startingDataContainer = FindObjectOfType<StartingDataContainer>();
        if (startingDataContainer != null)
        {
            anim[0].runtimeAnimatorController = startingDataContainer.GetLeadWeaponData().Animators.InGamePlayerAnim;
        }

        List<Item> iDatas = startingDataContainer.GetItemDatas();

        for (int i = 0; i < 4; i++)
        {
            if (iDatas[i] == null)
            {
                sr[i + 1].gameObject.SetActive(false);
                anim[i + 1].gameObject.SetActive(false);
                continue;
            }

            sr[i + 1].GetComponent<Animator>().runtimeAnimatorController = iDatas[i].CardItemAnimator.CardImageAnim;

            if(sr[i + 1].GetComponent<Animator>().runtimeAnimatorController == null)
            {
                sr[i+1].gameObject.SetActive(false);
            }
        }
        Debug.Log("essential indes = " + startingDataContainer.GetEssectialIndex());
        sr[startingDataContainer.GetEssectialIndex() + 1].gameObject.SetActive(false); // 필수 장비는 비활성화
    }

    void LateUpdate()
    {
        if (IsStill)
        {
            InputVec = Vector2.zero;
            return;
        }
        
        Flip();
        UpdateAniamtionState();
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
        if (InputVec.x != 0 && InputVec.x * FacingDir < 0f)
        {
            FacingDir *= -1f;
        }
        for (int i = 0; i < sr.Length; i++)
        {
            sr[i].flipX = FacingDir < 0;
        }
    }
    void UpdateAniamtionState()
    {
        anim[0].SetFloat("Speed", InputVec.magnitude);
        if (InputVec.magnitude > .01f)
        {
            for (int i = 1; i < 5; i++)
            {
                if (anim[i].gameObject.activeSelf == false)
                    continue;
                anim[i].SetBool("Walk", true);
                anim[i].SetBool("Idle", false);
            }
        }
        else if (InputVec.magnitude < .01f)
        {
            for (int i = 1; i < 5; i++)
            {
                if (anim[i].gameObject.activeSelf == false)
                    continue;
                anim[i].SetBool("Walk", false);
                anim[i].SetBool("Idle", true);
            }
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
        anim[0].SetTrigger("Dead");
        rb.mass = 10000f;
    }
    #endregion
}
