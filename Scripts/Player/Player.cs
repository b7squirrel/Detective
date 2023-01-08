using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 InputVec { get; private set; }
    [SerializeField] float speed;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    [field : SerializeField]
    public float FacingDir { get; private set; } = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        SubscribeOnDie();
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
        InputVec= context.ReadValue<Vector2>();
    }

    void ApplyMovement()
    {
        Vector2 nextVec = InputVec.normalized * speed * Time.fixedDeltaTime;
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

    #region OnDead Event
    void SubscribeOnDie()
    {
        Character character = GetComponent<Character>();
        character.OnDie += Die;
    }
    void Die()
    {
        anim.SetTrigger("Dead");
        rb.mass = 10000f;
    }
    #endregion
}
