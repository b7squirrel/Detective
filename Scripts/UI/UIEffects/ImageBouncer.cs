using UnityEngine;
using VHierarchy.Libs;

public class ImageBouncer : MonoBehaviour
{
    [SerializeField] RectTransform trnsImage; // 튀어 오르는 이미지
    RectTransform trnsObject; // 튀어 오르는 이미지의 부모
    float verticalVelocity;
    float horizontalVelocity;
    bool isGrounded;
    bool isInitDone = false; // init이 되어야 중력이 작용하도록
    float gravity;
    float yLandingPos; // 착지할 높이

    void Awake()
    {
        if (trnsObject == null) trnsObject = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        if (isInitDone == false) return;
        UpdatePosition();
        CheckGroundHit();
    }

    public void InitBouncer(float verticalVelocity, float horizontalVelocity, float gravity, float yLandingPos)
    {
        this.verticalVelocity = verticalVelocity;
        this.horizontalVelocity = horizontalVelocity;
        this.gravity = gravity;
        isGrounded = false;
        isInitDone = true;
        trnsImage.anchoredPosition = Vector2.zero;
        this.yLandingPos = yLandingPos;
    }
    
    void UpdatePosition()
    {
        if (isGrounded == false)
        {
            // Time.unscaledDeltaTime 사용 (타임스케일 무시)
            verticalVelocity += gravity * Time.unscaledDeltaTime;
            
            trnsImage.anchoredPosition += new Vector2(horizontalVelocity, verticalVelocity) * Time.unscaledDeltaTime;
        }
    }
    
    void CheckGroundHit()
    {
        if (trnsImage.position.y < yLandingPos && !isGrounded)
        {
            trnsImage.anchoredPosition = Vector2.zero; // 부모 기준으로 원위치
            isGrounded = true;
            isInitDone = false;

            gameObject.Destroy();
        }
    }
}