using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightEnemy : MonoBehaviour
{
    [SerializeField] int bouncingNumbers;
    [SerializeField] string onLandingMask;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent;

    [SerializeField] Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    [SerializeField] SpriteRenderer sprRndBody; // 공중에 뜨는 오브젝트의 스프라이트
    [SerializeField] SpriteRenderer sprRndshadow; // 그림자 스프라이트. sorting order 변경을 위해

    [SerializeField] Enemy enemy; // 수평 속도를 가져오기 위해
    bool isJumper; // 점프를 하는 캐릭터인지 판별
    [SerializeField] float verticalVelocity; // 수직 점프 속도
    [SerializeField] float jumpInterval; // 점프를 하는 주기
    float currentVerticalVel;
    float jumpCounter; // 점프 주기를 재는 카운터

    float gravity = -100f;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    #region Update
    void Update()
    {
        if(isJumper == false) return; // 점프를 할 수 있는 캐릭터가 아니라면 아무것도 하지 않기
        UpdatePosition();
        UpdateLayers();
        CheckGroundHit();
        CountJumpCounter();
    }
    #endregion

    #region 초기화
    public void Initialize(float verticalVel)
    {
        if (!isInitialized)
        {
            //초기화
            sprRndBody = transform.GetComponentInChildren<SpriteRenderer>();
            sprRndshadow.sortingLayerName = "Shadow";
            isInitialized = true;
        }
        IsDone = false;

        ActivateCollider(false);

        isGrounded = false;
        currentVerticalVel = verticalVel;
        lastInitaialVerticalVelocity = verticalVel;

        anim = GetComponent<Animator>();
    }

    public void SetIsJumper(bool isJumper)
    {
        this.isJumper = isJumper;
    }
    #endregion

    #region 매 프레임마다 할 일
    void CountJumpCounter()
    {
        if(isGrounded) jumpCounter += Time.deltaTime; // 점프를 하고 있지 않을 때만 카운터가 돌아간다
        if (jumpCounter >= jumpInterval)
        {
            jumpCounter = 0;
            Initialize(verticalVelocity);
            Debug.Log("Jump 실행");
        }
    }

    void UpdatePosition()
    {
        if (!isGrounded)
        {
            currentVerticalVel += gravity * Time.deltaTime;
            trnsBody.position += new Vector3(0, currentVerticalVel, 0) * Time.deltaTime;
        }
    }
    void UpdateLayers()
    {
        if (isGrounded)
        {
            sprRndBody.sortingLayerName = onLandingMask;
            sprRndshadow.sortingLayerName = "Shadow";

            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            sprRndBody.sortingLayerName = "InAir";
            sprRndshadow.sortingLayerName = "ShadowOver";

            gameObject.layer = LayerMask.NameToLayer("InAir");
        }
    }

    void CheckGroundHit()
    {
        if (trnsBody.position.y < transform.position.y && !isGrounded)
        {
            trnsBody.position = transform.position;
            isGrounded = true;
            GroundHit();
        }
    }
    void GroundHit()
    {
        if (IsDone) return;
        onGroundHitEvent?.Invoke();
    }
    #endregion

    #region Unity Event에 붙일 함수들
    public void Bounce(float divisionFactor)
    {
        if (bouncingNumbers < 1)
        {
            IsDone = true;
            return;
        }
        Initialize(lastInitaialVerticalVelocity / divisionFactor);
        bouncingNumbers--;
    }

    public void ActivateCollider(bool ActivateCol)
    {
        // enemy.ActivateCollider(ActivateCol);
    }
    public void TriggerAnim(string animation)
    {
        anim.SetTrigger(animation);
    }
    #endregion
}
