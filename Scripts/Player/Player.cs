using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IBouncable
{
    public Vector2 InputVec { get; private set; }
    Vector2 pastInputVec;
    Rigidbody2D rb;
    [SerializeField] SpriteRenderer sr;
    Animator anim;
    Character character;

    [field: SerializeField]
    public float FacingDir { get; private set; } = 1f;

    [Header("Joystic")]
    public FloatingJoystick joy;

    bool isBouncing;
    Vector2 bouncingForce;
    Coroutine bouncingCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        character = GetComponent<Character>();
    }

    void LateUpdate()
    {
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

        InputVec = new Vector2(joy.Horizontal, joy.Vertical).normalized;
        Vector2 nextVec = InputVec * character.MoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
    }
    void Flip()
    {
        if (InputVec.x != 0 && InputVec.x * FacingDir < 0f)
        {
            FacingDir *= -1f;
        }
        sr.flipX = FacingDir < 0;
    }
    void UpdateAniamtionState()
    {
        anim.SetFloat("Speed", InputVec.magnitude);
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
        anim.SetTrigger("Dead");
        rb.mass = 10000f;
    }
    #endregion
}
