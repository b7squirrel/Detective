using UnityEngine;
using UnityEngine.Events;

public class ShadowHeight : MonoBehaviour
{
    Vector2 shadowOffset = new Vector2(.1f, -.2f);
    [SerializeField] int bouncingNumbers;
    [SerializeField] bool noHeightShadow;
    [SerializeField] string onLandingMask = "Shadow";
    public bool IsDone { get; private set; }
    public bool IsGrounded => isGrounded; // ✅ 추가: 외부에서 착지 여부를 읽을 수 있는 공개 프로퍼티
    public UnityEvent onGroundHitEvent;

    Transform trnsObject; // 부모 물체
    Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    Transform trnsShadow; // 그림자 스프라이트 오브젝트

    // 필드 추가
    float bowMaxHeight;
    bool isBowArc;

    SpriteRenderer sprRndBody;
    SpriteRenderer sprRndshadow;

    float gravity = -100f;
    Vector2 groundVelocity;
    Vector2 pastGroundVelocity;
    float verticalVelocity;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    Animator anim;

    // 목표 위치 기반 포물선을 위한 새로운 변수들
    Vector2 startPosition;
    Vector2 targetPosition;
    float trajectoryTime;
    float currentTime;
    bool useTargetBasedTrajectory;

    [Tooltip("0.01 - 0.05 사이에서 조절")]
    [SerializeField] float sizeRateForHeight; // 높이당 스케일 증가율
    Vector3 originalBodyScale; // 초기 스케일 저장

    void FixedUpdate()
    {
        if (!isInitialized) return;

        if (isBowArc)
        {
            UpdateBowArcPosition();
        }
        else if (useTargetBasedTrajectory)
        {
            UpdateTargetBasedPosition();
        }
        else
        {
            UpdatePosition();
        }

        UpdateShadow();
        CheckGroundHit();
    }

    // 기존 Initialize 메서드 (하위 호환성 유지)
    public void Initialize(Vector2 groundVelocity, float verticalVelocity)
    {
        InitializeCommon();

        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity;
        lastInitaialVerticalVelocity = verticalVelocity;
        useTargetBasedTrajectory = false;
    }

    // 새로운 Initialize 메서드 - 목표 위치와 비행 시간을 받음
    public void InitializeToTarget(Vector2 targetPosition, float trajectoryTime)
    {
        InitializeCommon();

        this.startPosition = trnsObject.position;
        this.targetPosition = targetPosition;
        this.trajectoryTime = trajectoryTime;
        this.currentTime = 0f;
        useTargetBasedTrajectory = true;

        // 포물선 계산을 위한 초기 속도 계산
        CalculateTrajectoryVelocities();
    }

    // 새로운 Initialize 메서드 - 목표 위치와 최대 높이를 받음
    public void InitializeWithHeight(Vector2 targetPosition, float maxHeight)
    {
        InitializeCommon();

        this.startPosition = trnsObject.position;
        this.targetPosition = targetPosition;
        useTargetBasedTrajectory = true;

        // 최대 높이를 기반으로 비행 시간 계산
        float distance = Vector2.Distance(startPosition, targetPosition);
        float timeToMaxHeight = Mathf.Sqrt(2f * maxHeight / Mathf.Abs(gravity));
        this.trajectoryTime = timeToMaxHeight * 2f; // 올라갔다 내려오는 시간
        this.currentTime = 0f;

        // 포물선 계산을 위한 초기 속도 계산
        CalculateTrajectoryVelocities();
    }

    void InitializeCommon()
    {
        if (!isInitialized)
        {
            // trnsObject, trnsBody, trnsShadow 초기화
            trnsObject = transform;
            sprRndBody = GetComponentInChildren<SpriteRenderer>();

            trnsBody = sprRndBody.transform;

            originalBodyScale = trnsBody.localScale;

            if (noHeightShadow == false)
            {
                trnsShadow = new GameObject().transform;
                trnsShadow.parent = trnsObject;
                trnsShadow.gameObject.name = "ShadowOver";
                trnsShadow.localRotation = trnsBody.localRotation;
                trnsShadow.localScale = trnsBody.localScale;

                // 그림자 만들기
                sprRndshadow = trnsShadow.gameObject.AddComponent<SpriteRenderer>();
                sprRndshadow.color = new Color(0, 0, 0, .15f);
                sprRndshadow.sortingLayerName = "ShadowOver";
            }
            isInitialized = true;
        }

        IsDone = false;
        isGrounded = false;
        isBowArc = false; // ✅ 재사용 시 이전 상태 초기화
        anim = GetComponent<Animator>();
    }

    // ✅ 활 전용 초기화 함수 (다른 무기와 완전히 독립)
    public void InitializeBowArc(Vector2 targetPosition, float maxHeight)
    {
        InitializeCommon();

        this.startPosition = trnsObject.position;
        this.targetPosition = targetPosition;
        this.bowMaxHeight = maxHeight;

        // 기존 InitializeWithHeight와 동일한 방식으로 비행 시간 산출 (체감 속도 유지)
        float timeToMaxHeight = Mathf.Sqrt(2f * maxHeight / Mathf.Abs(gravity));
        this.trajectoryTime = timeToMaxHeight * 2f;
        this.currentTime = 0f;

        // 착지 후 Bounce/SlowDownGroundVelocity가 참조할 수 있도록 실제 이동 속도로 설정
        this.groundVelocity = (targetPosition - startPosition) / trajectoryTime;

        isBowArc = true;
        useTargetBasedTrajectory = false;
    }

