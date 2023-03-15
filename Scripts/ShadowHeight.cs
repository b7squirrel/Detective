using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeight : MonoBehaviour
{
    [SerializeField] Vector2 offset = new Vector2(.3f, -.17f);
    public UnityEvent onGroundHitEvent;
    Transform trnsObject; // 부모 물체
    Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    Transform trnsShadow; // 그림자 스프라이트 오브젝트

    SpriteRenderer sprRndBody;
    SpriteRenderer sprRndshadow;

    float gravity = -300f;
    Vector2 groundVelocity;
    float verticalVelocity;
    float lastInitaialVerticalVelocity;
    bool isGrounded;
    bool isInitialized;

    void Update()
    {
        UpdatePosition();
        UpdateShadow();
        CheckGroundHit();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
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

        // 속도 초기화
        isGrounded = false;
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;

        isInitialized = true;
    }
    void UpdatePosition()
    {
        if (!isInitialized)
            Initialize(new Vector2(0, 0), 100f);

        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            trnsBody.position += new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        }

        trnsObject.position += (Vector3)groundVelocity * Time.deltaTime;
    }
    void UpdateShadow()
    {
        sprRndshadow.sprite = sprRndBody.sprite;
        sprRndshadow.flipX = sprRndBody.flipX;
        sprRndshadow.flipY = sprRndBody.flipY;

        if (isGrounded)
        {
            trnsShadow.position = new Vector2(trnsObject.position.x + offset.x,
                        trnsObject.position.y + offset.y);
        
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
        }
    }
    void GroundHit()
    {
        onGroundHitEvent?.Invoke();
    }

    // Unity Event에 붙일 함수들
    public void Stick()
    {
        groundVelocity = Vector2.zero;
    }
    public void Bounce(float divisionFactor)
    {
        Initialize(groundVelocity, lastInitaialVerticalVelocity / divisionFactor);
    }
    public void SlowDownGroundVelocity(float divisionFactor)
    {
        groundVelocity = groundVelocity / divisionFactor;
    }
}
