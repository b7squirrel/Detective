using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShadowHeight : MonoBehaviour
{
    public UnityEvent onGroundHitEvent;
    public Transform trnsObject;
    public Transform trnsBody;
    public Transform trnsShadow;

    public float gravity = -10f;
    public Vector2 groundVelocity;
    public float verticalVelocity;
    float lastInitaialVerticalVelocity;
    public bool isGrounded;

    ShadowSprite shadowSprite;

    void Update()
    {
        UpdatePosition();
        CheckGroundHit();
    }

    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        isGrounded = false;
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;
        shadowSprite = GetComponent<ShadowSprite>();
    }
    void UpdatePosition()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            trnsBody.position += new Vector3(0, verticalVelocity, 0) * Time.deltaTime;
        }

        trnsObject.position += (Vector3)groundVelocity * Time.deltaTime;
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
