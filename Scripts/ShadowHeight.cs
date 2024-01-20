using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeight : MonoBehaviour
{
    Vector2 shadowOffset = new Vector2(.07f, -.07f);
    [SerializeField] int bouncingNumbers;
    [SerializeField] bool noHeightShadow;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent;

    Transform trnsObject; // 부모 물체
    Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    Transform trnsShadow; // 그림자 스프라이트 오브젝트

    SpriteRenderer sprRndBody;
    SpriteRenderer sprRndshadow;
    Rigidbody2D rb;

    float gravity = -100f;
    Vector2 groundVelocity;
    Vector2 pastGroundVelocity;
    float verticalVelocity;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;

    Animator anim;

    void Update()
    {
        UpdatePosition();
        UpdateShadow();
        CheckGroundHit();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        if (!isInitialized)
        {
            // trnsObject, trnsBody, trnsShadow 초기화
            trnsObject = transform;
            sprRndBody = GetComponentInChildren<SpriteRenderer>();
            trnsBody = sprRndBody.transform;
            trnsShadow = new GameObject().transform;
            trnsShadow.parent = trnsObject;
            trnsShadow.gameObject.name = "ShadowOver";
            trnsShadow.localRotation = Quaternion.identity;

            // 그림자 만들기
            sprRndshadow = trnsShadow.gameObject.AddComponent<SpriteRenderer>();
            sprRndshadow.color = new Color(0, 0, 0, .25f);
            sprRndshadow.sortingLayerName = "ShadowOver";

            isInitialized = true;
        }
        IsDone = false;

        isGrounded = false;
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;

        anim = GetComponent<Animator>();
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
        sprRndshadow.sprite = sprRndBody.sprite;
        sprRndshadow.flipX = sprRndBody.flipX;
        sprRndshadow.flipY = sprRndBody.flipY;

        if (isGrounded)
        {
            trnsShadow.position = new Vector2(trnsObject.position.x + shadowOffset.x,
                        trnsObject.position.y + shadowOffset.y);

            sprRndBody.sortingLayerName = "Enemy";
            sprRndshadow.sortingLayerName = "Shadow";
        }
        else
        {
            trnsShadow.position = new Vector2(trnsObject.position.x,
                        trnsObject.position.y);

            sprRndBody.sortingLayerName = "FloatingOver";
            sprRndshadow.sortingLayerName = "ShadowOver";
        }
    }

    void CheckGroundHit()
    {
        if (trnsBody.position.y < trnsObject.position.y && !isGrounded)
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

    // Unity Event에 붙일 함수들
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
        Initialize(groundVelocity, lastInitaialVerticalVelocity / divisionFactor);
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
        if (noHeightShadow == false)
            return;
        if (verticalVelocity < 0.1f && bouncingNumbers == 0)
        {
            sprRndshadow.color = new Color(0, 0, 0, 0f);
        }
    }

    public void TriggerIdleAnim()
    {
        if (verticalVelocity < 0.1f && bouncingNumbers == 0)
        {
            anim.SetTrigger("Idle");
        }
    }
}
