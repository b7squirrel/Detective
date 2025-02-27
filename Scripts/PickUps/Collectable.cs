using System.Collections;
using UnityEngine;

// 습득 가능한 오브젝트의 행동들 정의
public class Collectable : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float knockBackForce = 20f;
    public float lifeTime; 
    public bool IsFlyingToPlayer { get; private set; }
    [field : SerializeField] public bool IsGem{get; private set;}

    public GameObject pickupEffect;

    protected Vector2 target;
    protected bool isKnockBack;
    public bool IsHit { get; private set; }

    public Vector2 dir;
    public float delay = 0.05f;

    protected float lifeTimeCount;
    protected bool isDisappearing; // 코루틴 반복 실행 방지

    WallManager wallManager;
    Vector2 pastPos;

    [Header("Effect")]
    //[SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    [SerializeField] public AudioClip pickup;
    //Material initialMat;
    SpriteRenderer sr;
    GemManager gemManager;

    float accel = 50f;

    [Header("Drop Bounce")]
    protected Collider2D[] colliders;

    public void TempWhite()
    {
        //sr.material = whiteMaterial;
    }
    #region 유니티 콜백함수
    protected virtual void OnEnable()
    {
        IsFlyingToPlayer = false;
        IsHit = false;
        lifeTimeCount = lifeTime;

        colliders = GetComponents<Collider2D>();
        DeactivateColliders();
    }

    public void DeactivateColliders()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }
    // shadow height의 유니티 이벤트에서 참조
    public void ActivateColliders()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        //sr.material = initialMat;
    }
    protected void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        //initialMat = sr.material;
    }

    protected virtual void Update()
    {
        DieOnTime();
        DieOnWall();
    }
    #endregion

    #region 자력에 닿았을 때
    public virtual void OnHitMagnetField(Vector2 direction)
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            GetComponent<Animator>().SetTrigger("Init");
        }

        StartCoroutine(Antic(direction));
        //StartCoroutine(Blink()); // 반짝거리면서 antic 모션
    }

    IEnumerator Antic(Vector2 dir)
    {
        IsHit = true;
        GameObject effect = GameManager.instance.poolManager.GetMisc(pickupEffect);
        if (effect != null) effect.transform.position = transform.position;

        float anticTime = .3f;
        float currentSpeed = 15;
        while (true)
        {
            currentSpeed -= accel * Time.deltaTime;
            transform.position += new Vector3(currentSpeed * dir.x, currentSpeed * dir.y, 0) * Time.deltaTime;
            anticTime -= Time.deltaTime;
            if (anticTime < 0) break;
            yield return null;
        }

        yield return StartCoroutine(FlyToPlayer()); // antic이 끝나면 플레이어에게 날아가기 시작
    }
    IEnumerator Blink()
    {
        yield return new WaitForSeconds(.08f);
        //sr.material = whiteMaterial;
        yield return new WaitForSeconds(.08f);
        //sr.material = initialMat;
    }

    IEnumerator FlyToPlayer()
    {
        IsFlyingToPlayer = true;

        while (true)
        {
            transform.position =
            Vector2.MoveTowards(transform.position,
            GameManager.instance.player.transform.position,
            moveSpeed * Time.deltaTime);

            if ((transform.position - Player.instance.transform.position).sqrMagnitude <= .04f)
            {
                PickedUp();
                break;
            }
            yield return null;
        }
    }
    #endregion

    #region 목표물에 도달했을 때
    public void PickedUp()
    {
        StartCoroutine(PickUpCo());
    }

    IEnumerator PickUpCo()
    {
        while (true)
        {
            if ((transform.position - Player.instance.transform.position).sqrMagnitude < .04f)
            {
                HitPlayerFeedback();
                break;
            }
            yield return null;
        }
    }

    void HitPlayerFeedback()
    {
        Character character = Player.instance.GetComponent<Character>();

        GetComponent<IPickUpObject>().OnPickUp(character);

        if (gemManager == null) gemManager = GameManager.instance.GetComponent<GemManager>();
        gemManager.PlayGemSound();
        gameObject.SetActive(false);
    }
    #endregion

    #region 특정 조건에서 사라짐(시간 경과, 벽에 덮쳐짐)
    protected virtual void DieOnTime()
    {
        if (isDisappearing) return; // 코루틴이 실행 중이라면 Die 취소
        if (IsHit) return; // 자력에 닿아서 움직이는 도중이라면 사라짐 취소
        if (lifeTimeCount == 0) return; // 없어지지 않는 아이템이면 lifeTime을 0으로 하기로 약속

        if (lifeTimeCount > 0)
        {
            lifeTimeCount -= Time.deltaTime;
        }
        else
        {
            isDisappearing = true;
            StartCoroutine(TimesUpBlinking());
        }
    }

    IEnumerator TimesUpBlinking()
    {
        ShadowSprite shadow = GetComponentInChildren<ShadowSprite>();

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        //shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        //shadow.Show();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        //shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        //shadow.Show();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        //shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        //shadow.Show();
        if (!IsHit)
            isDisappearing = false;
            gameObject.SetActive(false);
    }

    void DieOnWall()
    {
        if (IsOutOfRange())
        {
            gameObject.SetActive(false);
        }
    }
    bool IsOutOfRange()
    {
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();

        return new Equation().IsOutOfRange(transform.position, spawnConst);
    }
    #endregion
}
