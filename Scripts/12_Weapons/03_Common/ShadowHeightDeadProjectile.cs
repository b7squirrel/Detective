using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightDeadProjectile : MonoBehaviour
{
    [Header("Body & Shadow")]
    [SerializeField] Transform trnsBody;
    [SerializeField] SpriteRenderer sprRndBody;
    [SerializeField] Transform trnsShadow;
    [SerializeField] SpriteRenderer sprRndshadow;

    Vector2 shadowOffset = new Vector2(.1f, -.2f);
    [SerializeField] int bouncingNumbers;
    [SerializeField] bool noHeightShadow;
    [SerializeField] string onLandingMask;
    
    // IsDone을 프로퍼티로 변경
    private bool isDone;
    public bool IsDone 
    { 
        get => isDone;
        private set 
        {
            if (!isDone && value) // false에서 true로 변경될 때만
            {
                isDone = value;
                onDoneEvent?.Invoke(); // 이벤트 실행
            }
            else
            {
                isDone = value;
            }
        }
    }
    
    public UnityEvent onGroundHitEvent;
    public UnityEvent onDoneEvent; // 새로운 이벤트 추가

    Transform trnsObject;

    float gravity = -100f;
    Vector2 groundVelocity;
    Vector2 pastGroundVelocity;
    float verticalVelocity;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    Vector2 startPosition;
    Vector2 targetPosition;
    float trajectoryTime;
    float currentTime;
    bool useTargetBasedTrajectory;

    void Update()
    {
        UpdatePosition();
        UpdateShadow();
        CheckGroundHit();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity, Sprite sprite)
    {
        InitializeCommon();
        
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;
        useTargetBasedTrajectory = false;

        if(sprite != null) sprRndBody.sprite = sprite;
    }

    void InitializeCommon()
    {
        if (!isInitialized)
        {
            trnsObject = transform;
            isInitialized = true;
        }
        
        IsDone = false;
        isGrounded = false;
        anim = GetComponent<Animator>();
    }

    void CalculateTrajectoryVelocities()
    {
        Vector2 horizontalDistance = new Vector2(targetPosition.x - startPosition.x, 0);
        groundVelocity = horizontalDistance / trajectoryTime;

        float verticalDistance = targetPosition.y - startPosition.y;
        verticalVelocity = (verticalDistance / trajectoryTime) - (0.5f * gravity * trajectoryTime);
        lastInitaialVerticalVelocity = verticalVelocity;
    }

    void UpdatePosition()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            trnsBody.position += new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        }
        if (IsDone == false)
        {
            trnsObject.position += (Vector3)groundVelocity * Time.deltaTime;
        }
    }

    void UpdateShadow()
    {
        if (noHeightShadow == false)
        {
            sprRndshadow.sprite = sprRndBody.sprite;
            sprRndshadow.flipX = sprRndBody.flipX;
            sprRndshadow.flipY = sprRndBody.flipY;

            trnsShadow.localRotation = trnsBody.localRotation;
            trnsShadow.localScale = trnsBody.localScale;
        }

        if (isGrounded)
        {
            sprRndBody.sortingLayerName = onLandingMask;
            if (noHeightShadow == false)
            {
                trnsShadow.position = new Vector2(trnsObject.position.x + shadowOffset.x,
                        trnsObject.position.y + shadowOffset.y);

                sprRndshadow.sortingLayerName = "Shadow";
            }
        }
        else
        {
            sprRndBody.sortingLayerName = "FloatingOver";

            if (noHeightShadow == false)
            {
                trnsShadow.position = new Vector2(trnsObject.position.x,
                        trnsObject.position.y);

                sprRndshadow.sortingLayerName = "ShadowOver";
            }
        }
    }

    void CheckGroundHit()
    {
        if (!useTargetBasedTrajectory && trnsBody.position.y < trnsObject.position.y && !isGrounded)
        {
            trnsBody.position = trnsObject.position;
            isGrounded = true;
            GroundHit();
        }
    }

    void GroundHit()
    {
        if (IsDone) return;
        onGroundHitEvent?.Invoke();
    }

    public void Stick()
    {
        groundVelocity = Vector2.zero;
    }

    public void Bounce(float divisionFactor)
    {
        if (IsDone)
        {
            groundVelocity = Vector2.zero;
            return;
        }
        if (bouncingNumbers < 1)
        {
            IsDone = true;
            return;
        }
        
        useTargetBasedTrajectory = false;
        Initialize(groundVelocity, lastInitaialVerticalVelocity / divisionFactor, null);
        bouncingNumbers--;
    }

    public void SlowDownGroundVelocity(float divisionFactor)
    {
        if (IsDone) return;
        if (Vector2.Distance(groundVelocity, pastGroundVelocity) < .1f)
        {
            IsDone = true;
            groundVelocity = Vector2.zero;
            return;
        }
        groundVelocity = groundVelocity / divisionFactor;
        pastGroundVelocity = groundVelocity;
    }

    public void RemoveHeigihtShadow()
    {
        if (noHeightShadow) return;

        if (verticalVelocity < 0.1f && bouncingNumbers == 0)
        {
            sprRndshadow.color = new Color(0, 0, 0, 0f);
        }
    }

    public void SetToKinematic()
    {
        if (IsDone)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }

    public void TriggerIdleAnim()
    {
        if (bouncingNumbers == 0)
        {
            anim.SetTrigger("Idle");
        }
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}