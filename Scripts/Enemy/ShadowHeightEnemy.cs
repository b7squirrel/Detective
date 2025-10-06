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

    [SerializeField] Transform trnsBody; // 공중에 뜨는 스프라이트 오브젝트
    [SerializeField] SpriteRenderer sprRndBody; // 공중에 뜨는 오브젝트의 스프라이트
    [SerializeField] SpriteRenderer sprRndshadow; // 그림자 스프라이트. sorting order 변경을 위해

    [SerializeField] Enemy enemy; // 수평 속도를 가져오기 위해
    bool isJumper; // 점프를 하는 캐릭터인지 판별
    [SerializeField] float verticalVelocity; // 수직 점프 속도
    float jumpFrequency; // 점프를 하는 주기
    float currentVerticalVel;
    float jumpCounter; // 점프 주기를 재는 카운터

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
            jumpCounter += Time.deltaTime; // 점프를 하고 있지 않을 때만 카운터가 돌아간다
            if (enemyBase == null) enemyBase = GetComponent<EnemyBase>();
            enemyBase.SetGrounded(isGrounded);
        }
        if (jumpCounter >= jumpFrequency)
        {
            jumpCounter = 0;

            // 점프 사운드 재생 (첫 번째 점프이거나 바운스가 아닌 일반 점프일 때)
            if (isFirstJump)
            {
                if (jumpSound != null) SoundManager.instance.Play(jumpSound); //
                isFirstJump = false;
            }

            Initialize(verticalVelocity);
            Debug.Log("Jump 실행");
            enemy.CastSlownessToEnemy(-1f);
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

        SoundManager.instance.Play(landingSound);
        GameObject landingEffect = GameManager.instance.poolManager.GetMisc(landingEffectPrefab);
        landingEffect.transform.position = landingEffectTrans.position;
        landingEffect.transform.localScale = 10f * Vector2.one;
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
}