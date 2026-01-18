using UnityEngine;
using UnityEngine.Events;

public class ShadowHeightEnemy : MonoBehaviour
{
    [SerializeField] int bouncingNumbers;
    [SerializeField] string onLandingMask;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip landingSound;
    [SerializeField] GameObject landingEffectPrefab;
    [SerializeField] Transform landingEffectTrans;
    public bool IsDone { get; private set; }
    public UnityEvent onGroundHitEvent;
    public UnityEvent onDone; // 지면에서 완전히 멈췄을 떄의 이벤트

    [SerializeField] Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    [SerializeField] SpriteRenderer sprRndBody; // 공중에 뜨는 오브젝트의 스프라이트
    [SerializeField] SpriteRenderer sprRndshadow; // 그림자 스프라이트. sorting order 변경을 위해

    [SerializeField] Enemy enemy; // 수평 속도를 가져오기 위해
    bool isJumper; // 점프를 하는 캐릭터인지 판별
    [SerializeField] float verticalVelocity; // 수직 점프 속도
    float jumpFrequency; // 점프를 하는 주기
    float currentVerticalVel;
    float jumpCounter; // 점프 주기를 재는 카운터

    [Header("Slow Effect")]
    bool isSlowed; // 현재 느림 상태인지
    float originalVerticalVelocity; // 원래 점프 속도
    float originalGravity; // 원래 중력
    float slowedVerticalVelocity; // 느려진 점프 속도
    float slowedGravity; // 느려진 중력

    float gravity = -100f;
    float lastInitaialVerticalVelocity;
    [SerializeField] bool isGrounded;
    bool isInitialized;
    bool isFirstJump = true; // 첫 번째 점프인지 체크
    Animator anim;

    EnemyBase enemyBase;

    #region Update
    void Update()
    {
        if (isJumper == false) return; // 점프를 할 수 있는 캐릭터가 아니라면 아무것도 하지 않기
        if (enemy.isTimeStopped()) return; // 시간이 정지되어 있는 상태라면 아무것도 하지 않기
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
            sprRndshadow.sortingLayerName = "Shadow";
            isInitialized = true;
        }
        IsDone = false;

        ActivateCollider(true);

        isGrounded = false;
        currentVerticalVel = verticalVel;
        lastInitaialVerticalVelocity = verticalVel;

        anim = GetComponent<Animator>();
    }

    public void SetIsJumper(bool isJumper, float jumpInterval)
    {
        // Debug.LogError($"Is Jumper = {isJumper}");
        this.isJumper = isJumper;
        if (isJumper && jumpInterval == 0) this.isJumper = false; // 실수로 점프 가능이면서 인터벌이 0일 때는 그냥 점프불가로
        this.jumpFrequency = jumpInterval + UnityEngine.Random.Range(-1f, 1f);
        isFirstJump = true; // 점프 캐릭터로 설정될 때 첫 번째 점프 플래그 초기화
    }
    #endregion

    #region 매 프레임마다 할 일
    void CountJumpCounter()
    {
        if (isGrounded)
        {
            jumpCounter += Time.deltaTime;
            if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
            enemyBase.SetGrounded(isGrounded);
        }
        if (jumpCounter >= jumpFrequency)
        {
            jumpCounter = 0;

            // 점프 사운드 재생
            if (isFirstJump)
            {
                if (jumpSound != null) SoundManager.instance.Play(jumpSound);
                isFirstJump = false;
            }

            Initialize(verticalVelocity);

            // ⭐ 느림 상태 확인
            if (enemyBase.IsSlowed)
            {
                // 느림 상태일 때는 수평 속도를 증가시키지 않거나, 약간만 증가
                enemy.SpeedUpOnJump(-0.2f); // 원래 속도의 1.2배 (거의 그대로)
            }
            else
            {
                // 정상 상태일 때는 기존대로 2배
                enemy.SpeedUpOnJump(-1f);
            }

            ActivateCollider(false);
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

        if (landingSound != null) SoundManager.instance.Play(landingSound);
        if (landingEffectPrefab != null)
        {
            GameObject landingEffect = GameManager.instance.poolManager.GetMisc(landingEffectPrefab);
            landingEffect.transform.position = landingEffectTrans.position;
            landingEffect.transform.localScale = 10f * Vector2.one;
        }

        onGroundHitEvent?.Invoke();
    }
    #endregion

    #region Unity Event에 붙일 함수들
    public void Bounce(float divisionFactor)
    {
        if (bouncingNumbers < 1)
        {
            IsDone = true;
            enemy.ResumeEnemy();
            ActivateCollider(true);
            isFirstJump = true; // 점프가 완료되면 다음 점프 사이클을 위해 초기화

            onDone?.Invoke(); // 완전히 멈췄을 때의 이벤트. 예를 들어 시한 폭탄의 경우 폭발하기

            return;
        }
        Initialize(lastInitaialVerticalVelocity / divisionFactor);
        bouncingNumbers--;
    }

    public void ActivateCollider(bool ActivateCol)
    {
        enemy.ActivateCollider(ActivateCol);
    }
    public void TriggerAnim(string animation)
    {
        anim.SetTrigger(animation);
    }

    // 점프 사운드 재생 함수
    void PlayJumpSound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.Play(jumpSound);
        }
    }
    #endregion

    #region 느림 효과
    /// <summary>
    /// 점프에 느림 효과 적용 (수직 속도와 중력 모두 감소)
    /// </summary>
    public void ApplySlowToJump(float slownessFactor)
    {
        if (!isJumper) return; // 점프 캐릭터가 아니면 무시

        if (!isSlowed)
        {
            // 첫 적용 시 원래 값 저장
            originalVerticalVelocity = verticalVelocity;
            originalGravity = gravity;
        }

        isSlowed = true;

        // 중력 감소 (느린 느낌)
        float gravityFactor = (1f - slownessFactor * 1.2f);
        slowedGravity = originalGravity * gravityFactor;

        // 수직 속도는 제곱근보다 더 많이 감소 (낮은 점프)
        // 0.7을 곱해서 원래 높이의 약 70% 정도로 낮춤
        slowedVerticalVelocity = originalVerticalVelocity * Mathf.Sqrt(gravityFactor) * 0.7f;

        // 현재 진행 중인 점프에도 즉시 적용
        if (!isGrounded)
        {
            float currentRatio = currentVerticalVel / verticalVelocity;
            currentVerticalVel = slowedVerticalVelocity * currentRatio;
        }

        verticalVelocity = slowedVerticalVelocity;
        gravity = slowedGravity;

        Debug.Log($"[ShadowHeight] 💤 느리고 낮은 점프 - 수직속도: {originalVerticalVelocity:F1} → {slowedVerticalVelocity:F1}, 중력: {originalGravity:F1} → {slowedGravity:F1}");
    }

    /// <summary>
    /// 점프 느림 효과 해제
    /// </summary>
    public void ReleaseSlowFromJump()
    {
        if (!isJumper || !isSlowed) return;

        isSlowed = false;

        // 원래 값으로 복구
        verticalVelocity = originalVerticalVelocity;
        gravity = originalGravity;

        // 현재 진행 중인 점프에도 즉시 적용
        if (!isGrounded)
        {
            float currentRatio = currentVerticalVel / slowedVerticalVelocity;
            currentVerticalVel = originalVerticalVelocity * currentRatio;
        }

        Debug.Log($"[ShadowHeight] 점프 느림 해제 - 수직속도: {slowedVerticalVelocity} → {originalVerticalVelocity}, 중력: {slowedGravity} → {originalGravity}");
    }
    #endregion
}