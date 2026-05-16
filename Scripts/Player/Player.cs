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

    [Header("Joystic")]
    public FloatingJoystick joy;

    bool isBouncing;
    Vector2 bouncingForce;
    Coroutine bouncingCoroutine;

    // 점액 위에 있을 때 속도가 느려지게 하기 위한 인자. 
    // 속도 업그레이드가 되어도 여전히 일정 비율로 느려지도록 직접 speed를 건드리지 않고 slowDownFactor로 속도 제어
    float slowDownFactor;
    bool isIceMode;
    float iceSlideDecay;
    Vector2 iceVelocity;
    float iceAcceleration; // 현재 가속도
    Vector2 windForce;

    public bool ShouldBeStill { get; set; } // 메뉴, 이벤트 등 플레이어가 움직이면 안되는 상황

    void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        character = GetComponent<Character>();

        slowDownFactor = 1f;
    }

    void Start()
    {
        GameEvents.OnGameStart?.Invoke();
    }

    void LateUpdate()
    {
        if (ShouldBeStill ||
            GameManager.instance.IsPaused ||
            GameManager.instance.IsPlayerDead)
        {
            InputVec = Vector2.zero;
            return;
        }

        Flip();
    }
    void FixedUpdate()
    {
        if (ShouldBeStill ||
            GameManager.instance.IsPlayerDead)
        {
            InputVec = Vector2.zero;
            return;
        }
        ApplyMovement();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
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

        if (isIceMode)
        {
            if (InputVec != Vector2.zero)
            {
                iceAcceleration = 1f; // 처음부터 최고 가속

                Vector2 targetVelocity = InputVec * character.MoveSpeed * 6f * Time.fixedDeltaTime;

                // 현재 속도가 낮으면 빠르게 반응, 속도가 붙으면 방향전환 느려짐
                float currentSpeed = iceVelocity.magnitude;
                float maxSpeed = character.MoveSpeed * Time.fixedDeltaTime;
                float lerpFactor = Mathf.Lerp(0.3f, 0.03f, currentSpeed / maxSpeed);
                // 0.3f: 처음 시작 시 반응속도 / 0.03f: 고속 시 방향전환 반응속도

                iceVelocity = Vector2.Lerp(iceVelocity, targetVelocity, lerpFactor);
            }
            else
            {
                iceAcceleration = 0f;
            }

            iceVelocity *= iceSlideDecay;
            rb.MovePosition(rb.position + iceVelocity);
        }
        else  // ← 이게 없으면 아이스 모드가 아닐 때 이동 코드가 실행되지 않습니다
        {
            Vector2 nextVec = InputVec * character.MoveSpeed * slowDownFactor * Time.fixedDeltaTime;
                nextVec += windForce * Time.fixedDeltaTime; // 바람 힘 추가
                rb.MovePosition(rb.position + nextVec);
        }
    }
    void Flip()
    {
        if (GameManager.instance.IsPaused) return;

        if (weaponContainerAnim == null) weaponContainerAnim = GetComponentInChildren<WeaponContainerAnim>();

        if (InputVec.x != 0 && InputVec.x * FacingDir < 0f)
        {
            FacingDir *= -1f;
        }

        if (FacingDir < 0)
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

    public bool IsActuallyMoving()
    {
        if (isIceMode)
            return iceVelocity.magnitude > 0.001f; // 실제 이동 속도 기준
        return InputVec != Vector2.zero;
    }

    public void SetSlowDownFactor(float factor)
    {
        slowDownFactor = factor;
    }
    public void ResetSlowDownFactor()
    {
        slowDownFactor = 1f;
    }
    public void SetWindForce(Vector2 force)
    {
        windForce = force;
    }

    public void EnableIceMode(bool enabled, float decay)
    {
        isIceMode = enabled;
        iceSlideDecay = decay;
        iceVelocity = Vector2.zero;
        iceAcceleration = 0f;
    }

    #region OnDead Event
    public void Die()
    {
        //anim.SetTrigger("Dead");
        rb.mass = 10000f;
    }
    #endregion
}