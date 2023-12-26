using UnityEngine;
using UnityEngine.Events;

public class ProjectileHeight : MonoBehaviour
{
    [SerializeField] Vector2 offset = new Vector2(.3f, -.17f);
    [SerializeField] int bouncingNumbers;
    public bool IsDone { get; private set; } // 다른 클래스에서 접근해서 이후의 동작을 진행
    public UnityEvent onHitGround;

    Transform trnsObject; // 부모 물체
    Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    Transform trnsShadow; // 그림자 스프라이트 오브젝트

    SpriteRenderer sprRndBody;
    SpriteRenderer sprRndshadow;
    Rigidbody2D rb;

    float gravity = -100f;
    float verticalVelocity;
    float initVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    [SerializeField] float sizeRateForHeight;

    void Update()
    {
        UpdatePosition();
        UpdateShadow();
        CheckGroundHit();
    }

    public void Initialize(float verticalVelocity)
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
            sprRndshadow.color = new Color(0, 0, 0, .5f);
            sprRndshadow.sortingLayerName = "ShadowOver";

            IsDone = false;

            isInitialized = true;
        }

        isGrounded = false;
        this.verticalVelocity = verticalVelocity;
        initVerticalVelocity = verticalVelocity;
    }

    void UpdatePosition()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            trnsBody.position += new Vector3(0, verticalVelocity, 0) * Time.deltaTime;

            float verticalVelocityRate = Mathf.Abs(verticalVelocity / initVerticalVelocity);
            float sizeFactor = 1f + (1 - (verticalVelocityRate * sizeRateForHeight));
            trnsBody.localScale = sizeFactor * Vector2.one;
        }
    }
    void UpdateShadow()
    {
        trnsShadow.position = trnsObject.position;
        sprRndshadow.sprite = sprRndBody.sprite;
        sprRndshadow.flipX = sprRndBody.flipX;
        sprRndshadow.flipY = sprRndBody.flipY;
    }

    void CheckGroundHit()
    {
        if (trnsBody.position.y < trnsObject.position.y && !isGrounded)
        {
            trnsBody.position = trnsObject.position;
            //GetComponent<BombProjectile>().Explode();
            onHitGround?.Invoke(); // Bomb projectile의 Explode함수를 끌어다 놓았음
        }
    }
}