    // ✅ 활 전용 위치 갱신: 실제 이동과 높이감을 완전히 분리
    void UpdateBowArcPosition()
    {
        if (!isGrounded && currentTime < trajectoryTime)
        {
            currentTime += Time.fixedDeltaTime;
            float p = Mathf.Clamp01(currentTime / trajectoryTime);

            // 실제 이동: 순수 직선 보간 (높이 개념 없음)
            Vector2 realPos = Vector2.Lerp(startPosition, targetPosition, p);
            trnsObject.position = realPos;

            // 시각적 높이감: 실제 위치 위에 얹는 표준 포물선 (p=0, p=1에서 항상 0)
            float heightOffset = 4f * bowMaxHeight * p * (1f - p);
            trnsBody.position = new Vector2(realPos.x, realPos.y + heightOffset);

            Debug.Log($"p={p:F2}, bowMaxHeight={bowMaxHeight}, heightOffset={heightOffset:F2}, trnsBody.y={trnsBody.position.y:F2}, trnsObject.y={trnsObject.position.y:F2}");

            if (p >= 1f)
            {
                trnsObject.position = targetPosition;
                trnsBody.position = targetPosition;
                isGrounded = true;
                GroundHit();
            }
        }
        // else if (!IsDone && isGrounded)
        // {
        //     trnsObject.position += (Vector3)groundVelocity * Time.fixedDeltaTime;
        // }
        Debug.DrawLine(trnsObject.position, trnsBody.position, Color.red, 0f, false);
    }

    void CalculateTrajectoryVelocities()
    {
        // 수평 속도 계산
        Vector2 horizontalDistance = new Vector2(targetPosition.x - startPosition.x, 0);
        groundVelocity = horizontalDistance / trajectoryTime;

        // 수직 속도 계산 (포물선 공식 사용)
        float verticalDistance = targetPosition.y - startPosition.y;
        verticalVelocity = (verticalDistance / trajectoryTime) - (0.5f * gravity * trajectoryTime);
        lastInitaialVerticalVelocity = verticalVelocity;
    }

    void UpdateTargetBasedPosition()
    {
        if (!isGrounded && currentTime < trajectoryTime)
        {
            currentTime += Time.fixedDeltaTime;

            float t = currentTime;

            // 수평 이동 (등속 운동)
            Vector2 horizontalPos = startPosition + (targetPosition - startPosition) * (t / trajectoryTime);

            // 수직 이동 (포물선 운동)
            float verticalPos = startPosition.y + (lastInitaialVerticalVelocity * t) + (0.5f * gravity * t * t);

            // 그림자 위치 업데이트
            trnsObject.position = new Vector2(horizontalPos.x, horizontalPos.y);

            // 스프라이트가 그림자 아래로 내려가지 않도록 제한
            float clampedVerticalPos = Mathf.Max(verticalPos, horizontalPos.y);
            trnsBody.position = new Vector2(horizontalPos.x, clampedVerticalPos);

            // ✅ 새로운 도착 판정: 여러 조건 중 하나라도 만족하면 도착
            bool arrivedByTime = currentTime >= trajectoryTime * 0.95f; // 시간의 95% 도달
            bool arrivedByHeight = verticalPos <= targetPosition.y; // 수직 위치 도달
            bool arrivedByDistance = Vector2.Distance(trnsObject.position, targetPosition) < 2.0f; // 거리 2유닛 이내

            if (arrivedByTime || (arrivedByHeight && arrivedByDistance))
            {
                trnsObject.position = targetPosition;
                trnsBody.position = targetPosition;
                isGrounded = true;
                GroundHit();
            }
        }
        else if (!IsDone && isGrounded)
        {
            trnsObject.position += (Vector3)groundVelocity * Time.fixedDeltaTime;
        }
    }

    void UpdatePosition()
    {
        if (trnsBody == null) return; // 안전장치

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
        // ✅ 높이 기반 스케일 적용
        if (!isGrounded && sizeRateForHeight > 0f)
        {
            float heightAboveGround = Mathf.Max(0f, trnsBody.position.y - trnsObject.position.y);
            float sizeFactor = 1f + heightAboveGround * sizeRateForHeight;
            trnsBody.localScale = originalBodyScale * sizeFactor;
        }
        else
        {
            trnsBody.localScale = originalBodyScale; // 착지 후 원래 크기로 복원
        }

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
        // ✅ 활 전용 경로는 자체적으로 착지 판정을 하므로 여기서 제외
        if (!useTargetBasedTrajectory && !isBowArc && trnsBody.position.y < trnsObject.position.y && !isGrounded)
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

        // 바운스 시에는 기존 방식 사용
        useTargetBasedTrajectory = false;
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
        if (noHeightShadow) return;

        if (verticalVelocity < 0.1f && bouncingNumbers == 0)
        {
            sprRndshadow.color = new Color(0, 0, 0, 0f);
        }
    }
    public void ShowHeightShadow()
    {
        if (noHeightShadow) return;
        if (sprRndshadow != null)
        {
            sprRndshadow.color = new Color(0, 0, 0, 0.25f); // 원래 알파값으로 복구
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
}